﻿namespace Microsoft.ApplicationInsights
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
	using Extensibility.Implementation;
    using TestFramework;

    [TestClass]
    public class TelemetryClientExtensionTests
    {
        private TelemetryClient telemetryClient;
        private List<ITelemetry> sendItems;

        [TestInitialize]
        public void TestInitialize()
        {
            var configuration = new TelemetryConfiguration();
            this.sendItems = new List<ITelemetry>();
            configuration.TelemetryChannel = new StubTelemetryChannel { OnSend = item => this.sendItems.Add(item) };
            configuration.InstrumentationKey = Guid.NewGuid().ToString();
            this.telemetryClient = new TelemetryClient(configuration);
            AsyncLocalHelpers.SaveOperationContext(null);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            AsyncLocalHelpers.SaveOperationContext(null);
        }

        [TestMethod]
        public void StartDependencyTrackingReturnsOperationWithSameTelemetryItem()
        {
            var operation = this.telemetryClient.StartOperation<DependencyTelemetry>(null);
            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.Telemetry);

            AsyncLocalHelpers.SaveOperationContext(null);

        }

        [TestMethod]
        public void StartDependencyTrackingReturnsOperationWithInitializedOperationName()
        {
            var operation = this.telemetryClient.StartOperation<DependencyTelemetry>("TestOperationName");
            Assert.AreEqual("TestOperationName", operation.Telemetry.Name);
        }

        [TestMethod]
        public void StartDependencyTrackingReturnsOperationWithInitializedOperationId()
        {
            var operation = this.telemetryClient.StartOperation<DependencyTelemetry>("TestOperationName");
            Assert.IsNotNull(operation.Telemetry.Id);
        }

        [TestMethod]
#if !WINDOWS_UWP
		[ExpectedException(typeof(ArgumentNullException))]
#endif
		public void StartDependencyTrackingThrowsExceptionWithNullTelemetryClient()
        {
            TelemetryClient tc = null;
#if !WINDOWS_UWP
            tc.StartOperation<DependencyTelemetry>(null);
#else
			Assert.ThrowsException<ArgumentNullException>(() => tc.StartOperation<DependencyTelemetry>(null));
#endif
		}

        [TestMethod]
        public void StartDependencyTrackingCreatesADependencyTelemetryItemWithTimeStamp()
        {
            var operation = this.telemetryClient.StartOperation<DependencyTelemetry>(null);
            Assert.AreEqual(operation.Telemetry.StartTime, operation.Telemetry.Timestamp);
            Assert.AreNotEqual(operation.Telemetry.StartTime, DateTimeOffset.MinValue);

            AsyncLocalHelpers.SaveOperationContext(null);
        }

        [TestMethod]
        public void StartDependencyTrackingAddsOperationContextStoreToCallContext()
        {
            Assert.IsNull(AsyncLocalHelpers.GetCurrentOperationContext());
            var operation = this.telemetryClient.StartOperation<DependencyTelemetry>(null);
            Assert.IsNotNull(AsyncLocalHelpers.GetCurrentOperationContext());

            AsyncLocalHelpers.SaveOperationContext(null);

        }

        [TestMethod]
        public void UsingSendsTelemetryAndDisposesOperationItem()
        {
            Assert.IsNull(AsyncLocalHelpers.GetCurrentOperationContext());
            using (var operation = this.telemetryClient.StartOperation<DependencyTelemetry>(null))
            {
            }

            Assert.IsNull(AsyncLocalHelpers.GetCurrentOperationContext());
            Assert.AreEqual(1, this.sendItems.Count);
            AsyncLocalHelpers.SaveOperationContext(null);
        }

        [TestMethod]
        public void UsingWithStopOperationSendsTelemetryAndDisposesOperationItemOnlyOnce()
        {
            Assert.IsNull(AsyncLocalHelpers.GetCurrentOperationContext());
            using (var operation = this.telemetryClient.StartOperation<DependencyTelemetry>(null))
            {
                this.telemetryClient.StopOperation(operation);
            }

            Assert.IsNull(AsyncLocalHelpers.GetCurrentOperationContext());
            Assert.AreEqual(1, this.sendItems.Count);
        }

        [TestMethod]
        public void StartDependencyTrackingHandlesMultipleContextStoresInCallContext()
        {
            var operation = this.telemetryClient.StartOperation<DependencyTelemetry>("OperationName") as AsyncLocalBasedOperationHolder<DependencyTelemetry>;
            var parentContextStore = AsyncLocalHelpers.GetCurrentOperationContext();
            Assert.AreEqual(operation.Telemetry.Context.Operation.Id, parentContextStore.ParentOperationId);
            Assert.AreEqual(operation.Telemetry.Context.Operation.Name, parentContextStore.RootOperationName);

            var childOperation = this.telemetryClient.StartOperation<DependencyTelemetry>("OperationName") as AsyncLocalBasedOperationHolder<DependencyTelemetry>;
            var childContextStore = AsyncLocalHelpers.GetCurrentOperationContext();
            Assert.AreEqual(childOperation.Telemetry.Context.Operation.Id, childContextStore.ParentOperationId);
            Assert.AreEqual(childOperation.Telemetry.Context.Operation.Name, childContextStore.RootOperationName);

            Assert.IsNull(operation.ParentContext);
            Assert.AreEqual(parentContextStore, childOperation.ParentContext);

            this.telemetryClient.StopOperation(childOperation);
            Assert.AreEqual(parentContextStore, AsyncLocalHelpers.GetCurrentOperationContext());
            this.telemetryClient.StopOperation(operation);
            Assert.IsNull(AsyncLocalHelpers.GetCurrentOperationContext());
        }

        [TestMethod]
        public void StopOperationDoesNotFailOnNullOperation()
        {
            TelemetryClient tc = new TelemetryClient();
            tc.StopOperation<DependencyTelemetry>(null);
        }

        [TestMethod]
