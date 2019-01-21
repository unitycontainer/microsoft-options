using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Unity;
using Unity.Microsoft.Options;

namespace Tests.OptionsFactoryTest
{
    public partial class OptionsFactoryTest
    {
        [TestMethod]
        public void CreateSupportsNames()
        {
            // Setup
            Container.Configure<FakeOptions>("1", options => options.Message = "one");
            Container.Configure<FakeOptions>("2", options => options.Message = "two");

            // Act
            var factory = Container.Resolve<IOptionsFactory<FakeOptions>>();

            // Validate
            Assert.AreEqual("one", factory.Create("1").Message);
            Assert.AreEqual("two", factory.Create("2").Message);
        }

        [TestMethod]
        public void CanConfigureAllOptions()
        {
            // Setup
            Container.ConfigureAll<FakeOptions>(o => o.Message = "Default");

            // Act
            var factory = Container.Resolve<IOptionsFactory<FakeOptions>>();

            // Validate
            Assert.AreEqual("Default", factory.Create("1").Message);
            Assert.AreEqual("Default", factory.Create(Options.DefaultName).Message);
            Assert.AreEqual("Default", factory.Create("2").Message);
        }

        [TestMethod]
        public void PostConfiguresInOrderAfterConfigures()
        {
            // Setup
            Container.Configure<FakeOptions>("-", o => o.Message += "-");
            Container.ConfigureAll<FakeOptions>(o => o.Message += "[");
            Container.Configure<FakeOptions>("+", o => o.Message += "+");
            Container.PostConfigure<FakeOptions>("-", o => o.Message += "-");
            Container.PostConfigureAll<FakeOptions>(o => o.Message += "A");
            Container.PostConfigure<FakeOptions>("+", o => o.Message += "+");
            Container.PostConfigureAll<FakeOptions>(o => o.Message += "B");
            Container.PostConfigureAll<FakeOptions>(o => o.Message += "C");
            Container.PostConfigure<FakeOptions>("+", o => o.Message += "+");
            Container.PostConfigure<FakeOptions>("-", o => o.Message += "-");
            Container.Configure<FakeOptions>("+", o => o.Message += "+");
            Container.ConfigureAll<FakeOptions>(o => o.Message += "]");
            Container.Configure<FakeOptions>("-", o => o.Message += "-");

            // Act
            var factory = Container.Resolve<IOptionsFactory<FakeOptions>>();

            // Validate
            Assert.AreEqual("[]ABC", factory.Create("1").Message);
            Assert.AreEqual("[++]A+BC+", factory.Create("+").Message);
            Assert.AreEqual("-[]--ABC-", factory.Create("-").Message);
        }

        [TestMethod]
        public void CanConfigureAndPostConfigureAllOptions()
        {
            // Setup
            Container.ConfigureAll<FakeOptions>(o => o.Message = "D");
            Container.PostConfigureAll<FakeOptions>(o => o.Message += "f");
            Container.ConfigureAll<FakeOptions>(o => o.Message += "e");
            Container.PostConfigureAll<FakeOptions>(o => o.Message += "ault");

            // Act
            var factory = Container.Resolve<IOptionsFactory<FakeOptions>>();

            // Validate
            Assert.AreEqual("Default", factory.Create("1").Message);
            Assert.AreEqual("Default", factory.Create("2").Message);
        }

        [TestMethod]
        public void NamedSnapshotsConfiguresInRegistrationOrder()
        {
            // Setup
            Container.Configure<FakeOptions>("-", o => o.Message += "-");
            Container.ConfigureAll<FakeOptions>(o => o.Message += "A");
            Container.Configure<FakeOptions>("+", o => o.Message += "+");
            Container.ConfigureAll<FakeOptions>(o => o.Message += "B");
            Container.ConfigureAll<FakeOptions>(o => o.Message += "C");
            Container.Configure<FakeOptions>("+", o => o.Message += "+");
            Container.Configure<FakeOptions>("-", o => o.Message += "-");

            // Act
            var factory = Container.Resolve<IOptionsFactory<FakeOptions>>();

            // Validate
            Assert.AreEqual("ABC", factory.Create("1").Message);
            Assert.AreEqual("A+BC+", factory.Create("+").Message);
            Assert.AreEqual("-ABC-", factory.Create("-").Message);
        }

        [TestMethod]
        public void CanConfigureAllDefaultAndNamedOptions()
        {
            // Setup
            Container.ConfigureAll<FakeOptions>(o => o.Message += "Default");
            Container.Configure<FakeOptions>(o => o.Message += "0");
            Container.Configure<FakeOptions>("1", o => o.Message += "1");

            // Act
            var factory = Container.Resolve<IOptionsFactory<FakeOptions>>();

            // Validate
            Assert.AreEqual("Default", factory.Create("Default").Message);
            Assert.AreEqual("Default0", factory.Create(Options.DefaultName).Message);
            Assert.AreEqual("Default1", factory.Create("1").Message);
        }

