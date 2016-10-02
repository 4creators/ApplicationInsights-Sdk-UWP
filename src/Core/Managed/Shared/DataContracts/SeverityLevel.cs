namespace Microsoft.ApplicationInsights.DataContracts
{
    /// <summary>
    /// This enumeration is used by ExceptionTelemetry and TraceTelemetry to identify severity level.
    /// </summary>
    public enum SeverityLevel
    {
		/// <summary>
		/// Replacement value for null used in nullable type
		/// </summary>
		None,

		/// <summary>
		/// Verbose severity level.
		/// </summary>
		Verbose,

        /// <summary>
        /// Information severity level.
        /// </summary>
        Information,

        /// <summary>
        /// Warning severity level.
        /// </summary>
        Warning,

        /// <summary>
        /// Error severity level.
        /// </summary>
        Error,

        /// <summary>
        /// Critical severity level.
        /// </summary>
        Critical,

    }
}
