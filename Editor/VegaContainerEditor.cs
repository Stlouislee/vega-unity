using UnityEngine;
using UnityEditor;
using UVis.Core;

namespace UVis.Editor
{
    /// <summary>
    /// Menu items for creating UVis objects from Hierarchy context menu.
    /// </summary>
    public static class UVisMenuItems
    {
        private const string DEFAULT_BAR_CHART = @"{
  ""data"": {
    ""values"": [
      {""category"": ""A"", ""value"": 30},
      {""category"": ""B"", ""value"": 80},
      {""category"": ""C"", ""value"": 45},
      {""category"": ""D"", ""value"": 60}
    ]
  },
  ""mark"": ""bar"",
  ""encoding"": {
    ""x"": {""field"": ""category"", ""type"": ""ordinal""},
    ""y"": {""field"": ""value"", ""type"": ""quantitative""},
    ""color"": {""value"": ""#4e79a7""}
  },
  ""width"": 640,
  ""height"": 400
}";

        /// <summary>
        /// Create a VegaContainer (2D Canvas mode) from Hierarchy menu.
        /// </summary>
        [MenuItem("GameObject/UVis/Vega Container (2D)", false, 10)]
        public static void CreateVegaContainer2D(MenuCommand menuCommand)
        {
            var go = new GameObject("VegaChart");
            var container = go.AddComponent<VegaContainer>();
            
            // Set default spec via reflection (since field is private)
            var field = typeof(VegaContainer).GetField("_chartSpecJson", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(container, DEFAULT_BAR_CHART);

            // Parent to context object if available
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            // Register undo
            Undo.RegisterCreatedObjectUndo(go, "Create Vega Container (2D)");
            
            // Select the new object
            Selection.activeGameObject = go;
        }

        /// <summary>
        /// Create a VegaContainer (3D World Space mode) from Hierarchy menu.
        /// </summary>
        [MenuItem("GameObject/UVis/Vega Container (3D)", false, 11)]
        public static void CreateVegaContainer3D(MenuCommand menuCommand)
        {
            var go = new GameObject("VegaChart3D");
            var container = go.AddComponent<VegaContainer>();
            
            // Set 3D mode via reflection
            var modeField = typeof(VegaContainer).GetField("_renderMode", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            modeField?.SetValue(container, VegaContainer.RenderMode.WorldSpace3D);
            
            // Set default spec
            var specField = typeof(VegaContainer).GetField("_chartSpecJson", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            specField?.SetValue(container, DEFAULT_BAR_CHART);

            // Parent to context object if available
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

            // Register undo
            Undo.RegisterCreatedObjectUndo(go, "Create Vega Container (3D)");
            
            // Select the new object
            Selection.activeGameObject = go;
        }

        /// <summary>
        /// Create a Bar Chart from Hierarchy menu.
        /// </summary>
        [MenuItem("GameObject/UVis/Charts/Bar Chart", false, 20)]
        public static void CreateBarChart(MenuCommand menuCommand)
        {
            CreateVegaContainer2D(menuCommand);
            Selection.activeGameObject.name = "BarChart";
        }

        /// <summary>
        /// Create a Line Chart from Hierarchy menu.
        /// </summary>
        [MenuItem("GameObject/UVis/Charts/Line Chart", false, 21)]
        public static void CreateLineChart(MenuCommand menuCommand)
        {
            var go = new GameObject("LineChart");
            var container = go.AddComponent<VegaContainer>();
            
            var lineChartSpec = @"{
  ""data"": {
    ""values"": [
      {""x"": 0, ""y"": 10},
      {""x"": 1, ""y"": 25},
      {""x"": 2, ""y"": 15},
      {""x"": 3, ""y"": 40},
      {""x"": 4, ""y"": 35}
    ]
  },
  ""mark"": ""line"",
  ""encoding"": {
    ""x"": {""field"": ""x"", ""type"": ""quantitative""},
    ""y"": {""field"": ""y"", ""type"": ""quantitative""},
    ""color"": {""value"": ""#f28e2c""}
  },
  ""width"": 640,
  ""height"": 400
}";
            
            var field = typeof(VegaContainer).GetField("_chartSpecJson", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(container, lineChartSpec);

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Line Chart");
            Selection.activeGameObject = go;
        }

        /// <summary>
        /// Create a Scatter Plot from Hierarchy menu.
        /// </summary>
        [MenuItem("GameObject/UVis/Charts/Scatter Plot", false, 22)]
        public static void CreateScatterPlot(MenuCommand menuCommand)
        {
            var go = new GameObject("ScatterPlot");
            var container = go.AddComponent<VegaContainer>();
            
            var scatterSpec = @"{
  ""data"": {
    ""values"": [
      {""x"": 10, ""y"": 20, ""size"": 5},
      {""x"": 25, ""y"": 15, ""size"": 10},
      {""x"": 40, ""y"": 35, ""size"": 7},
      {""x"": 55, ""y"": 45, ""size"": 12},
      {""x"": 70, ""y"": 30, ""size"": 8}
    ]
  },
  ""mark"": ""point"",
  ""encoding"": {
    ""x"": {""field"": ""x"", ""type"": ""quantitative""},
    ""y"": {""field"": ""y"", ""type"": ""quantitative""},
    ""size"": {""field"": ""size""},
    ""color"": {""value"": ""#e15759""}
  },
  ""width"": 640,
  ""height"": 400
}";
            
            var field = typeof(VegaContainer).GetField("_chartSpecJson", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(container, scatterSpec);

            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Scatter Plot");
            Selection.activeGameObject = go;
        }
    }

    /// <summary>
    /// Custom editor for VegaContainer with enhanced specification editing.
    /// </summary>
    [CustomEditor(typeof(VegaContainer))]
    public class VegaContainerEditor : UnityEditor.Editor
    {
        private SerializedProperty _renderModeProp;
        private SerializedProperty _chartSpecAssetProp;
        private SerializedProperty _chartSpecJsonProp;
        private SerializedProperty _targetCanvasProp;
        private SerializedProperty _defaultMaterial2DProp;
        private SerializedProperty _plotRootProp;
        private SerializedProperty _defaultMaterial3DProp;
        private SerializedProperty _pixelScaleProp;
        private SerializedProperty _fontProp;

        private bool _showJsonEditor = true;
        private Vector2 _jsonScrollPos;

        private static readonly string[] TEMPLATE_NAMES = new string[]
        {
            "Bar Chart",
            "Stacked Bar Chart",
            "Grouped Bar Chart",
            "Color Scale Demo",
            "Line Chart",
            "Scatter Plot",
            "Custom..."
        };

        private static readonly string[] TEMPLATE_JSON = new string[]
        {
            // Bar Chart
            @"{
  ""data"": {
    ""values"": [
      {""category"": ""A"", ""value"": 30},
      {""category"": ""B"", ""value"": 80},
      {""category"": ""C"", ""value"": 45},
      {""category"": ""D"", ""value"": 60}
    ]
  },
  ""mark"": ""bar"",
  ""encoding"": {
    ""x"": {""field"": ""category"", ""type"": ""ordinal""},
    ""y"": {""field"": ""value"", ""type"": ""quantitative""},
    ""color"": {""value"": ""#4e79a7""}
  },
  ""width"": 640,
  ""height"": 400
}",
            // Stacked Bar Chart
            @"{
  ""data"": {
    ""values"": [
      {""category"": ""A"", ""series"": ""Q1"", ""value"": 30},
      {""category"": ""A"", ""series"": ""Q2"", ""value"": 25},
      {""category"": ""A"", ""series"": ""Q3"", ""value"": 40},
      {""category"": ""B"", ""series"": ""Q1"", ""value"": 50},
      {""category"": ""B"", ""series"": ""Q2"", ""value"": 35},
      {""category"": ""B"", ""series"": ""Q3"", ""value"": 45},
      {""category"": ""C"", ""series"": ""Q1"", ""value"": 40},
      {""category"": ""C"", ""series"": ""Q2"", ""value"": 60},
      {""category"": ""C"", ""series"": ""Q3"", ""value"": 30}
    ]
  },
  ""mark"": ""bar"",
  ""encoding"": {
    ""x"": {""field"": ""category"", ""type"": ""ordinal""},
    ""y"": {""field"": ""value"", ""type"": ""quantitative""},
    ""color"": {""field"": ""series"", ""type"": ""nominal""}
  },
  ""width"": 640,
  ""height"": 400
}",
            // Grouped Bar Chart (stack: null for side-by-side bars)
            @"{
  ""data"": {
    ""values"": [
      {""category"": ""A"", ""quarter"": ""Q1"", ""sales"": 30},
      {""category"": ""A"", ""quarter"": ""Q2"", ""sales"": 45},
      {""category"": ""A"", ""quarter"": ""Q3"", ""sales"": 35},
      {""category"": ""B"", ""quarter"": ""Q1"", ""sales"": 50},
      {""category"": ""B"", ""quarter"": ""Q2"", ""sales"": 40},
      {""category"": ""B"", ""quarter"": ""Q3"", ""sales"": 55},
      {""category"": ""C"", ""quarter"": ""Q1"", ""sales"": 25},
      {""category"": ""C"", ""quarter"": ""Q2"", ""sales"": 60},
      {""category"": ""C"", ""quarter"": ""Q3"", ""sales"": 45}
    ]
  },
  ""mark"": ""bar"",
  ""encoding"": {
    ""x"": {""field"": ""category"", ""type"": ""ordinal""},
    ""y"": {""field"": ""sales"", ""type"": ""quantitative"", ""stack"": null},
    ""color"": {""field"": ""quarter"", ""type"": ""nominal""}
  },
  ""width"": 640,
  ""height"": 400
}",
            // Color Scale Demo
            @"{
  ""data"": {
    ""values"": [
      {""month"": ""Jan"", ""product"": ""A"", ""sales"": 120},
      {""month"": ""Jan"", ""product"": ""B"", ""sales"": 80},
      {""month"": ""Jan"", ""product"": ""C"", ""sales"": 150},
      {""month"": ""Feb"", ""product"": ""A"", ""sales"": 140},
      {""month"": ""Feb"", ""product"": ""B"", ""sales"": 95},
      {""month"": ""Feb"", ""product"": ""C"", ""sales"": 130},
      {""month"": ""Mar"", ""product"": ""A"", ""sales"": 160},
      {""month"": ""Mar"", ""product"": ""B"", ""sales"": 110},
      {""month"": ""Mar"", ""product"": ""C"", ""sales"": 145}
    ]
  },
  ""mark"": ""bar"",
  ""encoding"": {
    ""x"": {""field"": ""month"", ""type"": ""ordinal""},
    ""y"": {""field"": ""sales"", ""type"": ""quantitative""},
    ""color"": {""field"": ""product"", ""type"": ""nominal""}
  },
  ""width"": 640,
  ""height"": 400
}",
            // Line Chart
            @"{
  ""data"": {
    ""values"": [
      {""x"": 0, ""y"": 10},
      {""x"": 1, ""y"": 25},
      {""x"": 2, ""y"": 15},
      {""x"": 3, ""y"": 40},
      {""x"": 4, ""y"": 35}
    ]
  },
  ""mark"": ""line"",
  ""encoding"": {
    ""x"": {""field"": ""x"", ""type"": ""quantitative""},
    ""y"": {""field"": ""y"", ""type"": ""quantitative""},
    ""color"": {""value"": ""#f28e2c""}
  },
  ""width"": 640,
  ""height"": 400
}",
            // Scatter Plot
            @"{
  ""data"": {
    ""values"": [
      {""x"": 10, ""y"": 20, ""size"": 5},
      {""x"": 25, ""y"": 15, ""size"": 10},
      {""x"": 40, ""y"": 35, ""size"": 7},
      {""x"": 55, ""y"": 45, ""size"": 12},
      {""x"": 70, ""y"": 30, ""size"": 8}
    ]
  },
  ""mark"": ""point"",
  ""encoding"": {
    ""x"": {""field"": ""x"", ""type"": ""quantitative""},
    ""y"": {""field"": ""y"", ""type"": ""quantitative""},
    ""size"": {""field"": ""size""},
    ""color"": {""value"": ""#e15759""}
  },
  ""width"": 640,
  ""height"": 400
}"
        };

        private void OnEnable()
        {
            _renderModeProp = serializedObject.FindProperty("_renderMode");
            _chartSpecAssetProp = serializedObject.FindProperty("_chartSpecAsset");
            _chartSpecJsonProp = serializedObject.FindProperty("_chartSpecJson");
            _targetCanvasProp = serializedObject.FindProperty("_targetCanvas");
            _defaultMaterial2DProp = serializedObject.FindProperty("_defaultMaterial2D");
            _plotRootProp = serializedObject.FindProperty("_plotRoot");
            _defaultMaterial3DProp = serializedObject.FindProperty("_defaultMaterial3D");
            _pixelScaleProp = serializedObject.FindProperty("_pixelScale");
            _fontProp = serializedObject.FindProperty("_font");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var container = (VegaContainer)target;

            // Render Settings
            EditorGUILayout.LabelField("Render Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_renderModeProp);
            EditorGUILayout.Space();

            // Mode-specific settings
            var renderMode = (VegaContainer.RenderMode)_renderModeProp.enumValueIndex;
            if (renderMode == VegaContainer.RenderMode.Canvas2D)
            {
                EditorGUILayout.LabelField("2D Mode Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_targetCanvasProp);
                EditorGUILayout.PropertyField(_defaultMaterial2DProp);
            }
            else
            {
                EditorGUILayout.LabelField("3D Mode Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_plotRootProp);
                EditorGUILayout.PropertyField(_defaultMaterial3DProp);
                EditorGUILayout.PropertyField(_pixelScaleProp);
            }

            EditorGUILayout.Space();

            // Typography
            EditorGUILayout.LabelField("Typography", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_fontProp);
            EditorGUILayout.Space();

            // Specification
            EditorGUILayout.LabelField("Chart Specification", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_chartSpecAssetProp, new GUIContent("Spec Asset"));
            
            // Template dropdown
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Quick Templates");
            int templateIndex = EditorGUILayout.Popup(-1, TEMPLATE_NAMES);
            EditorGUILayout.EndHorizontal();

            if (templateIndex >= 0 && templateIndex < TEMPLATE_JSON.Length)
            {
                _chartSpecJsonProp.stringValue = TEMPLATE_JSON[templateIndex];
                serializedObject.ApplyModifiedProperties();
            }

            // JSON editor
            _showJsonEditor = EditorGUILayout.Foldout(_showJsonEditor, "JSON Editor", true);
            if (_showJsonEditor)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                _jsonScrollPos = EditorGUILayout.BeginScrollView(_jsonScrollPos, GUILayout.Height(200));
                EditorGUI.BeginChangeCheck();
                string json = EditorGUILayout.TextArea(_chartSpecJsonProp.stringValue, 
                    GUILayout.ExpandHeight(true));
                if (EditorGUI.EndChangeCheck())
                {
                    _chartSpecJsonProp.stringValue = json;
                }
                EditorGUILayout.EndScrollView();

                // Validation
                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        Newtonsoft.Json.JsonConvert.DeserializeObject<UVis.Spec.ChartSpec>(json);
                        EditorGUILayout.HelpBox("JSON is valid", MessageType.Info);
                    }
                    catch (System.Exception ex)
                    {
                        EditorGUILayout.HelpBox($"JSON Error: {ex.Message}", MessageType.Error);
                    }
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            // Actions
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Render", GUILayout.Height(30)))
            {
                container.Render();
            }

            if (GUILayout.Button("Clear", GUILayout.Height(30)))
            {
                container.Clear();
            }
            
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Format JSON"))
            {
                try
                {
                    var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(_chartSpecJsonProp.stringValue);
                    _chartSpecJsonProp.stringValue = Newtonsoft.Json.JsonConvert.SerializeObject(obj, 
                        Newtonsoft.Json.Formatting.Indented);
                }
                catch
                {
                    // Ignore format errors
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
