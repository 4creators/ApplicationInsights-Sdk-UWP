namespace Microsoft.ApplicationInsights.Extensibility.Implementation.External
{
	using System;
	using System.Globalization;

	/// <summary>
	/// Partial class to add the EventData attribute and any additional customizations to the generated type.
	/// </summary>
#if NET40
    [Microsoft.Diagnostics.Tracing.EventData]
#else
	[System.Diagnostics.Tracing.EventData]
#endif
    internal partial class DataPoint
    {
		public bool IsAggregate()
		{
			return count != null || min != null || max != null || stdDev != null;
		}
	}

#if NETFX_CORE
	///// <summary>
	///// Proxy for non aggregate DataPoint for serialization of EventSource in .NET Core runtime
	///// </summary>
	//[System.Diagnostics.Tracing.EventData]
	//internal class MetricDataPoint
	//{
	//	public string name { get; set; }

	//	public DataPointType kind { get; set; }

	//	public double value { get; set; }

	//	public MetricDataPoint(DataPoint data, bool throwIfAggregate = true)
	//	{
	//		if (data.IsAggregate())
	//			throw new ArgumentException(
	//				String.Format(CultureInfo.CurrentCulture, 
	//				"Passed DataPoint is aggregate: {0}, Use {1} class as a proxy.", 
	//				nameof(data), nameof(MetricDataPointAggregate)));
	//		name = data.name;
	//		kind = data.kind;
	//		value = data.value;
	//	}

	//	public static explicit operator MetricDataPoint(DataPoint data)
	//	{
	//		if (data.IsAggregate())
	//			return new MetricDataPointAggregate(data);
	//		else
	//			return new MetricDataPoint(data);
	//	}
	//}

	/// <summary>
	/// Proxy for DataPoint to pass EventSource serialization in .NET Core runtime
	/// </summary>
	[System.Diagnostics.Tracing.EventData]
	internal class MetricDataPointAggregate //: MetricDataPoint
	{
		public string name { get; set; }

		public DataPointType kind { get; set; }

		public double value { get; set; }

		public int count { get; set; }

		public double min { get; set; }

		public double max { get; set; }

		public double stdDev { get; set; }

		public NonNull nonNull { get; set; }

		public MetricDataPointAggregate(DataPoint data) //: base(data, false)
		{
			name = data.name;
			kind = data.kind;
			value = data.value;
			if (data.IsAggregate())
			{
				count = data.count.GetValueOrDefault();
				nonNull |= data.count.HasValue ? NonNull.Count : NonNull.None;
				min = data.min.GetValueOrDefault();
				nonNull |= data.min.HasValue ? NonNull.Min : NonNull.None;
				max = data.max.GetValueOrDefault();
				nonNull |= data.max.HasValue? NonNull.Max: NonNull.None;
				stdDev = data.stdDev.GetValueOrDefault();
				nonNull |= data.stdDev.HasValue ? NonNull.StDev : NonNull.None;
			}
		}

		public static explicit operator MetricDataPointAggregate(DataPoint data)
		{
			return new MetricDataPointAggregate(data);
		}

		public static explicit operator DataPoint(MetricDataPointAggregate data)
		{
			DataPoint result = new DataPoint
			{
				name = data.name,
				kind = data.kind,
				value = data.value,
			};

			MetricDataPointAggregate agg = data as MetricDataPointAggregate;
			if (agg != null)
			{
				if (agg.nonNull.HasFlag(MetricDataPointAggregate.NonNull.Count))
					result.count = agg.count;
				if (agg.nonNull.HasFlag(MetricDataPointAggregate.NonNull.Min))
					result.min = agg.min;
				if (agg.nonNull.HasFlag(MetricDataPointAggregate.NonNull.Max))
					result.max = agg.max;
				if (agg.nonNull.HasFlag(MetricDataPointAggregate.NonNull.StDev))
					result.stdDev = agg.stdDev;
			}

			return result;
		}

		[Flags]
		internal enum NonNull : uint
		{
			None = 0,
			Count = (1 << 0),
			Min = (1 << 1),
			Max = (1 << 2),
			StDev = (1 << 3),
		}
	}

#endif
}