using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using MEO = Microsoft.Extensions.Options.Options;

namespace Unity
{
    /// <summary>
    /// Extension methods for adding configuration related options services to the DI container.
    /// </summary>
    public static class OptionsConfigurationExtensions
    {
        /// <summary>
        /// Registers a configuration instance which TOptions will bind against.
        /// </summary>
        /// <typeparam name="TOptions">The type of options being configured.</typeparam>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <param name="config">The configuration being bound.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer Configure<TOptions>(this IUnityContainer container, IConfiguration config) where TOptions : class
            => container.Configure<TOptions>(MEO.DefaultName, config);

        /// <summary>
        /// Registers a configuration instance which TOptions will bind against.
        /// </summary>
        /// <typeparam name="TOptions">The type of options being configured.</typeparam>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <param name="name">The name of the options instance.</param>
        /// <param name="config">The configuration being bound.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer Configure<TOptions>(this IUnityContainer container, string name, IConfiguration config) where TOptions : class
            => container.Configure<TOptions>(name, config, _ => { });

        /// <summary>
        /// Registers a configuration instance which TOptions will bind against.
        /// </summary>
        /// <typeparam name="TOptions">The type of options being configured.</typeparam>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <param name="config">The configuration being bound.</param>
        /// <param name="configureBinder">Used to configure the <see cref="BinderOptions"/>.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer Configure<TOptions>(this IUnityContainer container, IConfiguration config, Action<BinderOptions> configureBinder)
            where TOptions : class
            => container.Configure<TOptions>(MEO.DefaultName, config, configureBinder);

        /// <summary>
        /// Registers a configuration instance which TOptions will bind against.
        /// </summary>
        /// <typeparam name="TOptions">The type of options being configured.</typeparam>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <param name="name">The name of the options instance.</param>
        /// <param name="config">The configuration being bound.</param>
        /// <param name="configureBinder">Used to configure the <see cref="BinderOptions"/>.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer Configure<TOptions>(this IUnityContainer container, string name, IConfiguration config, Action<BinderOptions> configureBinder)
            where TOptions : class
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            container.RegisterInstance<IOptionsChangeTokenSource<TOptions>>(Guid.NewGuid().ToString(), new ConfigurationChangeTokenSource<TOptions>(name, config));
            container.RegisterInstance<IConfigureOptions<TOptions>>(Guid.NewGuid().ToString(), new NamedConfigureFromConfigurationOptions<TOptions>(name, config, configureBinder));

            return container;
        }
    }
}
