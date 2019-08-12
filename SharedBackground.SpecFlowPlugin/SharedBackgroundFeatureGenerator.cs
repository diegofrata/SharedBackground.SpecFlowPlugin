using System;
using System.CodeDom;
using System.Linq;
using System.Text.RegularExpressions;
using Gherkin.Ast;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.CodeDom;
using TechTalk.SpecFlow.Generator.Interfaces;
using TechTalk.SpecFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Parser;

namespace SharedBackground.SpecFlowPlugin
{
    public class SharedBackgroundFeatureGenerator : IFeatureGenerator
    {
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
            var filePath = Regex.Match(document.SpecFlowFeature.Background.Description, "file:(.+)").Groups[1].Value;

            var backgroundDocument = GenerateTestFileCode(new FeatureFileInput(filePath));

            var background = backgroundDocument.SpecFlowFeature.Background;

            var clonedDocument = new SpecFlowDocument(
                new SpecFlowFeature(
                    document.SpecFlowFeature.Tags.ToArray(),
                    document.SpecFlowFeature.Location,
                    document.SpecFlowFeature.Language,
                    document.SpecFlowFeature.Keyword,
                    document.SpecFlowFeature.Name,
                    document.SpecFlowFeature.Description,
                    document.SpecFlowFeature.Children.Where(x => !(x is Background)).Append(background).ToArray()
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