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
        static readonly Regex FileRegex = new Regex("file:(.+)");
        static readonly Regex StepRegex = new Regex("the background steps (?:of|in) '(.+)' have been executed");

        internal static bool CanGenerate(SpecFlowDocument document)
        {
            var canGenerate = document.SpecFlowFeature.Children
                .Any(x =>
                {
                    switch (x)
                    {
                        case Background background:
                            return FileRegex.IsMatch(background.Name ?? "");
                        case StepsContainer stepsContainer:
                            return stepsContainer.Steps.Any(step => StepRegex.IsMatch(step.Text ?? ""));
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
            SpecFlowDocument GetBackgroundDocument(Regex regex, string text)
            {
                var match = regex.Match(text);
                
                if (!match.Success) 
                    return null;
                
                var filePath =  Path.Combine(Path.GetDirectoryName(document.SourceFilePath), match.Groups[1].Value);
                var backgroundDocument = GenerateTestFileCode(new FeatureFileInput(filePath));
                return backgroundDocument;  
            }
            
            var transformedChildren = document.SpecFlowFeature.Children.Select(x =>
            {
                switch (x)
                {
                    case Background background:
                    {
                        var backgroundDocument = GetBackgroundDocument(FileRegex, background.Name ?? "");
                        return backgroundDocument?.SpecFlowFeature?.Background ?? background;
                    }
                    case Scenario scenario:
                    {
                        var transformedSteps = scenario.Steps.SelectMany(step =>
                        {
                            var backgroundDocument = GetBackgroundDocument(StepRegex, step.Text ?? "");
                            return backgroundDocument?.SpecFlowFeature?.Background?.Steps ?? new[] { step };
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

            return _unitTestFeatureGenerator.GenerateUnitTestFixture(clonedDocument, testClassName, targetNamespace);
        }

        SpecFlowDocument GenerateTestFileCode(FeatureFileInput featureFileInput)
        {
            var parser = _gherkinParserFactory.Create(_generatorConfiguration.FeatureLanguage);
            using (var contentReader = featureFileInput.GetFeatureFileContentReader(_projectSettings))
                return parser.Parse(contentReader, featureFileInput.GetFullPath(_projectSettings));
        }
    }
}