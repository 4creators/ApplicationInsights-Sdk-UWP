namespace Microsoft.ApplicationInsights.Extensibility.Implementation.External
{
	using System;
	using System.Collections.Generic;
	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
#if NET40
    [Microsoft.Diagnostics.Tracing.EventData(Name = "PartB_MetricData")]
#else
	[System.Diagnostics.Tracing.EventData(Name = "PartB_MetricData")]
#endif
    internal partial class MetricData
    {

    }

#if NETFX_CORE

	[System.Diagnostics.Tracing.EventData(Name = "PartB_MetricData")]
	internal class EtwMetricData
	{
		public int ver { get; set; }

		public IList<MetricDataPointAggregate> metrics { get; set; }

		public IDictionary<string, string> properties { get; set; }

		public EtwMetricData(MetricData data)
		{
			ver = data.ver;
			properties = data.properties;
			metrics = new List<MetricDataPointAggregate>();
			foreach (DataPoint p in data.metrics)
				metrics.Add(new MetricDataPointAggregate(p));
		}
	}

#endif
}