        [TestMethod]
        public void CanConfigureAndPostConfigureAllDefaultAndNamedOptions()
        {
            // Setup
            Container.ConfigureAll<FakeOptions>(o => o.Message += "Default");
            Container.Configure<FakeOptions>(o => o.Message += "0");
            Container.Configure<FakeOptions>("1", o => o.Message += "1");
            Container.PostConfigureAll<FakeOptions>(o => o.Message += "PostConfigure");
            Container.PostConfigure<FakeOptions>(o => o.Message += "2");
            Container.PostConfigure<FakeOptions>("1", o => o.Message += "3");

            // Act
            var factory = Container.Resolve<IOptionsFactory<FakeOptions>>();

            // Validate
            Assert.AreEqual("DefaultPostConfigure", factory.Create("Default").Message);
            Assert.AreEqual("Default0PostConfigure2", factory.Create(Options.DefaultName).Message);
            Assert.AreEqual("Default1PostConfigure3", factory.Create("1").Message);
        }

        [TestMethod]
        public void CanPostConfigureAllOptions()
        {
            // Setup
            Container.PostConfigureAll<FakeOptions>(o => o.Message = "Default");

            // Act
            var factory = Container.Resolve<IOptionsFactory<FakeOptions>>();

            // Validate
            Assert.AreEqual("Default", factory.Create("1").Message);
            Assert.AreEqual("Default", factory.Create("2").Message);
        }

        [TestMethod]
        public void CanPostConfigureAllDefaultAndNamedOptions()
        {
            // Setup
            Container.PostConfigureAll<FakeOptions>(o => o.Message += "Default");
            Container.PostConfigure<FakeOptions>(o => o.Message += "0");
            Container.PostConfigure<FakeOptions>("1", o => o.Message += "1");

            // Act
            var factory = Container.Resolve<IOptionsFactory<FakeOptions>>();

            // Validate
            Assert.AreEqual("Default", factory.Create("Default").Message);
            Assert.AreEqual("Default0", factory.Create(Options.DefaultName).Message);
            Assert.AreEqual("Default1", factory.Create("1").Message);
        }

        [TestMethod]
        public void CanConfigureOptionsOnlyDefault()
        {
            // Setup
            Container.ConfigureOptions<FakeOptionsSetupA>();
            Container.ConfigureOptions(typeof(FakeOptionsSetupB));
            Container.ConfigureOptions(new ConfigureOptions<FakeOptions>(o => o.Message += "hi!"));

            // Act
            var factory = Container.Resolve<IOptionsFactory<FakeOptions>>();

            // Validate
            Assert.AreEqual("ABhi!", factory.Create(Options.DefaultName).Message);
            Assert.AreEqual("", factory.Create("anything").Message);
        }

        [TestMethod]
        public void CanConfigureTwoOptionsWithConfigureOptions()
        {
            // Setup
            Container.ConfigureOptions<UberBothSetup>();

            // Act
            var factory = Container.Resolve<IOptionsFactory<FakeOptions>>();
            var factory2 = Container.Resolve<IOptionsFactory<FakeOptions2>>();

            // Validate
            Assert.AreEqual("[]", factory.Create(Options.DefaultName).Message);
            Assert.AreEqual("[hao]", factory.Create("hao").Message);
            Assert.AreEqual("[[]]", factory2.Create(Options.DefaultName).Message);
            Assert.AreEqual("[[hao]]", factory2.Create("hao").Message);
        }

        [TestMethod]
        public void CanMixConfigureEverything()
        {
            // Setup
            Container.ConfigureAll<FakeOptions2>(o => o.Message = "!");
            Container.ConfigureOptions<UberBothSetup>();
            Container.Configure<FakeOptions>("#1", o => o.Message += "#");
            Container.PostConfigureAll<FakeOptions2>(o => o.Message += "|");
            Container.ConfigureOptions(new PostConfigureOptions<FakeOptions>("override", o => o.Message = "override"));
            Container.PostConfigure<FakeOptions>("end", o => o.Message += "_");

            // Act
            var factory = Container.Resolve<IOptionsFactory<FakeOptions>>();
            var factory2 = Container.Resolve<IOptionsFactory<FakeOptions2>>();

            // Validate
            Assert.AreEqual("[]", factory.Create(Options.DefaultName).Message);
            Assert.AreEqual("[hao]", factory.Create("hao").Message);
            Assert.AreEqual("[#1#]", factory.Create("#1").Message);
            Assert.AreEqual("![[#1]]|", factory2.Create("#1").Message);
            Assert.AreEqual("![[]]|", factory2.Create(Options.DefaultName).Message);
            Assert.AreEqual("![[hao]]|", factory2.Create("hao").Message);
            Assert.AreEqual("override", factory.Create("override").Message);
            Assert.AreEqual("![[override]]|", factory2.Create("override").Message);
            Assert.AreEqual("[end]_", factory.Create("end").Message);
            Assert.AreEqual("![[end]]|", factory2.Create("end").Message);
        }
    }
}
