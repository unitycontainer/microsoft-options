using Microsoft.Extensions.Options;
using System;
using MEO = Microsoft.Extensions.Options.Options;

namespace Unity
{
    public static class OptionsValidationExtension
    {
        /// <summary>
        /// Register a validation action for an options type using a default failure message..
        /// </summary>
        /// <param name="validation">The validation function.</param>
        /// <returns>The current OptionsBuilder.</returns>
        public static IUnityContainer Validate<TOptions>(this IUnityContainer container, Func<TOptions, bool> validation)
            where TOptions : class => Validate(container, validation: validation, failureMessage: "A validation error has occured.");

        /// <summary>
        /// Register a validation action for an options type using a default failure message..
        /// </summary>
        /// <param name="validation">The validation function.</param>
        /// <returns>The current OptionsBuilder.</returns>
        public static IUnityContainer Validate<TOptions>(this IUnityContainer container, string name, Func<TOptions, bool> validation)
            where TOptions : class => Validate(container, name, validation, "A validation error has occured.");

        /// <summary>
        /// Register a validation action for an options type.
        /// </summary>
        /// <param name="validation">The validation function.</param>
        /// <param name="failureMessage">The failure message to use when validation fails.</param>
        /// <returns>The current OptionsBuilder.</returns>
        public static IUnityContainer Validate<TOptions>(this IUnityContainer container, Func<TOptions, bool> validation, string failureMessage)
            where TOptions : class => Validate(container, MEO.DefaultName, validation, failureMessage);

        /// <summary>
        /// Register a validation action for an options type.
        /// </summary>
        /// <param name="validation">The validation function.</param>
        /// <param name="failureMessage">The failure message to use when validation fails.</param>
        /// <returns>The current OptionsBuilder.</returns>
        public static IUnityContainer Validate<TOptions>(this IUnityContainer container, string name, Func<TOptions, bool> validation, string failureMessage)
            where TOptions : class
        {
            if (validation == null)
            {
                throw new ArgumentNullException(nameof(validation));
            }

            container.RegisterInstance<IValidateOptions<TOptions>>(Guid.NewGuid().ToString(),
                new ValidateOptions<TOptions>(name, validation, failureMessage));

            return container;
        }
    }
}
