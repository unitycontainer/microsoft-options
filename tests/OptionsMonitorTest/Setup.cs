using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Unity;
using Unity.Microsoft.Options;

namespace Tests.OptionsMonitorTest
{
    [TestClass]
    public partial class OptionsMonitorTest
    {
        protected IUnityContainer Container;

        public int SetupInvokeCount { get; set; }

        [TestInitialize]
        public void Setup()
        {
            Container = new UnityContainer()
                .AddExtension(new OptionsExtension())
                .AddExtension(new Diagnostic());
        }

        private class CountIncrement : IConfigureNamedOptions<FakeOptions>
        {
            private OptionsMonitorTest _test;

            public CountIncrement(OptionsMonitorTest test)
            {
                _test = test;
            }

            public void Configure(FakeOptions options) => Configure(Options.DefaultName, options);

            public void Configure(string name, FakeOptions options)
            {
                _test.SetupInvokeCount++;
                options.Message += _test.SetupInvokeCount;
            }
        }
    }

    #region Test Data

    public class FakeSource : IOptionsChangeTokenSource<FakeOptions>
    {
        public FakeSource(FakeChangeToken token)
        {
            Token = token;
        }

        public FakeChangeToken Token { get; set; }

        public string Name { get; set; }

        public IChangeToken GetChangeToken()
        {
            return Token;
        }

        public void Changed()
        {
            Token.HasChanged = true;
            Token.InvokeChangeCallback();
        }
    }

    public class FakeChangeToken : IChangeToken, IDisposable
    {
        public bool ActiveChangeCallbacks { get; set; }
        public bool HasChanged { get; set; }
        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            _callback = () => callback(state);
            return this;
        }

        public void InvokeChangeCallback()
        {
            _callback?.Invoke();
        }

        public void Dispose()
        {
            _callback = null;
        }

        private Action _callback;
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

    public class ChangeToken : IChangeToken
    {
        public List<(Action<object>, object)> Callbacks { get; } = new List<(Action<object>, object)>();

        public bool HasChanged => false;

        public bool ActiveChangeCallbacks => true;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            var item = (callback, state);
            Callbacks.Add(item);
            return new DisposableAction(() => Callbacks.Remove(item));
        }

        private class DisposableAction : IDisposable
        {
            private Action _action;

            public DisposableAction(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                var a = _action;
                if (a != null)
                {
                    _action = null;
                    a();
                }
            }
        }
    }

    public class ChangeTokenSource<T> : IOptionsChangeTokenSource<T>
    {
        private readonly IChangeToken _changeToken;
        public ChangeTokenSource(IChangeToken changeToken)
        {
            _changeToken = changeToken;
        }

        public string Name => null;

        public IChangeToken GetChangeToken() => _changeToken;
    }

    #endregion
}
