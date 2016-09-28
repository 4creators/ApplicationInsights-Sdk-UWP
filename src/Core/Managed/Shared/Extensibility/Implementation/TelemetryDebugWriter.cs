﻿// <copyright file="TelemetryDebugWriter.cs" company="Microsoft">
// Copyright © Microsoft. All Rights Reserved.
// </copyright>

#define DEBUG

namespace Microsoft.ApplicationInsights.Extensibility.Implementation
{
    using System;
    using System.Diagnostics;
    using Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Platform;

    /// <summary>
    /// Writes telemetry items to debug output.
    /// </summary>
    public class TelemetryDebugWriter : IDebugOutput
    {
        /// <summary>
        /// Gets or sets a value indicating whether writing telemetry items to debug output is enabled.
        /// </summary>
        public static bool IsTracingDisabled { get; set; }

        /// <summary>
        /// Write the specified <see cref="ITelemetry"/> item to debug output.
        /// </summary>
        /// <param name="telemetry">Item to write.</param>
        /// <param name="filteredBy">If specified, indicates the telemetry item was filtered out and not sent to the API.</param>
        public static void WriteTelemetry(ITelemetry telemetry, string filteredBy = null)
        {
            var output = PlatformSingleton.Current.GetDebugOutput();
            if (output.IsAttached() && output.IsLogging())
            {
                string prefix = "Application Insights Telemetry: ";
                if (string.IsNullOrEmpty(telemetry.Context.InstrumentationKey))
                {
                    prefix = "Application Insights Telemetry (unconfigured): ";
                }

                if (!string.IsNullOrEmpty(filteredBy))
                {
                    prefix = "Application Insights Telemetry (filtered by " + filteredBy + "): ";
                }

                string serializedTelemetry = JsonSerializer.SerializeAsString(telemetry);
                output.WriteLine(prefix + serializedTelemetry);
            }
        }

        void IDebugOutput.WriteLine(string message)
        {
#if CORE_PCL || NETFX_CORE
            Debug.WriteLine(message);
#else
            Debugger.Log(0, "category", message + Environment.NewLine);
#endif
        }

        bool IDebugOutput.IsLogging()
        {
            if (IsTracingDisabled)
            {
                return false;
            }
#if CORE_PCL || NETFX_CORE
            return true;
#else
            return Debugger.IsLogging();
#endif
        }

        bool IDebugOutput.IsAttached()
        {
            return Debugger.IsAttached;
        }
    }
}
