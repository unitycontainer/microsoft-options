using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Unity.Microsoft.Options.Tests.OptionTests
{
    public partial class OptionTests
    {
        [TestMethod]
        public void UsesFactory()
        {
            // Setup
            Container.RegisterType<IOptionsFactory<FakeOptions>, FakeOptionsFactory>(TypeLifetime.PerContainer);
            Container.Configure<FakeOptions>(o => o.Message = "Ignored");

            // Act
            var snap = Container.Resolve<IOptions<FakeOptions>>();

            // Validate
            Assert.AreSame(FakeOptionsFactory.Options, snap.Value);
            Assert.AreEqual(FakeOptionsFactory.Options, snap.Value);
        }

        [TestMethod]
        public void CanReadComplexProperties()
        {
            // Setup
            var dic = new Dictionary<string, string>
            {
                {"Integer", "-2"},
                {"Boolean", "TRUe"},
                {"Nested:Integer", "11"}
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();


            Container.RegisterInstance<IOptionsChangeTokenSource<ComplexOptions>>(new ConfigurationChangeTokenSource<ComplexOptions>(string.Empty, config));
            Container.RegisterInstance<IConfigureOptions<ComplexOptions>>(new NamedConfigureFromConfigurationOptions<ComplexOptions>(string.Empty, config, (_) => { }));

            var options = Container.Resolve<IOptions<ComplexOptions>>().Value;

            Assert.IsTrue(options.Boolean);
            Assert.AreEqual(-2, options.Integer);
            Assert.AreEqual(11, options.Nested.Integer);
        }
    }
}