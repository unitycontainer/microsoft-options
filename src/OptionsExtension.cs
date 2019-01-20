using Microsoft.Extensions.Options;
using Unity.Extension;

namespace Unity.Microsoft.Options
{
    public class OptionsExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType(typeof(IOptions<>),             typeof(OptionsManager<>), UnityContainer.All, TypeLifetime.Singleton);
            Container.RegisterType(typeof(IOptionsSnapshot<>    ), typeof(OptionsManager<>), UnityContainer.All, TypeLifetime.Scoped   );
            Container.RegisterType(typeof(IOptionsMonitor<>     ), typeof(OptionsMonitor<>), UnityContainer.All, TypeLifetime.Singleton);
            Container.RegisterType(typeof(IOptionsFactory<>     ), typeof(OptionsFactory<>), UnityContainer.All, TypeLifetime.Transient);
            Container.RegisterType(typeof(IOptionsMonitorCache<>), typeof(OptionsCache<>  ), UnityContainer.All, TypeLifetime.Singleton);
        }
    }
}
