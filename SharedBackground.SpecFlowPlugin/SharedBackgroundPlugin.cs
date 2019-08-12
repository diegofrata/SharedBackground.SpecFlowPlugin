using SharedBackground.SpecFlowPlugin;
using TechTalk.SpecFlow.Generator.Plugins;
using TechTalk.SpecFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.UnitTestProvider;

[assembly: GeneratorPlugin(typeof(SharedBackgroundGeneratorPlugin))]

namespace SharedBackground.SpecFlowPlugin
{
    public class SharedBackgroundGeneratorPlugin : IGeneratorPlugin
    {
        public void Initialize(
            GeneratorPluginEvents generatorPluginEvents,
            GeneratorPluginParameters generatorPluginParameters,
            UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            generatorPluginEvents.RegisterDependencies += (sender, args) =>
            {
                args.ObjectContainer.RegisterTypeAs<SharedBackgroundFeatureGeneratorProvider, IFeatureGeneratorProvider>();
//                args.ObjectContainer.RegisterTypeAs<SharedBackgroundFeatureGenerator, IFeatureGenerator>("default");
            };
        }
    }
}