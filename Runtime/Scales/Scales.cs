using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UVis.Scales
{
    /// <summary>
    /// Tick information for scale axis rendering.
    /// </summary>
    public struct ScaleTick
    {
        public object Value;
        public float Position;
        public string Label;

        public ScaleTick(object value, float position, string label)
        {
            Value = value;
            Position = position;
            Label = label;
        }
    }

    /// <summary>
    /// Interface for all scale types.
    /// Maps data domain values to visual range values.
    /// </summary>
    public interface IScale
    {
        /// <summary>
        /// Map a domain value to a range value.
        /// </summary>
        float Map(object value);

        /// <summary>
        /// Inverse map - get domain value from range value.
        /// </summary>
        object Invert(float pixel);

        /// <summary>
        /// Generate tick marks for axis rendering.
        /// </summary>
        IEnumerable<ScaleTick> GenerateTicks(int count);

        /// <summary>
        /// The minimum value of the range.
        /// </summary>
        float RangeMin { get; }

        /// <summary>
        /// The maximum value of the range.
        /// </summary>
        float RangeMax { get; }
    }

    /// <summary>
    /// Linear scale for quantitative data.
    /// Maps continuous numeric domain to continuous range.
    /// </summary>
    public class LinearScale : IScale
    {
        private readonly double _domainMin;
        private readonly double _domainMax;
        private readonly float _rangeMin;
        private readonly float _rangeMax;

        public float RangeMin => _rangeMin;
        public float RangeMax => _rangeMax;
        public double DomainMin => _domainMin;
        public double DomainMax => _domainMax;

        public LinearScale(IEnumerable<double> domain, float rangeMin, float rangeMax, bool includeZero = false, bool nice = true)
        {
            var domainList = domain.ToList();
            _domainMin = domainList.Count > 0 ? domainList.Min() : 0;
            _domainMax = domainList.Count > 0 ? domainList.Max() : 1;

            if (includeZero && _domainMin > 0)
            {
                _domainMin = 0;
            }

            // Apply nice rounding to domain
            if (nice)
            {
                NiceDomain(ref _domainMin, ref _domainMax);
            }

            _rangeMin = rangeMin;
            _rangeMax = rangeMax;
        }

        public LinearScale(double domainMin, double domainMax, float rangeMin, float rangeMax)
        {
            _domainMin = domainMin;
            _domainMax = domainMax;
            _rangeMin = rangeMin;
            _rangeMax = rangeMax;
        }

        public float Map(object value)
        {
            double x = Convert.ToDouble(value);
            if (Math.Abs(_domainMax - _domainMin) < double.Epsilon)
            {
                return (_rangeMin + _rangeMax) / 2f;
            }
            double t = (x - _domainMin) / (_domainMax - _domainMin);
            return Mathf.Lerp(_rangeMin, _rangeMax, (float)t);
        }

        public object Invert(float pixel)
        {
            if (Math.Abs(_rangeMax - _rangeMin) < float.Epsilon)
            {
                return (_domainMin + _domainMax) / 2.0;
            }
            double t = (pixel - _rangeMin) / (_rangeMax - _rangeMin);
            return _domainMin + t * (_domainMax - _domainMin);
        }

        public IEnumerable<ScaleTick> GenerateTicks(int count)
        {
            count = Math.Max(1, count);
            double step = (_domainMax - _domainMin) / count;
            
            // Find nice step
            step = NiceStep(step);

            double start = Math.Ceiling(_domainMin / step) * step;
            
            for (double v = start; v <= _domainMax + step * 0.001; v += step)
            {
                if (v >= _domainMin && v <= _domainMax)
                {
                    yield return new ScaleTick(
                        v,
                        Map(v),
                        FormatTickLabel(v)
                    );
                }
            }
        }

        private static void NiceDomain(ref double min, ref double max)
        {
            double range = max - min;
            if (range <= 0) return;

            double step = NiceStep(range / 5);
            min = Math.Floor(min / step) * step;
            max = Math.Ceiling(max / step) * step;
        }

        private static double NiceStep(double step)
        {
            if (step <= 0) return 1;
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(step)));
            double residual = step / magnitude;

            if (residual <= 1.0) return 1.0 * magnitude;
            if (residual <= 2.0) return 2.0 * magnitude;
            if (residual <= 5.0) return 5.0 * magnitude;
            return 10.0 * magnitude;
        }

        private static string FormatTickLabel(double value)
        {
            if (Math.Abs(value) >= 1000000)
                return (value / 1000000).ToString("0.#") + "M";
            if (Math.Abs(value) >= 1000)
                return (value / 1000).ToString("0.#") + "K";
            if (Math.Abs(value) < 1 && value != 0)
                return value.ToString("0.##");
            return value.ToString("0.#");
        }
    }

    /// <summary>
    /// Logarithmic scale for data with exponential distribution.
    /// </summary>
    public class LogScale : IScale
    {
        private readonly double _domainMin;
        private readonly double _domainMax;
        private readonly float _rangeMin;
        private readonly float _rangeMax;
        private readonly double _logBase;

        public float RangeMin => _rangeMin;
        public float RangeMax => _rangeMax;

        public LogScale(IEnumerable<double> domain, float rangeMin, float rangeMax, double logBase = 10)
        {
            var domainList = domain.Where(d => d > 0).ToList();
            _domainMin = domainList.Count > 0 ? domainList.Min() : 1;
            _domainMax = domainList.Count > 0 ? domainList.Max() : 10;
            _rangeMin = rangeMin;
            _rangeMax = rangeMax;
            _logBase = logBase;

            // Ensure positive domain
            if (_domainMin <= 0) _domainMin = 0.1;
            if (_domainMax <= _domainMin) _domainMax = _domainMin * 10;
        }

        public float Map(object value)
        {
            double x = Math.Max(Convert.ToDouble(value), _domainMin);
            double logMin = Math.Log(_domainMin, _logBase);
            double logMax = Math.Log(_domainMax, _logBase);
            double logX = Math.Log(x, _logBase);

            if (Math.Abs(logMax - logMin) < double.Epsilon)
            {
                return (_rangeMin + _rangeMax) / 2f;
            }

            double t = (logX - logMin) / (logMax - logMin);
            return Mathf.Lerp(_rangeMin, _rangeMax, (float)t);
        }

        public object Invert(float pixel)
        {
            double logMin = Math.Log(_domainMin, _logBase);
            double logMax = Math.Log(_domainMax, _logBase);

            if (Math.Abs(_rangeMax - _rangeMin) < float.Epsilon)
            {
                return Math.Pow(_logBase, (logMin + logMax) / 2);
            }

            double t = (pixel - _rangeMin) / (_rangeMax - _rangeMin);
            double logValue = logMin + t * (logMax - logMin);
            return Math.Pow(_logBase, logValue);
        }

        public IEnumerable<ScaleTick> GenerateTicks(int count)
        {
            double logMin = Math.Log(_domainMin, _logBase);
            double logMax = Math.Log(_domainMax, _logBase);
            
            int startPower = (int)Math.Floor(logMin);
            int endPower = (int)Math.Ceiling(logMax);

            for (int p = startPower; p <= endPower; p++)
            {
                double value = Math.Pow(_logBase, p);
                if (value >= _domainMin && value <= _domainMax)
                {
                    yield return new ScaleTick(
                        value,
                        Map(value),
                        FormatLogLabel(value)
                    );
                }
            }
        }

        private static string FormatLogLabel(double value)
        {
            if (value >= 1000000) return (value / 1000000).ToString("0.#") + "M";
            if (value >= 1000) return (value / 1000).ToString("0.#") + "K";
            if (value < 1) return value.ToString("0.###");
            return value.ToString("0.#");
        }
    }

    /// <summary>
    /// Band scale for ordinal/categorical data.
    /// Maps discrete categories to equal-width bands.
    /// </summary>
    public class BandScale : IScale
    {
        private readonly List<string> _domain;
        private readonly float _rangeMin;
        private readonly float _rangeMax;
        private readonly float _bandWidth;
        private readonly float _step;
        private readonly float _paddingInner;
        private readonly float _paddingOuter;

        public float RangeMin => _rangeMin;
        public float RangeMax => _rangeMax;
        public float BandWidth => _bandWidth;
        public float Step => _step;
        public IReadOnlyList<string> Domain => _domain;

        public BandScale(IEnumerable<string> domain, float rangeMin, float rangeMax, 
            double paddingInner = 0.1, double paddingOuter = 0.05)
        {
            _domain = domain.Distinct().ToList();
            _rangeMin = rangeMin;
            _rangeMax = rangeMax;
            _paddingInner = (float)paddingInner;
            _paddingOuter = (float)paddingOuter;

            int n = _domain.Count;
            if (n == 0)
            {
                _step = 0;
                _bandWidth = 0;
                return;
            }

            float totalRange = rangeMax - rangeMin;
            
            // Calculate step and bandwidth
            // step = bandwidth + inner padding
            // Total = 2 * outer padding + n * step - inner padding (no padding after last)
            float outerPadding = _paddingOuter * totalRange / (1 + 2 * _paddingOuter);
            float availableRange = totalRange - 2 * outerPadding;
            
            if (n == 1)
            {
                _step = availableRange;
                _bandWidth = _step;
            }
            else
            {
                _step = availableRange / n;
                _bandWidth = _step * (1 - _paddingInner);
            }
        }

        public float Map(object value)
        {
            string category = value?.ToString() ?? "";
            int index = _domain.IndexOf(category);
            if (index < 0) index = 0;

            float outerPadding = _paddingOuter * (_rangeMax - _rangeMin) / (1 + 2 * _paddingOuter);
            return _rangeMin + outerPadding + index * _step + _bandWidth / 2f;
        }

        /// <summary>
        /// Get the start position of a band (left edge for horizontal bars).
        /// </summary>
        public float MapBandStart(object value)
        {
            string category = value?.ToString() ?? "";
            int index = _domain.IndexOf(category);
            if (index < 0) index = 0;

            float outerPadding = _paddingOuter * (_rangeMax - _rangeMin) / (1 + 2 * _paddingOuter);
            return _rangeMin + outerPadding + index * _step;
        }

        public object Invert(float pixel)
        {
            float outerPadding = _paddingOuter * (_rangeMax - _rangeMin) / (1 + 2 * _paddingOuter);
            int index = (int)((pixel - _rangeMin - outerPadding) / _step);
            index = Mathf.Clamp(index, 0, _domain.Count - 1);
            return _domain.Count > 0 ? _domain[index] : "";
        }

        public IEnumerable<ScaleTick> GenerateTicks(int count)
        {
            foreach (var category in _domain)
            {
                yield return new ScaleTick(
                    category,
                    Map(category),
                    category
                );
            }
        }
    }

    /// <summary>
    /// Ordinal color scale for mapping categorical data to colors.
    /// </summary>
    public class OrdinalColorScale
    {
        private readonly List<string> _domain;
        private readonly List<Color> _colors;

        // Default Tableau10-inspired color palette
        private static readonly Color[] DefaultPalette = new Color[]
        {
            new Color(0.306f, 0.475f, 0.655f), // #4e79a7
            new Color(0.945f, 0.502f, 0.200f), // #f28e2b
            new Color(0.890f, 0.353f, 0.361f), // #e15759
            new Color(0.467f, 0.675f, 0.702f), // #76b7b2
            new Color(0.353f, 0.616f, 0.337f), // #59a14f
            new Color(0.929f, 0.788f, 0.282f), // #edc948
            new Color(0.710f, 0.494f, 0.718f), // #b07aa1
            new Color(1.000f, 0.600f, 0.667f), // #ff9da7
            new Color(0.620f, 0.533f, 0.427f), // #9c755f
            new Color(0.749f, 0.749f, 0.749f)  // #bab0ac
        };

        public IReadOnlyList<string> Domain => _domain;

        public OrdinalColorScale(IEnumerable<string> domain, IEnumerable<Color> colors = null)
        {
            _domain = domain.Distinct().ToList();
            _colors = colors?.ToList() ?? DefaultPalette.ToList();
        }

        public Color Map(object value)
        {
            string category = value?.ToString() ?? "";
            int index = _domain.IndexOf(category);
            if (index < 0) index = 0;
            return _colors[index % _colors.Count];
        }

        public static OrdinalColorScale FromData(IEnumerable<object> values)
        {
            var categories = values.Select(v => v?.ToString() ?? "").Distinct().ToList();
            return new OrdinalColorScale(categories);
        }
    }
}