#if !WINDOWS_UWP
		[ExpectedException(typeof(ArgumentNullException))]
#endif
		public void StopDependencyTrackingThrowsExceptionWithNullTelemetryClient()
        {
            var operationItem = new AsyncLocalBasedOperationHolder<DependencyTelemetry>(this.telemetryClient, new DependencyTelemetry());
            TelemetryClient tc = null;
#if !WINDOWS_UWP
            tc.StopOperation(operationItem);
#else
			Assert.ThrowsException<ArgumentNullException>(() => tc.StopOperation(operationItem));
#endif
		}

		[TestMethod]
        public void StopOperationDoesNotThrowExceptionIfParentOpertionIsStoppedBeforeChildOperation()
        {
            using (var parentOperation = this.telemetryClient.StartOperation<DependencyTelemetry>("operationName"))
            {
                using (var childOperation = this.telemetryClient.StartOperation<DependencyTelemetry>("operationName"))
                {
                    this.telemetryClient.StopOperation(parentOperation);
                }
            }
        }

        [TestMethod]
        public void StopOperationWorksFineWithNestedOperations()
        {
            using (var parentOperation = this.telemetryClient.StartOperation<DependencyTelemetry>("operationName"))
            {
                using (var childOperation = this.telemetryClient.StartOperation<DependencyTelemetry>("operationName"))
                {
                    this.telemetryClient.StopOperation(childOperation);
                }

                this.telemetryClient.StopOperation(parentOperation);
            }

            Assert.AreEqual(2, this.sendItems.Count);
        }


        [TestMethod]
        public void StartDependencyTrackingStoresTheArgumentOperationNameInContext()
        {
            var operation = this.telemetryClient.StartOperation<DependencyTelemetry>("TestOperationName");
            Assert.AreEqual("TestOperationName", AsyncLocalHelpers.GetCurrentOperationContext().RootOperationName);
        }
    }
}
