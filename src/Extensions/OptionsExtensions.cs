using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MEO = Microsoft.Extensions.Options.Options;

namespace Unity
{
    /// <summary>
    /// Extension methods for adding options services to the DI container.
    /// </summary>
    public static class OptionsServiceCollectionExtensions
    {
        /// <summary>
        /// Registers an action used to configure a particular type of options.
        /// Note: These are run before all <seealso cref="PostConfigure{TOptions}(IUnityContainer, Action{TOptions})"/>.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer Configure<TOptions>(this IUnityContainer container, Action<TOptions> configureOptions) where TOptions : class
            => container.Configure(MEO.DefaultName, configureOptions);

        /// <summary>
        /// Registers an action used to configure a particular type of options.
        /// Note: These are run before all <seealso cref="PostConfigure{TOptions}(IUnityContainer, Action{TOptions})"/>.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <param name="name">The name of the options instance.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer Configure<TOptions>(this IUnityContainer container, string name, Action<TOptions> configureOptions)
            where TOptions : class
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            var value = new ConfigureNamedOptions<TOptions>(name, configureOptions);
            container.RegisterInstance<IConfigureOptions<TOptions>>(Guid.NewGuid().ToString(), value);
            return container;
        }

        /// <summary>
        /// Registers an action used to configure all instances of a particular type of options.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer ConfigureAll<TOptions>(this IUnityContainer container, Action<TOptions> configureOptions) where TOptions : class
            => container.Configure(name: null, configureOptions: configureOptions);

        /// <summary>
        /// Registers an action used to initialize a particular type of options.
        /// Note: These are run after all <seealso cref="Configure{TOptions}(IUnityContainer, Action{TOptions})"/>.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer PostConfigure<TOptions>(this IUnityContainer container, Action<TOptions> configureOptions) where TOptions : class
            => container.PostConfigure(MEO.DefaultName, configureOptions);

        /// <summary>
        /// Registers an action used to configure a particular type of options.
        /// Note: These are run after all <seealso cref="Configure{TOptions}(IUnityContainer, Action{TOptions})"/>.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configure.</typeparam>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <param name="name">The name of the options instance.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer PostConfigure<TOptions>(this IUnityContainer container, string name, Action<TOptions> configureOptions)
            where TOptions : class
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            container.RegisterInstance<IPostConfigureOptions<TOptions>>(Guid.NewGuid().ToString(), new PostConfigureOptions<TOptions>(name, configureOptions));
            return container;
        }

        /// <summary>
        /// Registers an action used to post configure all instances of a particular type of options.
        /// Note: These are run after all <seealso cref="Configure{TOptions}(IUnityContainer, Action{TOptions})"/>.
        /// </summary>
        /// <typeparam name="TOptions">The options type to be configured.</typeparam>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <param name="configureOptions">The action used to configure the options.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer PostConfigureAll<TOptions>(this IUnityContainer container, Action<TOptions> configureOptions) where TOptions : class
            => container.PostConfigure(name: null, configureOptions: configureOptions);

        /// <summary>
        /// Registers a type that will have all of its I[Post]ConfigureOptions registered.
        /// </summary>
        /// <typeparam name="TConfigureOptions">The type that will configure options.</typeparam>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer ConfigureOptions<TConfigureOptions>(this IUnityContainer container) where TConfigureOptions : class
            => container.ConfigureOptions(typeof(TConfigureOptions));

        private static bool IsAction(Type type)
            => (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Action<>));

        private static IEnumerable<Type> FindIConfigureOptions(Type type)
        {
            var serviceTypes = type.GetTypeInfo().ImplementedInterfaces
                .Where(t => t.GetTypeInfo().IsGenericType &&
                (t.GetGenericTypeDefinition() == typeof(IConfigureOptions<>)
                || t.GetGenericTypeDefinition() == typeof(IPostConfigureOptions<>)));
            if (!serviceTypes.Any())
            {
                throw new InvalidOperationException(
                    IsAction(type)
                    ? "Error: No IConfigure Options And Action"
                    : "Error: No IConfigure Options");
            }
            return serviceTypes;
        }

        /// <summary>
        /// Registers a type that will have all of its I[Post]ConfigureOptions registered.
        /// </summary>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <param name="configureType">The type that will configure options.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer ConfigureOptions(this IUnityContainer container, Type configureType)
        {
            var serviceTypes = FindIConfigureOptions(configureType);
            foreach (var serviceType in serviceTypes)
            {
                container.RegisterType(serviceType, configureType, Guid.NewGuid().ToString());
            }
            return container;
        }

        /// <summary>
        /// Registers an object that will have all of its I[Post]ConfigureOptions registered.
        /// </summary>
        /// <param name="container">The <see cref="IUnityContainer"/> to add the services to.</param>
        /// <param name="configureInstance">The instance that will configure options.</param>
        /// <returns>The <see cref="IUnityContainer"/> so that additional calls can be chained.</returns>
        public static IUnityContainer ConfigureOptions(this IUnityContainer container, object configureInstance)
        {
            var serviceTypes = FindIConfigureOptions(configureInstance.GetType());
            foreach (var serviceType in serviceTypes)
            {
                container.RegisterInstance(serviceType, Guid.NewGuid().ToString(), configureInstance);
            }
            return container;
        }
    }
}
