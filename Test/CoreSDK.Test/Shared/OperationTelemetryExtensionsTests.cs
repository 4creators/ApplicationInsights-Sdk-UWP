﻿namespace Microsoft.ApplicationInsights
{
	using System;
	using System.Threading;
	using Microsoft.ApplicationInsights.DataContracts;
#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
	using Assert = Xunit.Assert;
	using System.Threading.Tasks;

	/// <summary>
	/// Tests corresponding to OperationExtension methods.
	/// </summary>
	[TestClass]
    public class OperationTelemetryExtensionsTests
    {
        /// <summary>
        /// Tests the scenario if StartOperation returns operation with telemetry item of same type.
        /// </summary>
        [TestMethod]
        public void OperationTelemetryStartInitializesTimeStampAndStartTimeToTelemetry()
        {
            var telemetry = new DependencyTelemetry();
            Assert.Equal(telemetry.StartTime, DateTimeOffset.MinValue);
            Assert.Equal(telemetry.Timestamp, DateTimeOffset.MinValue);
            telemetry.Start();
            Assert.Equal(telemetry.StartTime, telemetry.Timestamp);
        }

        /// <summary>
        /// Tests the scenario if Stop does not change start time and timestamp after start is called.
        /// </summary>
        [TestMethod]
        public void OperationTelemetryStopDoesNotAffectTimeStampAndStartTimeAfterStart()
        {
            var telemetry = new DependencyTelemetry();
            telemetry.Start();
            DateTimeOffset actualTime = telemetry.Timestamp;
            telemetry.Stop();
            Assert.Equal(actualTime, telemetry.Timestamp);
            Assert.Equal(actualTime, telemetry.StartTime);
        }

        /// <summary>
        /// Tests the scenario if Stop computes the duration of the telemetry.
        /// </summary>
        [TestMethod]
        public void OperationTelemetryStopComputesDurationAfterStart()
        {
            var telemetry = new DependencyTelemetry();
            telemetry.Start();
            //Thread.Sleep(2000);
			Task.Delay(2000).Wait();
            Assert.Equal(TimeSpan.Zero, telemetry.Duration);
            telemetry.Stop();
            Assert.True(telemetry.Duration.TotalMilliseconds > 0);
        }

        /// <summary>
        /// Tests the scenario if Stop computes assigns current time to start time and time stamp and assigns 0 to duration without start().
        /// </summary>
        [TestMethod]
        public void OperationTelemetryStopAssignsCurrentTimeAsStartTimeAndTimeStampWithoutStart()
        {
            var telemetry = new DependencyTelemetry();
            telemetry.Stop();
            Assert.NotEqual(DateTimeOffset.MinValue, telemetry.StartTime);
            Assert.Equal(telemetry.StartTime, telemetry.Timestamp);
            Assert.Equal(telemetry.Duration, TimeSpan.Zero);
        }
    }
}
