﻿namespace Microsoft.ApplicationInsights.TestFramework
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
#if CORE_PCL || NET45 || NET46 || NETFX_CORE 
    using System.Diagnostics.Tracing;
#endif
	using System.Threading;
#if NET40
    using Microsoft.Diagnostics.Tracing;
#endif

    internal class TestEventListener : EventListener
    {
        private readonly ConcurrentQueue<EventWrittenEventArgs> events;
        private readonly AutoResetEvent eventWritten;

        public TestEventListener()
        {
            this.events = new ConcurrentQueue<EventWrittenEventArgs>();
            this.eventWritten = new AutoResetEvent(false);
            this.OnOnEventWritten = e =>
            {
                this.events.Enqueue(e);
                this.eventWritten.Set();
            };
        }

        public Action<EventSource> OnOnEventSourceCreated { get; set; }

        public Action<EventWrittenEventArgs> OnOnEventWritten { get; set; }

        public IEnumerable<EventWrittenEventArgs> Messages
        {
            get 
            {
                if (this.events.Count == 0)
                {
                    this.eventWritten.WaitOne(TimeSpan.FromSeconds(5));
                }

                while (this.events.Count != 0)
                {
                    EventWrittenEventArgs nextEvent;
                    if (this.events.TryDequeue(out nextEvent))
                    {
                        yield return nextEvent;
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
        }
        
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            this.OnOnEventWritten(eventData);
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            // Check for null because this method is called by the base class constror before we can initialize it
            Action<EventSource> callback = this.OnOnEventSourceCreated;
            if (callback != null)
            {
                callback(eventSource);
            }
        }
    }
}
