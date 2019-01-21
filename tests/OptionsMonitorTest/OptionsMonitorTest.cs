using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Unity;

namespace Tests.OptionsMonitorTest
{
    public partial class OptionsMonitorTest
    {
        [TestMethod]
        public void MonitorUsesFactory()
        {
            // Setup
            Container.RegisterType<IOptionsFactory<FakeOptions>, FakeOptionsFactory>();
            Container.Configure<FakeOptions>(o => o.Message = "Ignored");

            var monitor = Container.Resolve<IOptionsMonitor<FakeOptions>>();
            Assert.AreEqual(FakeOptionsFactory.Options, monitor.CurrentValue);
            Assert.AreEqual(FakeOptionsFactory.Options, monitor.Get("1"));
            Assert.AreEqual(FakeOptionsFactory.Options, monitor.Get("bsdfsdf"));
        }

        [TestMethod]
        public void CanClearNamedOptions()
        {
            // Setup
            Container.RegisterInstance<IConfigureOptions<FakeOptions>>(new CountIncrement(this));

            // Act

            var monitor = Container.Resolve<IOptionsMonitor<FakeOptions>>();
            var cache = Container.Resolve<IOptionsMonitorCache<FakeOptions>>();
            Assert.AreEqual("1", monitor.Get("#1").Message);
            Assert.AreEqual("2", monitor.Get("#2").Message);
            Assert.AreEqual("1", monitor.Get("#1").Message);
            Assert.AreEqual("2", monitor.Get("#2").Message);
            cache.Clear();
            Assert.AreEqual("3", monitor.Get("#1").Message);
            Assert.AreEqual("4", monitor.Get("#2").Message);
            Assert.AreEqual("3", monitor.Get("#1").Message);
            Assert.AreEqual("4", monitor.Get("#2").Message);

            cache.Clear();
            Assert.AreEqual("5", monitor.Get("#1").Message);
            Assert.AreEqual("6", monitor.Get("#2").Message);
            Assert.AreEqual("5", monitor.Get("#1").Message);
            Assert.AreEqual("6", monitor.Get("#2").Message);
        }

        [TestMethod]
        public void CanWatchNamedOptions()
        {
            // Setup
            var changeToken = new FakeChangeToken();
            var changeToken2 = new FakeChangeToken();
            Container.RegisterInstance<IConfigureOptions<FakeOptions>>(new CountIncrement(this));
            Container.RegisterInstance<IOptionsChangeTokenSource<FakeOptions>>("#1", new FakeSource(changeToken)  { Name = "#1" });
            Container.RegisterInstance<IOptionsChangeTokenSource<FakeOptions>>("#2", new FakeSource(changeToken2) { Name = "#2" });

            // Act
            var monitor = Container.Resolve<IOptionsMonitor<FakeOptions>>();

            Assert.IsNotNull(monitor);
            Assert.AreEqual("1", monitor.Get("#1").Message);

            string updatedMessage = null;
            monitor.OnChange((o, n) => updatedMessage = o.Message + n);

            changeToken.InvokeChangeCallback();
            Assert.AreEqual("2#1", updatedMessage);
            Assert.AreEqual("2", monitor.Get("#1").Message);

            changeToken2.InvokeChangeCallback();
            Assert.AreEqual("3#2", updatedMessage);
            Assert.AreEqual("3", monitor.Get("#2").Message);
        }

        [TestMethod]
        public void CanWatchOptions()
        {
            // Setup
            var changeToken = new FakeChangeToken();
            Container.RegisterInstance<IConfigureOptions<FakeOptions>>(new CountIncrement(this));
            Container.RegisterInstance<IOptionsChangeTokenSource<FakeOptions>>(new FakeSource(changeToken));

            // Act
            var monitor = Container.Resolve<IOptionsMonitor<FakeOptions>>();
            Assert.IsNotNull(monitor);
            Assert.AreEqual("1", monitor.CurrentValue.Message);

            string updatedMessage = null;
            monitor.OnChange(o => updatedMessage = o.Message);
            changeToken.InvokeChangeCallback();
            Assert.AreEqual("2", updatedMessage);

            // Verify old watch is changed too
            Assert.AreEqual("2", monitor.CurrentValue.Message);
        }

        [TestMethod]
        public void CanWatchOptionsWithMultipleSourcesAndCallbacks()
        {
            // Setup
            var changeToken = new FakeChangeToken();
            var tracker = new FakeSource(changeToken);
            var changeToken2 = new FakeChangeToken();
            var tracker2 = new FakeSource(changeToken2);
            Container.RegisterInstance<IConfigureOptions<FakeOptions>>(new CountIncrement(this));
            Container.RegisterInstance<IOptionsChangeTokenSource<FakeOptions>>("1", tracker);
            Container.RegisterInstance<IOptionsChangeTokenSource<FakeOptions>>("2", tracker2);

            // Act

            var monitor = Container.Resolve<IOptionsMonitor<FakeOptions>>();
            Assert.IsNotNull(monitor);
            Assert.AreEqual("1", monitor.CurrentValue.Message);

            string updatedMessage = null;
            string updatedMessage2 = null;
            var cleanup = monitor.OnChange(o => updatedMessage = o.Message);
            var cleanup2 = monitor.OnChange(o => updatedMessage2 = o.Message);
            changeToken.InvokeChangeCallback();
            Assert.AreEqual("2", updatedMessage);
            Assert.AreEqual("2", updatedMessage2);

            // Verify old watch is changed too
            Assert.AreEqual("2", monitor.CurrentValue.Message);

            changeToken2.InvokeChangeCallback();
            Assert.AreEqual("3", updatedMessage);
            Assert.AreEqual("3", updatedMessage2);

            // Verify old watch is changed too
            Assert.AreEqual("3", monitor.CurrentValue.Message);

            cleanup.Dispose();
            changeToken.InvokeChangeCallback();
            changeToken2.InvokeChangeCallback();

            // Verify only the second message changed
            Assert.AreEqual("3", updatedMessage);
            Assert.AreEqual("5", updatedMessage2);

            cleanup2.Dispose();
            changeToken.InvokeChangeCallback();
            changeToken2.InvokeChangeCallback();

            // Verify no message changed
            Assert.AreEqual("3", updatedMessage);
            Assert.AreEqual("5", updatedMessage2);
        }

