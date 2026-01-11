# UVis - Unity Visualization Runtime

A C# Vega-Lite style visualization runtime for Unity. Parse JSON specifications and render charts (bar, line, point) natively without JavaScript dependencies.

## Features

- **Declarative JSON Specifications**: Use Vega-Lite style syntax to define charts
- **Multiple Chart Types**: Bar, Line, and Point (scatter) marks
- **Dual Render Modes**: Canvas 2D (UI-based) and World Space 3D
- **Data Transforms**: Filter, aggregate, sort, and bin operations
- **Scales**: Linear, logarithmic, and band (categorical) scales
- **Axes & Legends**: Automatic axis generation with customizable styling
- **Editor Integration**: Custom inspector with JSON editing and templates

## Installation

1. Open Unity Package Manager
2. Click "+" → "Add package from disk..."
3. Navigate to the package folder and select `package.json`

Or add to your `manifest.json`:
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
4. Paste a JSON specification or select a template from the inspector
5. Click "Render"

## JSON Specification Format

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

### Supported Properties

**Mark Types**: `bar`, `line`, `point`

**Field Types**: `quantitative`, `ordinal`, `nominal`

**Scale Types**: `linear`, `log`, `band`

**Encoding Channels**: `x`, `y`, `color`, `size`

**Transforms**:
- `filter`: Expression filtering (e.g., `"datum.value > 10"`)
- `aggregate`: Group by with sum/mean/count
- `sort`: Sort by field ascending/descending
- `bin`: Histogram binning

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

Import samples from the Package Manager → UVis → Samples:
- **Basic Charts**: Bar, line, and scatter plot examples

## License

MIT License
