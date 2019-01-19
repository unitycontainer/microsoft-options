using Microsoft.Extensions.Options;

namespace Unity.Microsoft.Options
{
    public class OptionsFactory<TOptions> : IOptionsFactory<TOptions> 
        where TOptions : class, new()
    {
        IUnityContainer _container;

        public OptionsFactory(IUnityContainer container)
        {
            _container = container;
        }

        public TOptions Create(string name)
        {
            return _container.Resolve<TOptions>(name);
        }
    }
}
