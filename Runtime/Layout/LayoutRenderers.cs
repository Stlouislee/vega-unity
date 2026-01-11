using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UVis.Spec;
using UVis.Scales;

namespace UVis.Layout
{
    /// <summary>
    /// Layout calculator for chart dimensions and plot area.
    /// </summary>
    public class LayoutCalculator
    {
        public Rect CalculatePlotArea(ChartSpec spec)
        {
            var padding = spec.padding ?? new PaddingSpec();
            
            // Calculate raw width and height
            float rawWidth = spec.width - padding.left - padding.right;
            float rawHeight = spec.height - padding.top - padding.bottom;
            
            float x = padding.left;
            float y = padding.bottom;
            float width = rawWidth;
            float height = rawHeight;
            
            // If either dimension is invalid, use fallback margins for BOTH to preserve aspect ratio
            bool widthInvalid = rawWidth <= 0;
            bool heightInvalid = rawHeight <= 0;
            
            if (widthInvalid || heightInvalid)
            {
                // Use 10% margins on all sides
                float marginRatio = 0.1f;
                x = spec.width * marginRatio;
                y = spec.height * marginRatio;
                width = spec.width * (1f - 2f * marginRatio);
                height = spec.height * (1f - 2f * marginRatio);
                
                // Ensure minimums
                width = Mathf.Max(width, 1f);
                height = Mathf.Max(height, 1f);
            }

            return new Rect(x, y, width, height);
        }
    }

    /// <summary>
    /// Renders chart axes with tick marks and labels.
    /// </summary>
    public class AxisRenderer
    {
        private readonly List<GameObject> _axisElements = new List<GameObject>();
        private TMP_FontAsset _font;

        public AxisRenderer(TMP_FontAsset font = null)
        {
            _font = font;
        }

        public void RenderXAxis(IScale scale, AxisSpec spec, RectTransform plotArea, bool is3D, Transform plotRoot = null)
        {
            ClearAxisElements();

            if (spec == null) return;

            var ticks = scale.GenerateTicks(spec.tickCount);

            Color labelColor = Color.white;
            if (!string.IsNullOrEmpty(spec.labelColor))
                ColorUtility.TryParseHtmlString(spec.labelColor, out labelColor);

            if (is3D)
            {
                RenderXAxis3D(scale, spec, ticks, plotRoot, labelColor);
            }
            else
            {
                RenderXAxis2D(scale, spec, ticks, plotArea, labelColor);
            }
        }

        private void RenderXAxis2D(IScale scale, AxisSpec spec, IEnumerable<ScaleTick> ticks, RectTransform plotArea, Color labelColor)
        {
            // Create axis line
            var axisLine = CreateAxisLine2D(plotArea, "XAxisLine");
            var lineRect = axisLine.GetComponent<RectTransform>();
            lineRect.anchorMin = Vector2.zero;
            lineRect.anchorMax = new Vector2(1, 0);
            lineRect.pivot = new Vector2(0.5f, 1);
            lineRect.anchoredPosition = new Vector2(0, -5);
            lineRect.sizeDelta = new Vector2(0, 2);

            // Create tick marks and labels
            foreach (var tick in ticks)
            {
                // Tick mark
                var tickMark = CreateTickMark2D(plotArea, "XTick", true);
                var tickRect = tickMark.GetComponent<RectTransform>();
                tickRect.anchorMin = Vector2.zero;
                tickRect.anchorMax = Vector2.zero;
                tickRect.pivot = new Vector2(0.5f, 1);
                tickRect.anchoredPosition = new Vector2(tick.Position, -5);

                // Label
                var label = CreateLabel2D(plotArea, tick.Label, spec.labelFontSize, labelColor);
                var labelRect = label.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.zero;
                labelRect.pivot = new Vector2(0.5f, 1);
                labelRect.anchoredPosition = new Vector2(tick.Position, -15);

                if (Mathf.Abs(spec.labelAngle) > 0.01f)
                {
                    labelRect.localRotation = Quaternion.Euler(0, 0, spec.labelAngle);
                }
            }

            // Axis title
            if (!string.IsNullOrEmpty(spec.title))
            {
                Color titleColor = Color.white;
                if (!string.IsNullOrEmpty(spec.titleColor))
                    ColorUtility.TryParseHtmlString(spec.titleColor, out titleColor);

                var title = CreateLabel2D(plotArea, spec.title, spec.titleFontSize, titleColor);
                var titleRect = title.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0.5f, 0);
                titleRect.anchorMax = new Vector2(0.5f, 0);
                titleRect.pivot = new Vector2(0.5f, 1);
                titleRect.anchoredPosition = new Vector2(0, -40);
            }
        }

