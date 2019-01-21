using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Unity;
using Unity.Microsoft.Options;

namespace Tests.OptionsValidationTest
{
    [TestClass]
    public partial class OptionsValidationTest
    {
        protected IUnityContainer Container;

        [TestInitialize]
        public void Setup()
        {
            Container = new UnityContainer()
                .AddExtension(new OptionsExtension())
                .AddExtension(new Diagnostic());
        }

    }


    #region Test Data

    public class ComplexOptions
    {
        public ComplexOptions()
        {
            Nested = new NestedOptions();
            Virtual = "complex";
        }
        public NestedOptions Nested { get; set; }
        public int Integer { get; set; }
        public bool Boolean { get; set; }
        public virtual string Virtual { get; set; }

        public string PrivateSetter { get; private set; }
        public string ProtectedSetter { get; protected set; }
        public string InternalSetter { get; internal set; }
        public static string StaticProperty { get; set; }

        public string ReadOnly
        {
            get { return null; }
        }
    }

    public class NestedOptions
    {
        public int Integer { get; set; }
    }

    public class DerivedOptions : ComplexOptions
    {
        public override string Virtual
        {
            get
            {
                return base.Virtual;
            }
            set
            {
                base.Virtual = "Derived:" + value;
            }
        }
    }

    public class NullableOptions
    {
        public bool? MyNullableBool { get; set; }
        public int? MyNullableInt { get; set; }
        public DateTime? MyNullableDateTime { get; set; }
    }

    public class EnumOptions
    {
        public UriKind UriKind { get; set; }
    }

    public class UberBothSetup : IConfigureNamedOptions<FakeOptions>, IConfigureNamedOptions<FakeOptions2>, IPostConfigureOptions<FakeOptions>, IPostConfigureOptions<FakeOptions2>
    {
        public void Configure(string name, FakeOptions options)
            => options.Message += "[" + name;

        public void Configure(FakeOptions options) => Configure(Options.DefaultName, options);

        public void Configure(string name, FakeOptions2 options)
            => options.Message += "[[" + name;

        public void Configure(FakeOptions2 options) => Configure(Options.DefaultName, options);

        public void PostConfigure(string name, FakeOptions2 options)
            => options.Message += "]]";

        public void PostConfigure(string name, FakeOptions options)
            => options.Message += "]";
    }

    public class FakeOptionsSetupA : ConfigureOptions<FakeOptions>
    {
        public FakeOptionsSetupA() : base(o => o.Message += "A") { }
    }

    public class FakeOptionsSetupB : ConfigureOptions<FakeOptions>
    {
        public FakeOptionsSetupB() : base(o => o.Message += "B") { }
    }

    public class FakeOptionsFactory : IOptionsFactory<FakeOptions>
    {
        public static FakeOptions Options = new FakeOptions();

        public FakeOptions Create(string name) => Options;
    }

    public class FakeOptions
    {
        public FakeOptions()
        {
            Message = "";
        }

        public string Message { get; set; }
    }

    public class FakeOptions2 : FakeOptions { }

    #endregion
}
