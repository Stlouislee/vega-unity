using System;
using System.Collections.Generic;

namespace UVis.Spec
{
    /// <summary>
    /// Main specification model matching Vega-Lite subset.
    /// </summary>
    [Serializable]
    public class ChartSpec
    {
        public DataSpec data { get; set; }
        public string mark { get; set; } = "bar";
        public EncodingSpec encoding { get; set; }
        public int width { get; set; } = 640;
        public int height { get; set; } = 400;
        public PaddingSpec padding { get; set; }
        public AxisContainerSpec axis { get; set; }
        public LegendContainerSpec legend { get; set; }
        public List<TransformSpec> transform { get; set; }
    }

    /// <summary>
    /// Data specification containing values or URL reference.
    /// </summary>
    [Serializable]
    public class DataSpec
    {
        public List<Dictionary<string, object>> values { get; set; }
        public string url { get; set; }
    }

    /// <summary>
    /// Padding specification for chart margins.
    /// </summary>
    [Serializable]
    public class PaddingSpec
    {
        public float top { get; set; } = 20f;
        public float right { get; set; } = 20f;
        public float bottom { get; set; } = 40f;
        public float left { get; set; } = 60f;

        public PaddingSpec() { }

        public PaddingSpec(float top, float right, float bottom, float left)
        {
            this.top = top;
            this.right = right;
            this.bottom = bottom;
            this.left = left;
        }
    }

    /// <summary>
    /// Encoding specification for visual channels.
    /// </summary>
    [Serializable]
    public class EncodingSpec
    {
        public ChannelSpec x { get; set; }
        public ChannelSpec y { get; set; }
        public ChannelSpec z { get; set; }  // Z-axis for true 3D charts
        public ChannelSpec color { get; set; }
        public ChannelSpec size { get; set; }
    }

    /// <summary>
    /// Channel specification for a single encoding.
    /// </summary>
    [Serializable]
    public class ChannelSpec
    {
        public string field { get; set; }
        public string type { get; set; } = "quantitative"; // "quantitative", "ordinal", "nominal", "temporal"
        public ScaleSpec scale { get; set; }
        public string value { get; set; } // Constant value (e.g., color hex)
        public string aggregate { get; set; } // "sum", "mean", "count", etc.
        public string title { get; set; }
        
        /// <summary>
        /// Stack mode for bar charts. null = no stacking (grouped), "zero"/"center"/"normalize" = stacking.
        /// Default behavior: if color field is set, bars stack by default.
        /// </summary>
        public string stack { get; set; } = "zero"; // "zero", "center", "normalize", null for grouped
    }

    /// <summary>
    /// Scale specification for domain-to-range mapping.
    /// </summary>
    [Serializable]
    public class ScaleSpec
    {
        public string type { get; set; } = "linear"; // "linear", "log", "band", "point"
        public double? paddingInner { get; set; }
        public double? paddingOuter { get; set; }
        public List<object> domain { get; set; }
        public List<float> range { get; set; }
        public bool? zero { get; set; } // Include zero in domain
        public bool? nice { get; set; } // Round domain to nice values
    }

    /// <summary>
    /// Container for axis specifications.
    /// </summary>
    [Serializable]
    public class AxisContainerSpec
    {
        public AxisSpec x { get; set; }
        public AxisSpec y { get; set; }
        public AxisSpec z { get; set; }  // Z-axis for true 3D charts
    }

    /// <summary>
    /// Axis specification for ticks, labels, and styling.
    /// </summary>
    [Serializable]
    public class AxisSpec
    {
        public int tickCount { get; set; } = 5;
        public float labelAngle { get; set; } = 0f;
        public string title { get; set; }
        public bool grid { get; set; } = true;
        public string labelColor { get; set; } = "#333333";
        public float labelFontSize { get; set; } = 12f;
        public string titleColor { get; set; } = "#333333";
        public float titleFontSize { get; set; } = 14f;
    }

    /// <summary>
    /// Container for legend specifications.
    /// </summary>
    [Serializable]
    public class LegendContainerSpec
    {
        public LegendSpec color { get; set; }
        public LegendSpec size { get; set; }
    }

    /// <summary>
    /// Legend specification.
    /// </summary>
    [Serializable]
    public class LegendSpec
    {
        public string title { get; set; }
        public string orient { get; set; } = "right"; // "left", "right", "top", "bottom"
    }

    /// <summary>
    /// Transform specification for data operations.
    /// </summary>
    [Serializable]
    public class TransformSpec
    {
        // Filter transform
        public string filter { get; set; } // Expression like "datum.value > 10"

        // Aggregate transform
        public List<AggregateOpSpec> aggregate { get; set; }
        public List<string> groupby { get; set; }

        // Sort transform
        public List<SortFieldSpec> sort { get; set; }

        // Bin transform
        public BinSpec bin { get; set; }
        public string @as { get; set; } // Output field name for bin
        public string binField { get; set; } // Input field for bin
    }

    /// <summary>
    /// Aggregate operation specification.
    /// </summary>
    [Serializable]
    public class AggregateOpSpec
    {
        public string op { get; set; } // "sum", "mean", "count", "min", "max"
        public string field { get; set; }
        public string @as { get; set; } // Output field name
    }

    /// <summary>
    /// Sort field specification.
    /// </summary>
    [Serializable]
    public class SortFieldSpec
    {
        public string field { get; set; }
        public string order { get; set; } = "ascending"; // "ascending", "descending"
    }

    /// <summary>
    /// Bin specification for histogram binning.
    /// </summary>
    [Serializable]
    public class BinSpec
    {
        public int maxbins { get; set; } = 10;
        public double? step { get; set; }
        public double? extent_min { get; set; }
        public double? extent_max { get; set; }
    }

    /// <summary>
    /// Mark type enumeration.
    /// </summary>
    public enum MarkType
    {
        Bar,
        Line,
        Point
    }

    /// <summary>
    /// Field type enumeration for encoding channels.
    /// </summary>
    public enum FieldType
    {
        Quantitative,
        Ordinal,
        Nominal,
        Temporal
    }

    /// <summary>
    /// Helper to convert string field type to enum.
    /// </summary>
    public static class FieldTypeExtensions
    {
        public static FieldType ToFieldType(this string type)
        {
            return type?.ToLowerInvariant() switch
            {
                "quantitative" => FieldType.Quantitative,
                "ordinal" => FieldType.Ordinal,
                "nominal" => FieldType.Nominal,
                "temporal" => FieldType.Temporal,
                _ => FieldType.Quantitative
            };
        }

        public static bool IsOrdinal(this FieldType type)
        {
            return type == FieldType.Ordinal || type == FieldType.Nominal;
        }
    }

    /// <summary>
    /// Helper to convert string mark type to enum.
    /// </summary>
    public static class MarkTypeExtensions
    {
        public static MarkType ToMarkType(this string mark)
        {
            return mark?.ToLowerInvariant() switch
            {
                "bar" => MarkType.Bar,
                "line" => MarkType.Line,
                "point" => MarkType.Point,
                _ => MarkType.Bar
            };
        }
    }
}
