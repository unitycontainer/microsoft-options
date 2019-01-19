using Microsoft.Extensions.Options;
using Unity.Extension;
using Unity.Lifetime;

namespace Unity.Microsoft.Options
{
    public class OptionsExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType(typeof(IOptionsFactory<>), typeof(OptionsFactory<>), UnityContainer.All, new HierarchicalLifetimeManager());
        }
    }
}
