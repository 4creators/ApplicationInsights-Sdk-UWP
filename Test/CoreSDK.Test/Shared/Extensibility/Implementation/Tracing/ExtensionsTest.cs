namespace Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing
{
    using System;
    using System.Globalization;
    using System.Threading;
    using Microsoft.ApplicationInsights.TestFramework;
#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
	using Assert = Xunit.Assert;

    public class ExtensionsTest
    {
        [TestClass]
        public class ToInvariantString
        {
#if NETFX_CORE
			private CultureInfo originalUICulture = CultureInfo.CurrentUICulture;
#else
			private CultureInfo originalUICulture = Thread.CurrentThread.CurrentUICulture;
#endif

			[TestCleanup]
            public void Cleanup()
            {
#if NETFX_CORE
				CultureInfo.CurrentUICulture = this.originalUICulture;
#else
				Thread.CurrentThread.CurrentUICulture = this.originalUICulture;
#endif
			}

            [TestMethod]
            public void ExtractsStackTraceWithInvariantCultureToHelpOurTelemetryToolsMatchSimilarErrorsReportedByOSsWithDifferentLanguages()
            {
                CultureInfo stackTraceCulture = null;
                var exception = new StubException();
                exception.OnToString = () =>
                {
#if NETFX_CORE
					stackTraceCulture = CultureInfo.CurrentUICulture;
#else
					stackTraceCulture = Thread.CurrentThread.CurrentUICulture;
#endif
					return string.Empty;
                };

                Extensions.ToInvariantString(exception);

                Assert.Same(CultureInfo.InvariantCulture, stackTraceCulture);
            }

            [TestMethod]
            public void RestoresOriginalUICultureToPreserveGlobalStateOfApplication()
            {
                Extensions.ToInvariantString(new Exception());
#if NETFX_CORE
				Assert.Same(this.originalUICulture, CultureInfo.CurrentUICulture);
#else
				Assert.Same(this.originalUICulture, Thread.CurrentThread.CurrentUICulture);
#endif
			}
        }
    }
}
