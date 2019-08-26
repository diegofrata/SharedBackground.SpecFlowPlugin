using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gherkin.Ast;
using SharedBackground.SpecFlowPlugin.Grammars;
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
        internal static bool CanGenerate(SpecFlowDocument document) => document.SpecFlowFeature.Children.Any(CanGenerate);

        static bool CanGenerate(IHasLocation x)
        {
            var grammars = new IGrammar[] { new BackgroundGrammar(), new ScenarioGrammar(), new RedefinedGrammar() };

            bool IsMatch(string text) => grammars.Any(a => a.Match(text).HasValue);

            switch (x)
            {
                case Background background:
                    return IsMatch(background.Name ?? "");
                case StepsContainer stepsContainer:
                    return stepsContainer.Steps.Any(step => IsMatch(step.Text ?? ""));
                default:
                    return false;
            }
        }

        readonly SpecFlowConfiguration _generatorConfiguration;
        readonly ProjectSettings _projectSettings;
        readonly IGherkinParserFactory _gherkinParserFactory;
        readonly UnitTestFeatureGenerator _unitTestFeatureGenerator;
        readonly HashSet<(string Feature, string Scenario)> _redefinedScenarios = new HashSet<(string, string)>();

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
            var transformedChildren = document.SpecFlowFeature.Children
                .Select(x => x is StepsContainer stepsContainer ? TransformSteps(document.SourceFilePath, stepsContainer) : x);

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

        StepsContainer GetBackgroundDocument(string currentDocumentSourceFilePath, string text)
        {
            var grammars = new IGrammar[] { new BackgroundGrammar(), new ScenarioGrammar(), new RedefinedGrammar() };

            var match = grammars.Select(x => (Grammar: x, Result: x.Match(text))).FirstOrDefault(x => x.Result.HasValue);

            if (match == default)
                return null;

            var featureName = match.Result.Value.Feature;
            var scenarioName = match.Result.Value.Scenario;

            string featurePath;
            if (!string.IsNullOrEmpty(featureName))
            {
                if (!featureName.EndsWith(".feature", StringComparison.InvariantCultureIgnoreCase))
                    featureName += ".feature";

                featurePath = Path.Combine(Path.GetDirectoryName(currentDocumentSourceFilePath), featureName);
            }
            else
                featurePath = currentDocumentSourceFilePath;


            if (match.Grammar is RedefinedGrammar)
            {
                _redefinedScenarios.Add((featurePath.ToLowerInvariant(), scenarioName.ToLowerInvariant()));
                return new Scenario(default, default, default, default, default, new Step[0], default);
            }

            var isScenario = match.Grammar is ScenarioGrammar;

            if (isScenario && _redefinedScenarios.Contains((featurePath.ToLowerInvariant(), scenarioName.ToLowerInvariant())))
                return new Scenario(default, default, default, default, default, new Step[0], default);

            var backgroundDocument = GenerateTestFileCode(new FeatureFileInput(featurePath));

            var stepsContainer = isScenario
                ? backgroundDocument?.SpecFlowFeature.ScenarioDefinitions.FirstOrDefault(x =>
                    x.Name.Equals(scenarioName, StringComparison.InvariantCultureIgnoreCase))
                : backgroundDocument?.SpecFlowFeature?.Background;

            return CanGenerate(stepsContainer) ? TransformSteps(featurePath, stepsContainer) : stepsContainer;
        }

        StepsContainer TransformSteps(string currentDocumentSourceFilePath, StepsContainer hasLocation)
        {
            switch (hasLocation)
            {
                case Background background:
                {
                    var backgroundDocument = GetBackgroundDocument(currentDocumentSourceFilePath, background.Name ?? "");
                    return backgroundDocument ?? background;
                }

                case Scenario scenario:
                {
                    var transformedSteps = scenario.Steps.SelectMany(step =>
                    {
                        var backgroundDocument = GetBackgroundDocument(currentDocumentSourceFilePath, step.Text ?? "");
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

            return hasLocation;
        }

        SpecFlowDocument GenerateTestFileCode(FeatureFileInput featureFileInput)
        {
            var parser = _gherkinParserFactory.Create(_generatorConfiguration.FeatureLanguage);
            using (var contentReader = featureFileInput.GetFeatureFileContentReader(_projectSettings))
                return parser.Parse(contentReader, featureFileInput.GetFullPath(_projectSettings));
        }
    }
}