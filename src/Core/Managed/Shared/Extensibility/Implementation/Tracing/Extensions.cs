﻿namespace Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading;

    /// <summary>
    /// Provides a set of extension methods for tracing.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Extensions
    {
        /// <summary>
        /// Returns a culture-independent string representation of the given <paramref name="exception"/> object, 
        /// appropriate for diagnostics tracing.
        /// </summary>
        public static string ToInvariantString(this Exception exception)
        {
#if !CORE_PCL && !NETFX_CORE
            CultureInfo originalUICulture = Thread.CurrentThread.CurrentUICulture;
            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
#endif
			return exception.ToString();
#if !CORE_PCL && !NETFX_CORE
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
            }
#endif
		}
	}
}
