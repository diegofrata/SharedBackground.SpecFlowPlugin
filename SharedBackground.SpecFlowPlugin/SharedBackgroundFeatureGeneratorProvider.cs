using System;
using System.Text.RegularExpressions;
using BoDi;
using TechTalk.SpecFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Parser;

namespace SharedBackground.SpecFlowPlugin
{
    public class SharedBackgroundFeatureGeneratorProvider : IFeatureGeneratorProvider
    {
        readonly IObjectContainer _container;

        public SharedBackgroundFeatureGeneratorProvider(IObjectContainer container)
        {
            _container = container;
        }

        public int Priority => int.MaxValue;

        public bool CanGenerate(SpecFlowDocument document)
        {
            return false;
            var description = document.SpecFlowFeature.Background?.Description;
            return description != null && Regex.IsMatch(description, "file:(.+)");
        }

        public IFeatureGenerator CreateGenerator(SpecFlowDocument document)
        {
            Console.WriteLine("TEsSTTTT");
            return _container.Resolve<SharedBackgroundFeatureGenerator>();
        }
    }
}