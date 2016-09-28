﻿namespace Microsoft.ApplicationInsights
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
    
    /// <summary>
    /// Send events, metrics and other telemetry to the Application Insights service.
    /// </summary>
    public sealed class TelemetryClient
    {
        private const string VersionPrefix = "dotnet:";

        private readonly TelemetryConfiguration configuration;
        private TelemetryContext context;
        private string sdkVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryClient" /> class. Send telemetry with the active configuration, usually loaded from ApplicationInsights.config.
        /// </summary>
        public TelemetryClient() : this(TelemetryConfiguration.Active)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryClient" /> class. Send telemetry with the specified <paramref name="configuration"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The <paramref name="configuration"/> is null.</exception>
        public TelemetryClient(TelemetryConfiguration configuration)
        {
            if (configuration == null)
            {
                CoreEventSource.Log.TelemetryClientConstructorWithNoTelemetryConfiguration();
                configuration = TelemetryConfiguration.Active;
            }

            this.configuration = configuration;
        }

        /// <summary>
        /// Gets the current context that will be used to augment telemetry you send.
        /// </summary>
        public TelemetryContext Context
        {
            get { return LazyInitializer.EnsureInitialized(ref this.context, () => new TelemetryContext()); }
            internal set { this.context = value; }
        }

        /// <summary>
        /// Gets or sets the default instrumentation key for all <see cref="ITelemetry"/> objects logged in this <see cref="TelemetryClient"/>.
        /// </summary>
        public string InstrumentationKey
        {
            get { return this.Context.InstrumentationKey; }
            set { this.Context.InstrumentationKey = value; }
        }

        /// <summary>
        /// Gets the <see cref="TelemetryConfiguration"/> object associated with this telemetry client instance.
        /// </summary>
        internal TelemetryConfiguration TelemetryConfiguration
        {
            get { return this.configuration; }            
        }

        /// <summary>
        /// Check to determine if the tracking is enabled.
        /// </summary>
        public bool IsEnabled()
        {
            return !this.configuration.DisableTelemetry;
        }        
                
        /// <summary>
        /// Send an <see cref="EventTelemetry"/> for display in Diagnostic Search and aggregation in Metrics Explorer.
        /// </summary>
        /// <param name="eventName">A name for the event.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            var telemetry = new EventTelemetry(eventName);

            if (properties != null && properties.Count > 0)
            {
                Utils.CopyDictionary(properties, telemetry.Context.Properties);
            }

            if (metrics != null && metrics.Count > 0)
            {
                Utils.CopyDictionary(metrics, telemetry.Metrics);
            }

            this.TrackEvent(telemetry);
        }

        /// <summary>
        /// Send an <see cref="EventTelemetry"/> for display in Diagnostic Search and aggregation in Metrics Explorer.
        /// Create a separate <see cref="EventTelemetry"/> instance for each call to <see cref="TrackEvent(EventTelemetry)"/>.
        /// </summary>
        /// <param name="telemetry">An event log item.</param>
        public void TrackEvent(EventTelemetry telemetry)
        {
            if (telemetry == null)
            {
                telemetry = new EventTelemetry();
            }

            this.Track(telemetry);
        }

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        public void TrackTrace(string message)
        {
            this.TrackTrace(new TraceTelemetry(message));
        }

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="severityLevel">Trace severity level.</param>
        public void TrackTrace(string message, SeverityLevel severityLevel)
        {
            this.TrackTrace(new TraceTelemetry(message, severityLevel));
        }

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        public void TrackTrace(string message, IDictionary<string, string> properties)
        {
            TraceTelemetry telemetry = new TraceTelemetry(message);

            if (properties != null && properties.Count > 0)
            {
                Utils.CopyDictionary(properties, telemetry.Context.Properties);
            }

            this.TrackTrace(telemetry);
        }

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="severityLevel">Trace severity level.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        public void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string> properties)
        {
            TraceTelemetry telemetry = new TraceTelemetry(message, severityLevel);

            if (properties != null && properties.Count > 0)
            {
                Utils.CopyDictionary(properties, telemetry.Context.Properties);
            }

            this.TrackTrace(telemetry);
        }

        /// <summary>
        /// Send a trace message for display in Diagnostic Search.
        /// Create a separate <see cref="TraceTelemetry"/> instance for each call to <see cref="TrackTrace(TraceTelemetry)"/>.
        /// </summary>
        /// <param name="telemetry">Message with optional properties.</param>
        public void TrackTrace(TraceTelemetry telemetry)
        {
            telemetry = telemetry ?? new TraceTelemetry();
            this.Track(telemetry);
        }

        /// <summary>
        /// Send a <see cref="MetricTelemetry"/> for aggregation in Metric Explorer.
        /// </summary>
        /// <param name="name">Metric name.</param>
        /// <param name="value">Metric value.</param>
        /// <param name="properties">Named string values you can use to classify and filter metrics.</param>
        public void TrackMetric(string name, double value, IDictionary<string, string> properties = null)
        {
            var telemetry = new MetricTelemetry(name, value);
            if (properties != null && properties.Count > 0)
            {
                Utils.CopyDictionary(properties, telemetry.Properties);
            }

            this.TrackMetric(telemetry);
        }

        /// <summary>
        /// Send a <see cref="MetricTelemetry"/> for aggregation in Metric Explorer.
        /// Create a separate <see cref="MetricTelemetry"/> instance for each call to <see cref="TrackMetric(MetricTelemetry)"/>.
        /// </summary>
        public void TrackMetric(MetricTelemetry telemetry)
        {
            if (telemetry == null)
            {
                telemetry = new MetricTelemetry();
            }

            this.Track(telemetry);
        }

        /// <summary>
        /// Send an <see cref="ExceptionTelemetry"/> for display in Diagnostic Search.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="properties">Named string values you can use to classify and search for this exception.</param>
        /// <param name="metrics">Additional values associated with this exception.</param>
        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            if (exception == null)
            {
                exception = new Exception(Utils.PopulateRequiredStringValue(null, "message", typeof(ExceptionTelemetry).FullName));
            }

            var telemetry = new ExceptionTelemetry(exception) { HandledAt = ExceptionHandledAt.UserCode };

            if (properties != null && properties.Count > 0)
            {
                Utils.CopyDictionary(properties, telemetry.Context.Properties);
            }

            if (metrics != null && metrics.Count > 0)
            {
                Utils.CopyDictionary(metrics, telemetry.Metrics);
            }

            this.TrackException(telemetry);
        }

        /// <summary>
        /// Send an <see cref="ExceptionTelemetry"/> for display in Diagnostic Search.
        /// Create a separate <see cref="ExceptionTelemetry"/> instance for each call to <see cref="TrackException(ExceptionTelemetry)"/>
        /// </summary>
        public void TrackException(ExceptionTelemetry telemetry)
        {
            if (telemetry == null)
            {
                var exception = new Exception(Utils.PopulateRequiredStringValue(null, "message", typeof(ExceptionTelemetry).FullName));
                telemetry = new ExceptionTelemetry(exception)
                {
                    HandledAt = ExceptionHandledAt.UserCode,
                };
            }

            this.Track(telemetry);
        }

        /// <summary>
        /// Send information about external dependency call in the application.
        /// </summary>
        /// <param name="dependencyName">External dependency name.</param>
        /// <param name="commandName">Dependency call command name.</param>
        /// <param name="startTime">The time when the dependency was called.</param>
        /// <param name="duration">The time taken by the external dependency to handle the call.</param>
        /// <param name="success">True if the dependency call was handled successfully.</param>
        public void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
            this.TrackDependency(new DependencyTelemetry(dependencyName, commandName, startTime, duration, success));
        }

        /// <summary>
        /// Send information about external dependency call in the application.
        /// Create a separate <see cref="DependencyTelemetry"/> instance for each call to <see cref="TrackDependency(DependencyTelemetry)"/>
        /// </summary>
        public void TrackDependency(DependencyTelemetry telemetry)
        {
            if (telemetry == null)
            {
                telemetry = new DependencyTelemetry();
            }

            this.Track(telemetry);
        }

        /// <summary>
        /// Send information about availability of an application.
        /// </summary>
        /// <param name="name">Availability test name.</param>
        /// <param name="timeStamp">The time when the availability was captured.</param>
        /// <param name="duration">The time taken for the availability test to run.</param>
        /// <param name="runLocation">Name of the location the availability test was run from.</param>        
        /// <param name="success">True if the availability test ran successfully.</param>
        /// <param name="message">Error message on availability test run failure.</param>
        public void TrackAvailability(string name, DateTimeOffset timeStamp, TimeSpan duration, string runLocation, bool success, string message = null)
        {
            this.TrackAvailability(new AvailabilityTelemetry(name, timeStamp, duration, runLocation, success, message));
        }

        /// <summary>
        /// Send information about availability of an application.
        /// Create a separate <see cref="AvailabilityTelemetry"/> instance for each call to <see cref="TrackAvailability(AvailabilityTelemetry)"/>
        /// </summary>
        public void TrackAvailability(AvailabilityTelemetry telemetry)
        {
            if (telemetry == null)
            {
                telemetry = new AvailabilityTelemetry();
            }

            this.Track(telemetry);
        }

        /// <summary>
        /// This method is an internal part of Application Insights infrastructure. Do not call.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Track(ITelemetry telemetry)
        {
            // TALK TO YOUR TEAM MATES BEFORE CHANGING THIS.
            // This method needs to be public so that we can build and ship new telemetry types without having to ship core.
            // It is hidden from intellisense to prevent customer confusion.
            if (this.IsEnabled())
            {
                this.Initialize(telemetry);
                                
                if (string.IsNullOrEmpty(telemetry.Context.InstrumentationKey))
                {
                    TelemetryDebugWriter.WriteTelemetry(telemetry);
                    return;
                }

                // invokes the Process in the first processor in the chain
                this.configuration.TelemetryProcessorChain.Process(telemetry);

#if !CORE_PCL && !NETFX_CORE
                // logs rich payload ETW event for any partners to process it
                RichPayloadEventSource.Log.Process(telemetry);
#endif
			}
		}

        /// <summary>
        /// This method is an internal part of Application Insights infrastructure. Do not call.
        /// </summary>
        /// <param name="telemetry">Telemetry item to initialize.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Initialize(ITelemetry telemetry)
        {
            string instrumentationKey = this.Context.InstrumentationKey;

            if (string.IsNullOrEmpty(instrumentationKey))
            {
                instrumentationKey = this.configuration.InstrumentationKey;
            }

            var telemetryWithProperties = telemetry as ISupportProperties;
            if (telemetryWithProperties != null)
            {
                if ((this.configuration.TelemetryChannel != null) && (this.configuration.TelemetryChannel.DeveloperMode.HasValue && this.configuration.TelemetryChannel.DeveloperMode.Value))
                {
                    if (!telemetryWithProperties.Properties.ContainsKey("DeveloperMode"))
                    {
                        telemetryWithProperties.Properties.Add("DeveloperMode", "true");
                    }
                }

                Utils.CopyDictionary(this.Context.Properties, telemetryWithProperties.Properties);
            }

            telemetry.Context.Initialize(this.Context, instrumentationKey);
            foreach (ITelemetryInitializer initializer in this.configuration.TelemetryInitializers)
            {
                try
                {
                    initializer.Initialize(telemetry);
                }
                catch (Exception exception)
                {
                    CoreEventSource.Log.LogError(string.Format(
                                                    CultureInfo.InvariantCulture,
                                                    "Exception while initializing {0}, exception message - {1}",
                                                    initializer.GetType().FullName,
                                                    exception));
                }
            }

            if (telemetry.Timestamp == default(DateTimeOffset))
            {
                telemetry.Timestamp = Clock.Instance.Time;
            }

            // Currenly backend requires SDK version to comply "name: version"
            if (string.IsNullOrEmpty(telemetry.Context.Internal.SdkVersion))
            {
                var version = LazyInitializer.EnsureInitialized(ref this.sdkVersion, this.GetSdkVersion);
                telemetry.Context.Internal.SdkVersion = version;
            }
        }

        /// <summary>
        /// Send information about the page viewed in the application.
        /// </summary>
        /// <param name="name">Name of the page.</param>
        public void TrackPageView(string name)
        {
            this.Track(new PageViewTelemetry(name));
        }

        /// <summary>
        /// Send information about the page viewed in the application.
        /// Create a separate <see cref="PageViewTelemetry"/> instance for each call to <see cref="TrackPageView(PageViewTelemetry)"/>.
        /// </summary>
        public void TrackPageView(PageViewTelemetry telemetry)
        {
            if (telemetry == null)
            {
                telemetry = new PageViewTelemetry();
            }

            this.Track(telemetry);
        }

        /// <summary>
        /// Send information about a request handled by the application.
        /// </summary>
        /// <param name="name">The request name.</param>
        /// <param name="startTime">The time when the page was requested.</param>
        /// <param name="duration">The time taken by the application to handle the request.</param>
        /// <param name="responseCode">The response status code.</param>
        /// <param name="success">True if the request was handled successfully by the application.</param>
        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            this.Track(new RequestTelemetry(name, startTime, duration, responseCode, success));
        }

        /// <summary>
        /// Send information about a request handled by the application.
        /// Create a separate <see cref="RequestTelemetry"/> instance for each call to <see cref="TrackRequest(RequestTelemetry)"/>.
        /// </summary>
        public void TrackRequest(RequestTelemetry request)
        {
            if (request == null)
            {
                request = new RequestTelemetry();
            }

            this.Track(request);
        }
        
        /// <summary>
        /// Flushes the in-memory buffer. 
        /// </summary>
        public void Flush()
        {
            this.configuration.TelemetryChannel.Flush();
        }

        private string GetSdkVersion()
        {
#if !CORE_PCL && !NETFX_CORE
            string versionStr = typeof(TelemetryClient).Assembly.GetCustomAttributes(false)
                    .OfType<AssemblyFileVersionAttribute>()
                    .First()
                    .Version;
            
#else
            string versionStr = typeof(TelemetryClient).GetTypeInfo().Assembly.GetCustomAttributes<AssemblyFileVersionAttribute>()
                    .First()
                    .Version;
#endif

            Version version = new Version(versionStr);
            return VersionPrefix + version.ToString(3) + "-" + version.Revision;
        }
    }
}
