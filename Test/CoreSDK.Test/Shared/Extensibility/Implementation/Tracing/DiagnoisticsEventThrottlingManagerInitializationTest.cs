﻿namespace Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing
{
    using System.Collections.Generic;
    using System.Linq;
#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
	using Mocks;

    [TestClass]
    public class DiagnoisticsEventThrottlingManagerInitializationTest
    {
        private const uint SampleIntervalInMinutes = 10;
        private const uint SampleIntervalInMiliseconds = SampleIntervalInMinutes * 1000 * 60;

        private readonly DiagnoisticsEventThrottlingMock container = new DiagnoisticsEventThrottlingMock(
            throttleAll: true,
            signalJustExceeded: true,
            sampleCounters: new Dictionary<int, DiagnoisticsEventCounters>());

        private readonly DiagnoisticsEventThrottlingSchedulerMock scheduler = new DiagnoisticsEventThrottlingSchedulerMock();

        [TestMethod]
        public void TestActionRegisteredAfterInitialization()
        {
            var manager = new DiagnoisticsEventThrottlingManager<DiagnoisticsEventThrottlingMock>(
                this.container,
                this.scheduler,
                SampleIntervalInMinutes);

            Assert.AreEqual(1, this.scheduler.Items.Count, "Unexpected count of registered actions");

            var item = this.scheduler.Items.First();

            Assert.IsNotNull(item.Action, "Action is not set");

#if NET40
            Assert.AreEqual(
                manager.GetType(),
                item.Action.Method.DeclaringType,
                "Unexpected type of method binded with action");
#endif

            Assert.AreEqual(SampleIntervalInMiliseconds, (uint)item.Interval, "Unexpected interval value");
        }
    }
}