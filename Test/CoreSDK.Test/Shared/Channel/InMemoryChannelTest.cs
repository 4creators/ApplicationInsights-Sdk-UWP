﻿namespace Microsoft.ApplicationInsights.Channel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ApplicationInsights.TestFramework;

#if !NET40
    using System.Diagnostics.Tracing;
#endif

    using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;

#if NET40
    using Microsoft.Diagnostics.Tracing;
#endif

#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif

	using Assert = Xunit.Assert;
    
    [TestClass]
    public class InMemoryChannelTest
    {
        [TestMethod]
        public void WhenSendIsCalledTheEventIsBeingQueuedInTheBuffer()
        {
            var telemetryBuffer = new TelemetryBuffer();
            var channel = new InMemoryChannel(telemetryBuffer, new InMemoryTransmitter(telemetryBuffer));
            var sentTelemetry = new StubTelemetry();

            channel.Send(sentTelemetry);
            IEnumerable<ITelemetry> telemetries = telemetryBuffer.Dequeue();

            Assert.Equal(1, telemetries.Count());
            Assert.Same(sentTelemetry, telemetries.First());
        }

        [TestMethod]
        public void FlushCanBeAborted()
        {
            var telemetryBuffer = new TelemetryBuffer();
            var channel = new InMemoryChannel(telemetryBuffer, new InMemoryTransmitter(telemetryBuffer))
            {
                SendingInterval = TimeSpan.FromDays(1),
                EndpointAddress = "http://localhost/bad"
            };
            channel.Send(new StubTelemetry()); // Send telemetry so that it sets next send intreval and does not interfere with Flush
            channel.Flush();

            var transmission = new StubTelemetry();
            channel.Send(transmission);

            using (TestEventListener listener = new TestEventListener())
            {
                listener.EnableEvents(CoreEventSource.Log, EventLevel.Warning);
                channel.Flush(TimeSpan.FromTicks(1));

                var expectedMessage = listener.Messages.First();
                Assert.Equal(24, expectedMessage.EventId);
            }
        }
    }
}
