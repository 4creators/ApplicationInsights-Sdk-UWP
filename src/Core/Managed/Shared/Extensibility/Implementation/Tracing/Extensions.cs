namespace Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing
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
#if CORE_PCL
			return exception.ToString();
#else
			CultureInfo originalUICulture = null;
#if !NETFX_CORE
            originalUICulture = Thread.CurrentThread.CurrentUICulture;
#elif NETFX_CORE
			originalUICulture = CultureInfo.CurrentUICulture;
#endif
			try
			{
#if !NETFX_CORE
				Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
#else
				CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
#endif
				return exception.ToString();
            }
            finally
            {
#if !NETFX_CORE
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
#else
				CultureInfo.CurrentUICulture = originalUICulture;
#endif
			}
#endif
		}
	}
}
