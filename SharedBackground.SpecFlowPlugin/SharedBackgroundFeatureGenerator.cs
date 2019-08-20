using System;
using System.CodeDom;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Gherkin.Ast;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.CodeDom;
using TechTalk.SpecFlow.Generator.Generation;
using TechTalk.SpecFlow.Generator.Interfaces;
using TechTalk.SpecFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Parser;

namespace SharedBackground.SpecFlowPlugin
{
    public class SharedBackgroundFeatureGenerator : IFeatureGenerator
    {
        static readonly Regex BackgroundRegex = new Regex("the background steps (?:of|in) '(?<Feature>.+)' have been executed");
        static readonly Regex ScenarioRegex = new Regex("the scenario '(?<Scenario>.+?)' (?:(?:of|in) '(?<Feature>.+?)' )?has been executed");

        internal static bool CanGenerate(SpecFlowDocument document)
        {
            var canGenerate = document.SpecFlowFeature.Children
                .Any(x =>
                {
                    var patterns = new[] { BackgroundRegex, ScenarioRegex };

                    bool IsMatch(string text) => patterns.Any(a => a.IsMatch(text));
                    
                    switch (x)
                    {
                        case Background background:
                            return IsMatch(background.Name ?? "");
                        case StepsContainer stepsContainer:
                            return stepsContainer.Steps.Any(step => IsMatch(step.Text ?? ""));
                        default:
                            return false;
                    }
                });
            
            return canGenerate;
        }

        readonly SpecFlowConfiguration _generatorConfiguration;
        readonly ProjectSettings _projectSettings;
        readonly IGherkinParserFactory _gherkinParserFactory;
        readonly UnitTestFeatureGenerator _unitTestFeatureGenerator;

        public SharedBackgroundFeatureGenerator(
            IUnitTestGeneratorProvider testGeneratorProvider,
            CodeDomHelper codeDomHelper,
            SpecFlowConfiguration generatorConfiguration,
            IDecoratorRegistry decoratorRegistry,
            ProjectSettings projectSettings,
            IGherkinParserFactory gherkinParserFactory)
        {
            _generatorConfiguration = generatorConfiguration;
            _projectSettings = projectSettings;
            _gherkinParserFactory = gherkinParserFactory;
            _unitTestFeatureGenerator = new UnitTestFeatureGenerator(testGeneratorProvider, codeDomHelper, generatorConfiguration, decoratorRegistry);
        }

        public CodeNamespace GenerateUnitTestFixture(SpecFlowDocument document, string testClassName, string targetNamespace)
        {
            StepsContainer GetBackgroundDocument(string text)
            {
                var patterns = new[] { BackgroundRegex, ScenarioRegex };

                var match = patterns.Select(x => (Regex: x, Match: x.Match(text))).FirstOrDefault(x => x.Match.Success);

                if (match == default)
                    return null;
                
                var fileName = match.Match.Groups["Feature"].Value;

                string filePath;
                if (!string.IsNullOrEmpty(fileName))
                {

                    if (!fileName.EndsWith(".feature", StringComparison.InvariantCultureIgnoreCase))
                        fileName += ".feature";

                    filePath = Path.Combine(Path.GetDirectoryName(document.SourceFilePath), fileName);
                }
                else
                    filePath = document.SourceFilePath;

                var backgroundDocument = GenerateTestFileCode(new FeatureFileInput(filePath));

                var hasScenario = ReferenceEquals(match.Regex, ScenarioRegex); 

                return hasScenario 
                    ? backgroundDocument?.SpecFlowFeature.ScenarioDefinitions.FirstOrDefault(x => x.Name == match.Match.Groups["Scenario"].Value) 
                    : backgroundDocument?.SpecFlowFeature?.Background;
            }
            
            var transformedChildren = document.SpecFlowFeature.Children.Select(x =>
            {
                switch (x)
                {
                    case Background background:
                    {
                        var backgroundDocument = GetBackgroundDocument(background.Name ?? "");
                        return backgroundDocument ?? background;
                    }
                    case Scenario scenario:
                    {
                        var transformedSteps = scenario.Steps.SelectMany(step =>
                        {
                            var backgroundDocument = GetBackgroundDocument(step.Text ?? "");
                            return backgroundDocument?.Steps ?? new[] { step };
                        });

                        if (scenario is ScenarioOutline scenarioOutline)
                        {
                            return new ScenarioOutline(
                                scenarioOutline.Tags.ToArray(),
                                scenarioOutline.Location,
                                scenarioOutline.Keyword,
                                scenarioOutline.Name,
                                scenarioOutline.Description,
                                transformedSteps.ToArray(),
                                scenarioOutline.Examples.ToArray()
                            );
                        } 
                        
                        return new Scenario(
                            scenario.Tags.ToArray(),
                            scenario.Location,
                            scenario.Keyword,
                            scenario.Name,
                            scenario.Description,
                            transformedSteps.ToArray(),
                            scenario.Examples.ToArray()
                        );
                    }
                }
                
                return x;
            });

            var clonedDocument = new SpecFlowDocument(
                new SpecFlowFeature(
                    document.SpecFlowFeature.Tags.ToArray(),
                    document.SpecFlowFeature.Location,
                    document.SpecFlowFeature.Language,
                    document.SpecFlowFeature.Keyword,
                    document.SpecFlowFeature.Name,
                    document.SpecFlowFeature.Description,
                    transformedChildren.ToArray()
                ),
                document.Comments.ToArray(),
                document.SourceFilePath
            );

            return CanGenerate(clonedDocument) 
                ? GenerateUnitTestFixture(clonedDocument, testClassName, targetNamespace) 
                : _unitTestFeatureGenerator.GenerateUnitTestFixture(clonedDocument, testClassName, targetNamespace);
        }

        SpecFlowDocument GenerateTestFileCode(FeatureFileInput featureFileInput)
        {
            var parser = _gherkinParserFactory.Create(_generatorConfiguration.FeatureLanguage);
            using (var contentReader = featureFileInput.GetFeatureFileContentReader(_projectSettings))
                return parser.Parse(contentReader, featureFileInput.GetFullPath(_projectSettings));
        }
    }
}