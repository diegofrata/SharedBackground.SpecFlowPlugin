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

        public int Priority => PriorityValues.Normal;
        public bool CanGenerate(SpecFlowDocument document) => SharedBackgroundFeatureGenerator.CanGenerate(document);
        public IFeatureGenerator CreateGenerator(SpecFlowDocument document) => _container.Resolve<SharedBackgroundFeatureGenerator>();
    }
}