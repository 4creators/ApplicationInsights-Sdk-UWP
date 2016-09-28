﻿namespace Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing
{
#if CORE_PCL || NET45 || NET46 || NETFX_CORE
    using System.Diagnostics.Tracing;
#endif
#if NET40
    using Microsoft.Diagnostics.Tracing;
#endif
#if !WINDOWS_UWP
	using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
	using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
	using Mocks;

    [TestClass]
    public class F5DiagnosticsSenderTest
    {
        [TestMethod]
        public void TestLogMessage()
        {
            var senderMock = new F5DiagnosticsSenderMock();
            var evt = new TraceEvent
            {
                MetaData = new EventMetaData
                {
                    EventId = 10,
                    Keywords = 0x20,
                    Level = EventLevel.Warning,
                    MessageFormat = "Error occured at {0}, {1}"
                },
                Payload = new[] { "My function", "some failure" }
            };

            senderMock.Send(evt);
            Assert.AreEqual(1, senderMock.Messages.Count);
            Assert.AreEqual("Error occured at My function, some failure", senderMock.Messages[0]);
        }

        [TestMethod]
        public void TestLogMessageWithEmptyPayload()
        {
            var senderMock = new F5DiagnosticsSenderMock();
            var evt = new TraceEvent
            {
                MetaData = new EventMetaData
                {
                    EventId = 10,
                    Keywords = 0x20,
                    Level = EventLevel.Warning,
                    MessageFormat = "Error occured"
                },
                Payload = null
            };

            senderMock.Send(evt);
            Assert.AreEqual(1, senderMock.Messages.Count);
            Assert.AreEqual("Error occured", senderMock.Messages[0]);
        }
    }
}