        private void RenderXAxis3D(IScale scale, AxisSpec spec, IEnumerable<ScaleTick> ticks, Transform plotRoot, Color labelColor)
        {
            // Create axis line as a 3D Cube
            var axisGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            axisGo.name = "XAxisLine";
            axisGo.transform.SetParent(plotRoot, false);

            float length = scale.RangeMax - scale.RangeMin;
            // Proportional thickness: 1% of axis length, clamped for reasonable appearance
            float thickness = Mathf.Clamp(length * 0.01f, 0.02f, 0.2f);

            float outputMin = scale.RangeMin;
            float outputMax = scale.RangeMax;
            
            axisGo.transform.localPosition = new Vector3((outputMin + outputMax) / 2f, -thickness/2, 0);
            axisGo.transform.localScale = new Vector3(length, thickness, thickness);

            var renderer = axisGo.GetComponent<Renderer>();
            renderer.material = CreateAxisMaterial(Color.gray);
            
            // Remove collider
            if (Application.isPlaying) Object.Destroy(axisGo.GetComponent<Collider>());
            else Object.DestroyImmediate(axisGo.GetComponent<Collider>());

            _axisElements.Add(axisGo);

            // Create tick marks (small cubes) and labels
            foreach (var tick in ticks)
            {
                // Tick Mark - Cube
                var tickGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tickGo.name = $"XTick_{tick.Label}";
                tickGo.transform.SetParent(plotRoot, false);
                
                // Position at tick val
                tickGo.transform.localPosition = new Vector3(tick.Position, -thickness, 0);
                tickGo.transform.localScale = new Vector3(thickness/2, thickness, thickness/2);
                
                var tickRenderer = tickGo.GetComponent<Renderer>();
                tickRenderer.material = renderer.material; // reuse material
                
                if (Application.isPlaying) Object.Destroy(tickGo.GetComponent<Collider>());
                else Object.DestroyImmediate(tickGo.GetComponent<Collider>());

                _axisElements.Add(tickGo);

                // Label
                var labelGo = new GameObject($"XLabel_{tick.Label}");
                labelGo.transform.SetParent(plotRoot, false);
                
                // Font size: proportional to axis length, but capped relative to available space
                float rawFontSize = length * 0.08f;
                float fontSize = Mathf.Clamp(rawFontSize, 0.2f, 5f);
                
                // Offset: small fixed distance below axis, capped to not exceed 10% of axis length
                float labelOffset = Mathf.Min(thickness * 2f + 0.1f, length * 0.1f);
                
                labelGo.transform.localPosition = new Vector3(tick.Position, -labelOffset, 0);

                var tmp = labelGo.AddComponent<TextMeshPro>();
                tmp.text = tick.Label;
                tmp.fontSize = fontSize;
                tmp.alignment = TextAlignmentOptions.Top;
                tmp.horizontalAlignment = HorizontalAlignmentOptions.Center;
                tmp.color = labelColor;
                tmp.enableAutoSizing = false;
                
                // Set RectTransform size for proper text rendering
                var rectTransform = labelGo.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(length * 0.3f, fontSize * 2);

                if (_font != null) tmp.font = _font;

                _axisElements.Add(labelGo);
            }
        }

        public void RenderYAxis(IScale scale, AxisSpec spec, RectTransform plotArea, bool is3D, Transform plotRoot = null)
        {
            if (spec == null) return;

            var ticks = scale.GenerateTicks(spec.tickCount);

            Color labelColor = Color.white;
            if (!string.IsNullOrEmpty(spec.labelColor))
                ColorUtility.TryParseHtmlString(spec.labelColor, out labelColor);

            if (is3D)
            {
                RenderYAxis3D(scale, spec, ticks, plotRoot, labelColor);
            }
            else
            {
                RenderYAxis2D(scale, spec, ticks, plotArea, labelColor);
            }
        }

