namespace Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing
{
    using System;
#if CORE_PCL || NET45 || NET46 || NETFX_CORE
    using System.Diagnostics.Tracing;
#endif
    using System.Linq;
#if NET40
    using Microsoft.Diagnostics.Tracing;
#endif
    using System.Threading;
    using System.Threading.Tasks;
#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
	using Assert = Xunit.Assert;

    [TestClass]
    public class DiagnosticsTelemetryModuleTest
    {
        [TestMethod]
        public void TestModuleDefaultInitialization()
        {
            using (var initializedModule = new DiagnosticsTelemetryModule())
            {
                initializedModule.Initialize(new TelemetryConfiguration());
                
                Assert.True(string.IsNullOrEmpty(initializedModule.DiagnosticsInstrumentationKey));
                Assert.Equal("Error", initializedModule.Severity);

                Assert.Equal(2, initializedModule.Senders.Count);
                Assert.Equal(1, initializedModule.Senders.OfType<PortalDiagnosticsSender>().Count());
                Assert.Equal(1, initializedModule.Senders.OfType<F5DiagnosticsSender>().Count());
            }
        }

        [TestMethod]
        public void TestDiagnosticsModuleSetInstrumentationKey()
        {
            var diagnosticsInstrumentationKey = Guid.NewGuid().ToString();
            using (var initializedModule = new DiagnosticsTelemetryModule())
            {
                initializedModule.Initialize(new TelemetryConfiguration());
                initializedModule.DiagnosticsInstrumentationKey = diagnosticsInstrumentationKey;

                Assert.Equal(diagnosticsInstrumentationKey, initializedModule.DiagnosticsInstrumentationKey);

                Assert.Equal(
                    diagnosticsInstrumentationKey,
                    initializedModule.Senders.OfType<PortalDiagnosticsSender>().First().DiagnosticsInstrumentationKey);
            }
        }

        [TestMethod]
        public void TestDiagnosticsModuleSetSeverity()
        {
            using (var initializedModule = new DiagnosticsTelemetryModule())
            {
                initializedModule.Initialize(new TelemetryConfiguration());
                
                Assert.Equal(EventLevel.Error.ToString(), initializedModule.Severity);

                initializedModule.Severity = "Informational";

                Assert.Equal(EventLevel.Informational, initializedModule.EventListener.LogLevel);
            }
        }

        [TestMethod]
        public void TestDiagnosticModuleDoesNotThrowIfInitailizedTwice()
        {
            using (DiagnosticsTelemetryModule module = new DiagnosticsTelemetryModule())
            {
                module.Initialize(new TelemetryConfiguration());
                module.Initialize(new TelemetryConfiguration());
            }
        }

        [TestMethod]
        public void DiagnosticModuleDoesNotThrowIfQueueSenderContinuesRecieveEvents()
        {
            using (DiagnosticsTelemetryModule module = new DiagnosticsTelemetryModule())
            {
                var queueSender = module.Senders.OfType<PortalDiagnosticsQueueSender>().First();

                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    var taskStarted = new AutoResetEvent(false);
#if !NETFX_CORE
					TaskEx.Run(() =>
#else
					Task.Run(() =>
#endif
					{
                        taskStarted.Set();
                        while (!cancellationTokenSource.IsCancellationRequested)
                        {
                            queueSender.Send(new TraceEvent());
#if !NETFX_CORE

							Thread.Sleep(1);
#else
							Task.Delay(1).Wait();
#endif
                        }
                    }, cancellationTokenSource.Token);

                    taskStarted.WaitOne(TimeSpan.FromSeconds(5));

					module.Initialize(new TelemetryConfiguration());

					cancellationTokenSource.Cancel();
                }
            }
        }
    }
}