        [TestMethod]
        public void CanWatchOptionsWithMultipleSources()
        {
            // Setup
            var changeToken = new FakeChangeToken();
            var tracker = new FakeSource(changeToken);
            var changeToken2 = new FakeChangeToken();
            var tracker2 = new FakeSource(changeToken2);
            Container.RegisterInstance<IConfigureOptions<FakeOptions>>(new CountIncrement(this));
            Container.RegisterInstance<IOptionsChangeTokenSource<FakeOptions>>("1", tracker);
            Container.RegisterInstance<IOptionsChangeTokenSource<FakeOptions>>("2", tracker2);

            // Act

            var monitor = Container.Resolve<IOptionsMonitor<FakeOptions>>();
            Assert.IsNotNull(monitor);
            Assert.AreEqual("1", monitor.CurrentValue.Message);

            string updatedMessage = null;
            var cleanup = monitor.OnChange(o => updatedMessage = o.Message);
            changeToken.InvokeChangeCallback();
            Assert.AreEqual("2", updatedMessage);

            // Verify old watch is changed too
            Assert.AreEqual("2", monitor.CurrentValue.Message);

            changeToken2.InvokeChangeCallback();
            Assert.AreEqual("3", updatedMessage);

            // Verify old watch is changed too
            Assert.AreEqual("3", monitor.CurrentValue.Message);

            cleanup.Dispose();
            changeToken.InvokeChangeCallback();
            changeToken2.InvokeChangeCallback();

            // Verify messages aren't changed
            Assert.AreEqual("3", updatedMessage);
        }

        [TestMethod]
        public void CanMonitorConfigBoundOptions()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection().Build();

            // Setup
            Container.RegisterInstance<IConfigureOptions<FakeOptions>>(new CountIncrement(this));
            Container.Configure<FakeOptions>(config);

            // Act

            var monitor = Container.Resolve<IOptionsMonitor<FakeOptions>>();
            Assert.IsNotNull(monitor);
            Assert.AreEqual("1", monitor.CurrentValue.Message);

            string updatedMessage = null;

            var cleanup = monitor.OnChange(o => updatedMessage = o.Message);

            config.Reload();
            Assert.AreEqual("2", updatedMessage);

            // Verify old watch is changed too
            Assert.AreEqual("2", monitor.CurrentValue.Message);

            cleanup.Dispose();
            config.Reload();

            // Verify our message don't change after the subscription is disposed
            Assert.AreEqual("2", updatedMessage);

            // But the monitor still gets updated with the latest current value
            Assert.AreEqual("3", monitor.CurrentValue.Message);
        }

        [TestMethod]
        public void CanMonitorConfigBoundNamedOptions()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection().Build();

            // Setup
            Container.RegisterInstance<IConfigureOptions<FakeOptions>>(new CountIncrement(this));
            Container.Configure<FakeOptions>("config", config);

            // Act

            var monitor = Container.Resolve<IOptionsMonitor<FakeOptions>>();
            Assert.IsNotNull(monitor);
            Assert.AreEqual("1", monitor.CurrentValue.Message);

            string updatedMessage = null;

            var cleanup = monitor.OnChange((o, n) => updatedMessage = o.Message + "#" + n);

            config.Reload();
            Assert.AreEqual("2#config", updatedMessage);

            // Verify non-named option is unchanged
            Assert.AreEqual("1", monitor.CurrentValue.Message);

            cleanup.Dispose();
            config.Reload();

            // Verify our message don't change after the subscription is disposed
            Assert.AreEqual("2#config", updatedMessage);

            // But the monitor still gets updated with the latest current value
            Assert.AreEqual("3", monitor.Get("config").Message);
            Assert.AreEqual("1", monitor.CurrentValue.Message);
        }

        public class ControllerWithMonitor : IDisposable
        {
            IDisposable _watcher;
            FakeOptions _options;

            public ControllerWithMonitor(IOptionsMonitor<FakeOptions> watcher)
            {
                _watcher = watcher.OnChange(o => _options = o);
            }

            public void Dispose() => _watcher?.Dispose();

            public string Message => _options?.Message;
        }

        [TestMethod]
        public void ControllerCanWatchOptionsThatTrackConfigChanges()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection().Build();

            // Setup
            Container.RegisterInstance<IConfigureOptions<FakeOptions>>(new CountIncrement(this));
            Container.RegisterType<ControllerWithMonitor, ControllerWithMonitor>();
            Container.Configure<FakeOptions>(config);

            // Act

            var controller = Container.Resolve<ControllerWithMonitor>();
            Assert.IsNull(controller.Message);

            config.Reload();
            Assert.AreEqual("1", controller.Message);

            config.Reload();
            Assert.AreEqual("2", controller.Message);
        }
    }
}