        private void RenderYAxis2D(IScale scale, AxisSpec spec, IEnumerable<ScaleTick> ticks, RectTransform plotArea, Color labelColor)
        {
            // Create axis line
            var axisLine = CreateAxisLine2D(plotArea, "YAxisLine");
            var lineRect = axisLine.GetComponent<RectTransform>();
            lineRect.anchorMin = Vector2.zero;
            lineRect.anchorMax = new Vector2(0, 1);
            lineRect.pivot = new Vector2(1, 0.5f);
            lineRect.anchoredPosition = new Vector2(-5, 0);
            lineRect.sizeDelta = new Vector2(2, 0);

            // Create tick marks and labels
            foreach (var tick in ticks)
            {
                // Tick mark
                var tickMark = CreateTickMark2D(plotArea, "YTick", false);
                var tickRect = tickMark.GetComponent<RectTransform>();
                tickRect.anchorMin = Vector2.zero;
                tickRect.anchorMax = Vector2.zero;
                tickRect.pivot = new Vector2(1, 0.5f);
                tickRect.anchoredPosition = new Vector2(-5, tick.Position);

                // Label
                var label = CreateLabel2D(plotArea, tick.Label, spec.labelFontSize, labelColor);
                var labelRect = label.GetComponent<RectTransform>();
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.zero;
                labelRect.pivot = new Vector2(1, 0.5f);
                labelRect.anchoredPosition = new Vector2(-15, tick.Position);
            }

            // Axis title (rotated 90 degrees)
            if (!string.IsNullOrEmpty(spec.title))
            {
                Color titleColor = Color.white;
                if (!string.IsNullOrEmpty(spec.titleColor))
                    ColorUtility.TryParseHtmlString(spec.titleColor, out titleColor);

                var title = CreateLabel2D(plotArea, spec.title, spec.titleFontSize, titleColor);
                var titleRect = title.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 0.5f);
                titleRect.anchorMax = new Vector2(0, 0.5f);
                titleRect.pivot = new Vector2(0.5f, 0);
                titleRect.anchoredPosition = new Vector2(-50, 0);
                titleRect.localRotation = Quaternion.Euler(0, 0, 90);
            }

