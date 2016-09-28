namespace Microsoft.ApplicationInsights.Extensibility.Implementation
{
    using System;
    using Microsoft.ApplicationInsights.DataContracts;
#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

	/// <summary>
	/// Tests corresponding to TelemetryClientExtension methods.
	/// </summary>
	[TestClass]
    public class AsyncLoaclBasedOperationHolderTests
    {
        /// <summary>
        /// Tests the scenario if OperationItem throws ArgumentNullException with null telemetry client.
        /// </summary>
        [TestMethod]
#if !WINDOWS_UWP
		[ExpectedException(typeof(ArgumentNullException))]
#endif
		public void CreatingOperationItemWithNullTelemetryClientThrowsArgumentNullException()
        {
#if !WINDOWS_UWP
            var operationItem = new AsyncLocalBasedOperationHolder<DependencyTelemetry>(null, new DependencyTelemetry());
#else
			Assert.ThrowsException<ArgumentNullException>(() =>
			{
				var operationItem = new AsyncLocalBasedOperationHolder<DependencyTelemetry>(null, new DependencyTelemetry());
			});
#endif
		}

        /// <summary>
        /// Tests the scenario if OperationItem throws ArgumentNullException with null telemetry.
        /// </summary>
        [TestMethod]
#if !WINDOWS_UWP
		[ExpectedException(typeof(ArgumentNullException))]
#endif
		public void CreatingOperationItemWithNullTelemetryThrowsArgumentNullException()
        {
#if !WINDOWS_UWP
            var operationItem = new AsyncLocalBasedOperationHolder<DependencyTelemetry>(new TelemetryClient(), null);
#else
			Assert.ThrowsException<ArgumentNullException>(() =>
			{
				var operationItem = new AsyncLocalBasedOperationHolder<DependencyTelemetry>(new TelemetryClient(), null);
			});
#endif
		}

        /// <summary>
        /// Tests the scenario if creating OperationItem does not throw exception if arguments are not null.
        /// </summary>
        [TestMethod]
        public void CreatingOperationItemDoesNotThrowOnPassingValidArguments()
        {
            var operationItem = new AsyncLocalBasedOperationHolder<DependencyTelemetry>(new TelemetryClient(), new DependencyTelemetry());
        }
    }
}
