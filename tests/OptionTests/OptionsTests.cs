using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Unity;
using System.Collections.Generic;
using System.Linq;
using MEO = Microsoft.Extensions.Options.Options;
using Unity.Microsoft.Options;

namespace Tests.OptionTests
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
            Container.Configure<ComplexOptions>(new ConfigurationBuilder().AddInMemoryCollection(dic).Build());

            // Act
            var options = Container.Resolve<IOptions<ComplexOptions>>().Value;

            // Validate
            Assert.IsTrue(options.Boolean);
            Assert.AreEqual(-2, options.Integer);
            Assert.AreEqual(11, options.Nested.Integer);
        }

        [TestMethod]
        public void CanReadInheritedProperties()
        {
            // Setup
            var dic = new Dictionary<string, string>
            {
                {"Integer", "-2"},
                {"Boolean", "TRUe"},
                {"Nested:Integer", "11"},
                {"Virtual","Sup"}
            };
            Container.Configure<DerivedOptions>(new ConfigurationBuilder().AddInMemoryCollection(dic).Build());

            // Act
            var options = Container.Resolve<IOptions<DerivedOptions>>().Value;

            // Validate
            Assert.IsTrue(options.Boolean);
            Assert.AreEqual(-2, options.Integer);
            Assert.AreEqual(11, options.Nested.Integer);
            Assert.AreEqual("Derived:Sup", options.Virtual);
        }

        [TestMethod]
        public void CanReadStaticProperty()
        {
            // Setup
            var dic = new Dictionary<string, string>
            {
                {"StaticProperty", "stuff"},
            };
            Container.Configure<ComplexOptions>(new ConfigurationBuilder().AddInMemoryCollection(dic).Build());

            // Act
            var options = Container.Resolve<IOptions<ComplexOptions>>().Value;

            // Validate
            Assert.AreEqual("stuff", ComplexOptions.StaticProperty);
        }

        [DataTestMethod]
        [DataRow("ReadOnly")]
        [DataRow("PrivateSetter")]
        [DataRow("ProtectedSetter")]
        [DataRow("InternalSetter")]
        public void ShouldBeIgnoredTests(string property)
        {
            // Setup
            var dic = new Dictionary<string, string>
            {
                {property, "stuff"},
            };
            Container.Configure<ComplexOptions>(new ConfigurationBuilder().AddInMemoryCollection(dic).Build());

            // Act
            var options = Container.Resolve<IOptions<ComplexOptions>>().Value;

            // Validate
            Assert.IsNull(options.GetType().GetProperty(property).GetValue(options));
        }

        [DataTestMethod]
        [DataRow("PrivateSetter")]
        [DataRow("ProtectedSetter")]
        [DataRow("InternalSetter")]
        public void CanBindToNonPublicProperties(string property)
        {
            // Setup
            var dic = new Dictionary<string, string>
            {
                {property, "stuff"},
            };
            Container.Configure<ComplexOptions>(new ConfigurationBuilder().AddInMemoryCollection(dic).Build(), o => o.BindNonPublicProperties = true);

            // Act
            var options = Container.Resolve<IOptions<ComplexOptions>>().Value;

            // Validate
            Assert.AreEqual("stuff", options.GetType().GetProperty(property).GetValue(options));
        }

        [DataTestMethod]
        [DataRow("PrivateSetter")]
        [DataRow("ProtectedSetter")]
        [DataRow("InternalSetter")]
        public void CanNamedBindToNonPublicProperties(string property)
        {
            // Setup
            var dic = new Dictionary<string, string>
            {
                {property, "stuff"},
            };
            Container.Configure<ComplexOptions>("named", new ConfigurationBuilder().AddInMemoryCollection(dic).Build(), o => o.BindNonPublicProperties = true);

            // Act
            var options = Container.Resolve<IOptionsMonitor<ComplexOptions>>().Get("named");

            // Validate
            Assert.AreEqual("stuff", options.GetType().GetProperty(property).GetValue(options));
        }

        [TestMethod]
        public void SetupCallsInOrder()
        {
            // Setup
            var dic = new Dictionary<string, string>
            {
                {"Message", "!"},
            };
            var builder = new ConfigurationBuilder().AddInMemoryCollection(dic);
            var config = builder.Build();
            Container.Configure<FakeOptions>(o => o.Message += "Igetstomped");
            Container.Configure<FakeOptions>(config);
            Container.Configure<FakeOptions>(o => o.Message += "a");
            Container.Configure<FakeOptions>(o => o.Message += "z");

            // Act
            var service = Container.Resolve<IOptions<FakeOptions>>();

            // Validate
            Assert.IsNotNull(service);
            var options = service.Value;
            Assert.IsNotNull(options);
            Assert.AreEqual("!az", options.Message);
        }

        [TestMethod]
        public void PostConfiguresInRegistrationOrderAfterConfigures()
        {
            // Setup
            Container.Configure<FakeOptions>(o => o.Message += "_");
            Container.PostConfigure<FakeOptions>(o => o.Message += "A");
            Container.PostConfigure<FakeOptions>(o => o.Message += "B");
            Container.PostConfigure<FakeOptions>(o => o.Message += "C");
            Container.Configure<FakeOptions>(o => o.Message += "-");

            // Act
            var option = Container.Resolve<IOptions<FakeOptions>>().Value;

            // Validate
            Assert.AreEqual("_-ABC", option.Message);
        }

        [DataTestMethod]
        [DynamicData(nameof(Configure_GetsNullableOptionsFromConfiguration_Data))]
        public void Configure_GetsNullableOptionsFromConfiguration(IDictionary<string, string> configValues,
                                                                   IDictionary<string, object> expectedValues)
        {
            // Arrange
            var builder = new ConfigurationBuilder().AddInMemoryCollection(configValues);
            var config = builder.Build();
            Container.Configure<NullableOptions>(config);

            // Act
            var options = Container.Resolve<IOptions<NullableOptions>>().Value;

            // Validate
            var optionsProps = options.GetType().GetProperties().ToDictionary(p => p.Name);
            var assertions = expectedValues
                .Select(_ => new Action<KeyValuePair<string, object>>(kvp => Assert.AreEqual(kvp.Value, optionsProps[kvp.Key].GetValue(options))))
                .ToArray();

            var pairs = expectedValues.ToArray();
            for (var i = 0; i < assertions.Length; i++)
            {
                var pair = pairs[i];
                var assertion = assertions[i];
                assertion(pair);
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(Configure_GetsEnumOptionsFromConfiguration_Data))]
        public void Configure_GetsEnumOptionsFromConfiguration(
            IDictionary<string, string> configValues,
            IDictionary<string, object> expectedValues)
        {
            // Setup
            var builder = new ConfigurationBuilder().AddInMemoryCollection(configValues);
            var config = builder.Build();
            Container.Configure<EnumOptions>(config);

            // Act
            var options = Container.Resolve<IOptions<EnumOptions>>().Value;

            // Validate
            var optionsProps = options.GetType().GetProperties().ToDictionary(p => p.Name);
            var assertions = expectedValues
                .Select(_ => new Action<KeyValuePair<string, object>>(kvp =>Assert.AreEqual(kvp.Value, optionsProps[kvp.Key].GetValue(options))))
                .ToArray();

            var pairs = expectedValues.ToArray();
            for (var i = 0; i < assertions.Length; i++)
            {
                var pair = pairs[i];
                var assertion = assertions[i];
                assertion(pair);
            }
        }


        [TestMethod]
        public void Options_StaticCreateCreateMakesOptions()
        {
            var options = MEO.Create(new FakeOptions
            {
                Message = "This is a message"
            });

            Assert.AreEqual("This is a message", options.Value.Message);
        }

        [TestMethod]
        public void OptionsWrapper_MakesOptions()
        {
            var options = new OptionsWrapper<FakeOptions>(new FakeOptions
            {
                Message = "This is a message"
            });

            Assert.AreEqual("This is a message", options.Value.Message);
        }

        [TestMethod]
        public void Options_CanOverrideForSpecificTOptions()
        {
            Container.Configure<FakeOptions>(options =>
            {
                options.Message = "Initial value";
            });

            Container.RegisterInstance(MEO.Create(new FakeOptions
            {
                Message = "Override"
            }));

            Assert.AreEqual("Override", Container.Resolve<IOptions<FakeOptions>>().Value.Message);
        }
    }
}