            // Grid lines
            if (spec.grid)
            {
                RenderGridLines2D(scale, spec.tickCount, plotArea, true);
            }
        }

        public void RenderZAxis(IScale scale, AxisSpec spec, Transform plotRoot)
        {
            if (plotRoot == null) return;
            
            // Z axis only renders in 3D mode
            var ticks = scale.GenerateTicks(spec?.tickCount ?? 5);

            Color labelColor = Color.white;
            if (spec != null && !string.IsNullOrEmpty(spec.labelColor))
                ColorUtility.TryParseHtmlString(spec.labelColor, out labelColor);

            RenderZAxis3D(scale, spec, ticks, plotRoot, labelColor);
        }

        private void RenderZAxis3D(IScale scale, AxisSpec spec, IEnumerable<ScaleTick> ticks, Transform plotRoot, Color labelColor)
        {
            // Create Z-axis line as a 3D Cube (runs along Z direction at X=0, Y=0)
            var axisGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            axisGo.name = "ZAxisLine";
            axisGo.transform.SetParent(plotRoot, false);

            float length = scale.RangeMax - scale.RangeMin;
            // Proportional thickness: 1% of axis length, clamped for reasonable appearance
            float thickness = Mathf.Clamp(length * 0.01f, 0.02f, 0.2f);

            // Z-Axis runs along Z from min to max at origin (0,0,z)
            axisGo.transform.localPosition = new Vector3(0, 0, (scale.RangeMin + scale.RangeMax) / 2f);
            axisGo.transform.localScale = new Vector3(thickness, thickness, length);

            var renderer = axisGo.GetComponent<Renderer>();
            renderer.material = CreateAxisMaterial(Color.gray);

            if (Application.isPlaying) Object.Destroy(axisGo.GetComponent<Collider>());
            else Object.DestroyImmediate(axisGo.GetComponent<Collider>());

            _axisElements.Add(axisGo);

            // Ticks and labels along Z
            foreach (var tick in ticks)
            {
                // Skip dummy category label
                if (tick.Label == "_default") continue;
                
                // Tick Mark - Cube
                var tickGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tickGo.name = $"ZTick_{tick.Label}";
                tickGo.transform.SetParent(plotRoot, false);
                
                tickGo.transform.localPosition = new Vector3(-thickness, 0, tick.Position);
                tickGo.transform.localScale = new Vector3(thickness, thickness/2, thickness/2);
                
                var tickRenderer = tickGo.GetComponent<Renderer>();
                tickRenderer.material = renderer.material;
                
                if (Application.isPlaying) Object.Destroy(tickGo.GetComponent<Collider>());
                else Object.DestroyImmediate(tickGo.GetComponent<Collider>());

                _axisElements.Add(tickGo);

                var labelGo = new GameObject($"ZLabel_{tick.Label}");
                labelGo.transform.SetParent(plotRoot, false);
                
                // Font size: proportional to axis length, capped for very small charts
                float rawFontSize = length * 0.08f;
                float fontSize = Mathf.Clamp(rawFontSize, 0.2f, 5f);
                
                // Offset: capped to not exceed 15% of axis length
                float labelOffset = Mathf.Min(thickness * 2f + 0.1f, length * 0.15f);
                
                labelGo.transform.localPosition = new Vector3(-labelOffset, 0, tick.Position);
                // Rotate so label faces the camera
                labelGo.transform.localRotation = Quaternion.Euler(0, 45, 0);

                var tmp = labelGo.AddComponent<TextMeshPro>();
                tmp.text = tick.Label;
                tmp.fontSize = fontSize;
                tmp.alignment = TextAlignmentOptions.MidlineRight;
                tmp.color = labelColor;
                tmp.enableAutoSizing = false;
                
                var rectTransform = labelGo.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(length * 0.3f, fontSize * 2);

                if (_font != null) tmp.font = _font;

                _axisElements.Add(labelGo);
            }
        }


        private void RenderYAxis3D(IScale scale, AxisSpec spec, IEnumerable<ScaleTick> ticks, Transform plotRoot, Color labelColor)
        {
            // Create axis line as a 3D Cube
            var axisGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            axisGo.name = "YAxisLine";
            axisGo.transform.SetParent(plotRoot, false);

            float length = scale.RangeMax - scale.RangeMin;
            // Proportional thickness: 1% of axis length, clamped for reasonable appearance
            float thickness = Mathf.Clamp(length * 0.01f, 0.02f, 0.2f);
            
            
            axisGo.transform.localPosition = new Vector3(-thickness/2, (scale.RangeMin + scale.RangeMax) / 2f, 0);
            axisGo.transform.localScale = new Vector3(thickness, length, thickness);

            var renderer = axisGo.GetComponent<Renderer>();
            renderer.material = CreateAxisMaterial(Color.gray);

            if (Application.isPlaying) Object.Destroy(axisGo.GetComponent<Collider>());
            else Object.DestroyImmediate(axisGo.GetComponent<Collider>());

            _axisElements.Add(axisGo);

            // Ticks and labels
            foreach (var tick in ticks)
            {
                 // Tick Mark - Cube
                var tickGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tickGo.name = $"YTick_{tick.Label}";
                tickGo.transform.SetParent(plotRoot, false);
                
                tickGo.transform.localPosition = new Vector3(-thickness, tick.Position, 0);
                tickGo.transform.localScale = new Vector3(thickness, thickness/2, thickness/2);
                

                var tickRenderer = tickGo.GetComponent<Renderer>();
                tickRenderer.material = renderer.material;
                
                if (Application.isPlaying) Object.Destroy(tickGo.GetComponent<Collider>());
                else Object.DestroyImmediate(tickGo.GetComponent<Collider>());

                _axisElements.Add(tickGo);

                var labelGo = new GameObject($"YLabel_{tick.Label}");
                labelGo.transform.SetParent(plotRoot, false);
                
                // Font size: proportional to axis length, capped for very small charts
                float rawFontSize = length * 0.08f;
                float fontSize = Mathf.Clamp(rawFontSize, 0.2f, 5f);
                
                // Offset: capped to not exceed 15% of axis length
                float labelOffset = Mathf.Min(thickness * 2f + 0.1f, length * 0.15f);
                
                labelGo.transform.localPosition = new Vector3(-labelOffset, tick.Position, 0);

                var tmp = labelGo.AddComponent<TextMeshPro>();
                tmp.text = tick.Label;
                tmp.fontSize = fontSize;
                tmp.alignment = TextAlignmentOptions.MidlineRight;
                tmp.color = labelColor;
                tmp.enableAutoSizing = false;
                
                var rectTransform = labelGo.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(length * 0.3f, fontSize * 2);

                if (_font != null) tmp.font = _font;

                _axisElements.Add(labelGo);
            }
        }
        
        public void Render3DFrame(Rect plotBounds, float depth, Transform plotRoot)
        {
            // Create a bounding box wireframe or 'cage'
            var cageGo = new GameObject("PlotCage");
            cageGo.transform.SetParent(plotRoot, false);
            _axisElements.Add(cageGo);

            var lr = cageGo.AddComponent<LineRenderer>();
            lr.useWorldSpace = false; // Important!
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            lr.endColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            
            float x0 = plotBounds.xMin;
            float x1 = plotBounds.xMax;
            float y0 = plotBounds.yMin;
            float y1 = plotBounds.yMax;
            float z0 = 0;
            float z1 = depth;

            // Define corners of the box
            Vector3 p0 = new Vector3(x0, y0, z0);
            Vector3 p1 = new Vector3(x1, y0, z0);
            Vector3 p2 = new Vector3(x1, y1, z0);
            Vector3 p3 = new Vector3(x0, y1, z0);
            Vector3 p4 = new Vector3(x0, y0, z1);
            Vector3 p5 = new Vector3(x1, y0, z1);
            Vector3 p6 = new Vector3(x1, y1, z1);
            Vector3 p7 = new Vector3(x0, y1, z1);
            
            // Draw lines connecting them to form a box
            // We can use a continuous strip or separate segments.
            // Let's use loop: Front face -> Back face -> connecting edges
            
            Vector3[] positions = new Vector3[]
            {
                p0, p1, p2, p3, p0, // Front face loop
                p4, p5, p6, p7, p4, // Back face loop
                p5, p1, // Edge 1
                p2, p6, // Edge 2
                p7, p3, // Edge 3
                p0, p4  // Edge 4 (closing loop?) - No, LineRenderer is a strip.
            };
            
            // LineRenderer draws a continuous strip.
            // To draw a box with one LineRenderer, we need to retrace or use disconnects (not supported easily).
            // Better: use segments list carefully or multiple LineRenderers.
            // Let's iterate and create individual segments for clean look, or just one continuous path that doubles back?
            // Doubling back might look messy with transparency.
            // Let's create a single LineRenderer that traces the "cage" with minimal overlap?
            // p0->p1->p2->p3->p0->p4->p5->p6->p7->p4->(jump/gap?)
            // Actually, for a cage, maybe just 12 edges.
            // Since we want efficiency, let's use 1 LineRenderer with a path that covers most:
            // 0-1-2-3-0-4-5-6-7-4 + Back edges? 1-5, 2-6, 3-7.
            // It's hard to do without lifting the pen.
            // Simple approach: One LineRenderer for "Front", one for "Back", and 4 for "Connectors". 
            // Or just instantiate 12 cubes/cylinders for "Stereoscopic" cage?
            // User asked for "projection into a 3D cube". Wireframe is typical.
            // Let's use LineRenderer with position count = 16 (trace the box). 
            // 0-1-2-3-0-4-5-6-7-4-5 (1-5 covered), 6 (5-6), 2(6-2 covered), 3-7-6...
            // It's spaghetti. 
            // Let's just create 3 Line Loops: Front, Back, connectors.
            
            CreateLoop(cageGo, new[]{p0, p1, p2, p3}, true); // Front
            CreateLoop(cageGo, new[]{p4, p5, p6, p7}, true); // Back
            CreateLine(cageGo, p0, p4);
            CreateLine(cageGo, p1, p5);
            CreateLine(cageGo, p2, p6);
            CreateLine(cageGo, p3, p7);
        }

        private void CreateLoop(GameObject parent, Vector3[] points, bool close)
        {
            var go = new GameObject("Loop");
            go.transform.SetParent(parent.transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            Color c = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            lr.startColor = c;
            lr.endColor = c;
            
            int count = points.Length + (close ? 1 : 0);
            lr.positionCount = count;
            for(int i=0; i<points.Length; i++) lr.SetPosition(i, points[i]);
            if(close) lr.SetPosition(points.Length, points[0]);
        }

        private void CreateLine(GameObject parent, Vector3 start, Vector3 end)
        {
            var go = new GameObject("Connector");
            go.transform.SetParent(parent.transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            Color c = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            lr.startColor = c;
            lr.endColor = c;
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        private Material CreateAxisMaterial(Color color)
        {
            // Try URP shaders first (Simple Lit is more reliable than Lit), then Standard
            var shader = Shader.Find("Universal Render Pipeline/Simple Lit")
                      ?? Shader.Find("Universal Render Pipeline/Lit") 
                      ?? Shader.Find("Standard")
                      ?? Shader.Find("Unlit/Color");
            
            var material = new Material(shader);
            
            // Set color based on shader type
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color); // URP
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color); // Standard
            
            // Configure for clean matte appearance
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", 0.1f);
            if (material.HasProperty("_Glossiness"))
                material.SetFloat("_Glossiness", 0.1f);
            
            return material;
        }

        private void RenderGridLines2D(IScale scale, int tickCount, RectTransform plotArea, bool horizontal)
        {
            var ticks = scale.GenerateTicks(tickCount);
            var plotWidth = plotArea.rect.width;
            var plotHeight = plotArea.rect.height;

            foreach (var tick in ticks)
            {
                var gridLine = new GameObject(horizontal ? "HGridLine" : "VGridLine");
                gridLine.transform.SetParent(plotArea, false);

                var rect = gridLine.AddComponent<RectTransform>();
                var image = gridLine.AddComponent<UnityEngine.UI.Image>();
                image.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);

                if (horizontal)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = new Vector2(1, 0);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(0, tick.Position);
                    rect.sizeDelta = new Vector2(0, 1);
                }
                else
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = new Vector2(0, 1);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(tick.Position, 0);
                    rect.sizeDelta = new Vector2(1, 0);
                }

                _axisElements.Add(gridLine);
            }
        }

        private GameObject CreateAxisLine2D(RectTransform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            go.AddComponent<RectTransform>();
            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.gray;

            _axisElements.Add(go);
            return go;
        }

        private GameObject CreateTickMark2D(RectTransform parent, string name, bool horizontal)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = horizontal ? new Vector2(1, 5) : new Vector2(5, 1);

            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.gray;

            _axisElements.Add(go);
            return go;
        }

        private GameObject CreateLabel2D(RectTransform parent, string text, float fontSize, Color color)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);

            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;

            if (_font != null) tmp.font = _font;

            _axisElements.Add(go);
            return go;
        }

        public void ClearAxisElements()
        {
            foreach (var element in _axisElements)
            {
                if (element != null)
                {
                    if (Application.isPlaying)
                        Object.Destroy(element);
                    else
                        Object.DestroyImmediate(element);
                }
            }
            _axisElements.Clear();
        }

        public void Clear()
        {
            ClearAxisElements();
        }
    }

    /// <summary>
    /// Renders chart legends.
    /// </summary>
    public class LegendRenderer
    {
        private readonly List<GameObject> _legendElements = new List<GameObject>();
        private TMP_FontAsset _font;

        public LegendRenderer(TMP_FontAsset font = null)
        {
            _font = font;
        }

        public void RenderColorLegend(LegendSpec spec, List<string> categories, List<Color> colors, 
            RectTransform container, bool is3D)
        {
            Clear();

            if (spec == null || categories == null || categories.Count == 0)
                return;

            if (is3D)
            {
                // TODO: Implement 3D legend
                return;
            }

            // Create legend container
            var legendGo = new GameObject("Legend");
            legendGo.transform.SetParent(container, false);

            var legendRect = legendGo.AddComponent<RectTransform>();
            var vertLayout = legendGo.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            vertLayout.spacing = 5;
            vertLayout.childAlignment = TextAnchor.UpperLeft;
            vertLayout.childControlHeight = true;
            vertLayout.childControlWidth = true;

            // Position legend based on orient
            switch (spec.orient?.ToLower())
            {
                case "left":
                    legendRect.anchorMin = new Vector2(0, 0.5f);
                    legendRect.anchorMax = new Vector2(0, 0.5f);
                    legendRect.pivot = new Vector2(1, 0.5f);
                    legendRect.anchoredPosition = new Vector2(-10, 0);
                    break;
                case "top":
                    legendRect.anchorMin = new Vector2(0.5f, 1);
                    legendRect.anchorMax = new Vector2(0.5f, 1);
                    legendRect.pivot = new Vector2(0.5f, 0);
                    legendRect.anchoredPosition = new Vector2(0, 10);
                    break;
                case "bottom":
                    legendRect.anchorMin = new Vector2(0.5f, 0);
                    legendRect.anchorMax = new Vector2(0.5f, 0);
                    legendRect.pivot = new Vector2(0.5f, 1);
                    legendRect.anchoredPosition = new Vector2(0, -10);
                    break;
                default: // right
                    legendRect.anchorMin = new Vector2(1, 0.5f);
                    legendRect.anchorMax = new Vector2(1, 0.5f);
                    legendRect.pivot = new Vector2(0, 0.5f);
                    legendRect.anchoredPosition = new Vector2(10, 0);
                    break;
            }

            legendRect.sizeDelta = new Vector2(100, categories.Count * 25 + 20);

            _legendElements.Add(legendGo);

            // Title
            if (!string.IsNullOrEmpty(spec.title))
            {
                var titleGo = new GameObject("LegendTitle");
                titleGo.transform.SetParent(legendGo.transform, false);

                titleGo.AddComponent<RectTransform>();
                var tmp = titleGo.AddComponent<TextMeshProUGUI>();
                tmp.text = spec.title;
                tmp.fontSize = 14;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = Color.white;

                if (_font != null) tmp.font = _font;

                _legendElements.Add(titleGo);
            }

            // Legend items
            for (int i = 0; i < categories.Count; i++)
            {
                var itemGo = new GameObject($"LegendItem_{i}");
                itemGo.transform.SetParent(legendGo.transform, false);

                var itemRect = itemGo.AddComponent<RectTransform>();
                var horizLayout = itemGo.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                horizLayout.spacing = 5;
                horizLayout.childAlignment = TextAnchor.MiddleLeft;

                // Color swatch
                var swatchGo = new GameObject("Swatch");
                swatchGo.transform.SetParent(itemGo.transform, false);

                var swatchRect = swatchGo.AddComponent<RectTransform>();
                swatchRect.sizeDelta = new Vector2(15, 15);

                var swatchImage = swatchGo.AddComponent<UnityEngine.UI.Image>();
                swatchImage.color = i < colors.Count ? colors[i] : Color.gray;

                var layoutElement = swatchGo.AddComponent<UnityEngine.UI.LayoutElement>();
                layoutElement.minWidth = 15;
                layoutElement.minHeight = 15;

                // Label
                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(itemGo.transform, false);

                labelGo.AddComponent<RectTransform>();
                var tmp = labelGo.AddComponent<TextMeshProUGUI>();
                tmp.text = categories[i];
                tmp.fontSize = 12;
                tmp.color = Color.white;

                if (_font != null) tmp.font = _font;

                _legendElements.Add(itemGo);
                _legendElements.Add(swatchGo);
                _legendElements.Add(labelGo);
            }
        }

        public void Clear()
        {
            foreach (var element in _legendElements)
            {
                if (element != null)
                {
                    if (Application.isPlaying)
                        Object.Destroy(element);
                    else
                        Object.DestroyImmediate(element);
                }
            }
            _legendElements.Clear();
        }
    }
}
