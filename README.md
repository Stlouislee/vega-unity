# UVis - Unity Visualization Runtime

A C# Vega-Lite style visualization runtime for Unity. Parse JSON specifications and render charts natively without JavaScript dependencies.

## Features

- **Declarative JSON Specifications**: Use Vega-Lite style syntax to define charts
- **Multiple Chart Types**: Bar, Line, and Point (scatter) marks
- **Stacked & Grouped Bars**: Color-encoded stacking or side-by-side grouping
- **Color Scale Mapping**: Automatic ordinal color palette for categorical data
- **Dual Render Modes**: Canvas 2D (UI-based) and World Space 3D
- **True 3D Bar Charts**: X/Y/Z encoding for 3D grid layouts
- **Data Transforms**: Filter, aggregate, sort, and bin operations
- **Scales**: Linear, logarithmic, and band (categorical) scales
- **Axes & Legends**: Automatic axis generation with customizable styling
- **Editor Integration**: Custom inspector with JSON editing and quick templates

## Installation

### Via Git URL (Recommended)

1. Open **Window → Package Manager**
2. Click **+** → **Add package from git URL**
3. Enter:
   ```
   https://github.com/Stlouislee/vege-unity.git
   ```

### Via Local Folder

Add to your `manifest.json`:
```json
{
  "dependencies": {
    "com.uvis.runtime": "file:../path/to/com.uvis.runtime"
  }
}
```

## Dependencies

- **TextMeshPro**: Required for text rendering (included in Unity)
- **Newtonsoft.Json**: Required for JSON parsing. Install via Package Manager:
  - Add `"com.unity.nuget.newtonsoft-json": "3.0.2"` to your manifest

## Quick Start

1. Add an empty GameObject to your scene
2. Add the `VegaContainer` component (UVis → Vega Container)
3. Select a render mode (Canvas2D or WorldSpace3D)
4. Select a template from **Quick Templates** dropdown or paste custom JSON
5. Click **"Render"**

## JSON Specification Examples

### Basic Bar Chart
```json
{
  "data": {
    "values": [
      {"category": "A", "value": 30},
      {"category": "B", "value": 80}
    ]
  },
  "mark": "bar",
  "encoding": {
    "x": {"field": "category", "type": "ordinal"},
    "y": {"field": "value", "type": "quantitative"},
    "color": {"value": "#4e79a7"}
  },
  "width": 640,
  "height": 400
}
```

### Stacked Bar Chart
```json
{
  "encoding": {
    "x": {"field": "category", "type": "ordinal"},
    "y": {"field": "value", "type": "quantitative"},
    "color": {"field": "series", "type": "nominal"}
  }
}
```

### Grouped Bar Chart
```json
{
  "encoding": {
    "x": {"field": "category", "type": "ordinal"},
    "y": {"field": "value", "type": "quantitative", "stack": null},
    "color": {"field": "series", "type": "nominal"}
  }
}
```

## Supported Properties

| Property | Options |
|----------|---------|
| **Mark Types** | `bar`, `line`, `point` |
| **Field Types** | `quantitative`, `ordinal`, `nominal`, `temporal` |
| **Scale Types** | `linear`, `log`, `band` |
| **Encoding Channels** | `x`, `y`, `z` (3D), `color`, `size` |
| **Stack Modes** | `"zero"` (default), `null` (grouped) |

### Transforms

- `filter`: Expression filtering (e.g., `"datum.value > 10"`)
- `aggregate`: Group by with sum/mean/count/min/max
- `sort`: Sort by field ascending/descending
- `bin`: Histogram binning with `maxbins` or `step`

## API Usage

```csharp
using UVis.Core;

// Get reference to container
var container = GetComponent<VegaContainer>();

// Set spec from JSON string
container.SetSpec(jsonString);

// Set spec from TextAsset
container.SetSpec(myTextAsset);

// Re-render
container.Render();

// Clear chart
container.Clear();

// Save screenshot
container.SaveScreenshot("chart.png");
```

## Events

```csharp
container.OnSpecChanged += (spec) => Debug.Log($"Spec changed: {spec.mark}");
container.OnChartRendered += () => Debug.Log("Chart rendered!");
```

## Samples

Quick Templates available in the Inspector dropdown:
- **Bar Chart** - Basic categorical bar chart
- **Stacked Bar Chart** - Color-stacked bars
- **Grouped Bar Chart** - Side-by-side bars with `stack: null`
- **Color Scale Demo** - Ordinal color mapping
- **Line Chart** - Time series line
- **Scatter Plot** - Point mark visualization

## License

MIT License
