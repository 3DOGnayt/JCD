using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public class ObjectArranger : EditorWindow
    {
        public enum ShapeType { Circle, Ellipse, Square, Triangle, Line }
        public ShapeType shape = ShapeType.Circle;

        public List<GameObject> objects = new List<GameObject>();
        public Vector3 center = Vector3.zero;

        [Header("Common Settings")]
        public bool rotateToCenter = false;
        public bool fillInside = false;

        [Header("Circle / Ellipse Settings")]
        public float radiusX = 5f;
        public float radiusY = 5f;
        public float angleOffset = 0f;
        public int ringsCount = 3;
        public int elementsPerRing = 8;

        [System.Serializable]
        public class RingData
        {
            public int elements = 8;
            public float angleOffset = 0f;
            public float radiusFactor = 1f;
        }
        public List<RingData> rings = new List<RingData>();
        private bool showRingsList = true;
        private Vector2 ringsScroll;

        [Header("Square Settings")]
        public float spacing = 2f;

        [System.Serializable]
        public class SquareRing
        {
            public int elements = 8;
            public float offset = 0f;       // радиальное смещение (масштаб)
            public float angleOffset = 0f;  // поворот всего кольца
        }
        public int squareRingsCount = 1;
        public List<SquareRing> squareRings = new List<SquareRing>();
        private bool showSquareRingsList = true;
        private Vector2 squareScroll;

        [Header("Triangle / Line Settings")]
        public float width = 10f;
        public float height = 10f;

        [MenuItem("Tools/Object Arranger Pro")]
        public static void ShowWindow()
        {
            GetWindow<ObjectArranger>("Object Arranger Pro");
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Object Arranger Pro", EditorStyles.boldLabel);

            shape = (ShapeType)EditorGUILayout.EnumPopup("Shape", shape);
            center = EditorGUILayout.Vector3Field("Center", center);
            rotateToCenter = EditorGUILayout.Toggle("Rotate To Center", rotateToCenter);
            fillInside = EditorGUILayout.Toggle("Fill Inside", fillInside);
            EditorGUILayout.Space();

            switch (shape)
            {
                case ShapeType.Circle:
                case ShapeType.Ellipse:
                    DrawCircleUI();
                    break;
                case ShapeType.Square:
                    DrawSquareUI();
                    break;
                case ShapeType.Triangle:
                    width = EditorGUILayout.FloatField("Base Width", width);
                    height = EditorGUILayout.FloatField("Height", height);
                    break;
                case ShapeType.Line:
                    spacing = EditorGUILayout.FloatField("Spacing", spacing);
                    break;
            }

            EditorGUILayout.Space();
            var so = new SerializedObject(this);
            var listProperty = so.FindProperty("objects");
            EditorGUILayout.PropertyField(listProperty, true);
            so.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Arrange"))
            {
                ArrangeObjects();
            }
        }

        // ----------------------------------------------------------
        // Circle / Ellipse UI
        // ----------------------------------------------------------
        void DrawCircleUI()
        {
            radiusX = EditorGUILayout.FloatField("Radius X", radiusX);
            if (shape == ShapeType.Ellipse)
                radiusY = EditorGUILayout.FloatField("Radius Y", radiusY);
            else
                radiusY = radiusX;

            angleOffset = EditorGUILayout.Slider("Global Angle Offset", angleOffset, 0, 360);

            if (!fillInside) return;

            ringsCount = EditorGUILayout.IntField("Rings Count", Mathf.Max(1, ringsCount));
            elementsPerRing = EditorGUILayout.IntField("Elements Per Ring (default)", Mathf.Max(1, elementsPerRing));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Rings"))
                GenerateRings();
            if (GUILayout.Button("Auto Distribute"))
                AutoDistributeElementsToRings();
            EditorGUILayout.EndHorizontal();

            if (rings != null && rings.Count > 0)
            {
                showRingsList = EditorGUILayout.Foldout(showRingsList, $"Rings ({rings.Count})", true);
                if (showRingsList)
                {
                    ringsScroll = EditorGUILayout.BeginScrollView(ringsScroll, GUILayout.MaxHeight(250));
                    for (var i = 0; i < rings.Count; i++)
                    {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"Ring {i + 1}", EditorStyles.boldLabel);
                        rings[i].elements = EditorGUILayout.IntField("Elements", Mathf.Max(0, rings[i].elements));
                        rings[i].angleOffset = EditorGUILayout.FloatField("Angle Offset", rings[i].angleOffset);
                        rings[i].radiusFactor = EditorGUILayout.Slider("Radius Factor", rings[i].radiusFactor, 0f, 1f);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        // ----------------------------------------------------------
        // Square UI
        // ----------------------------------------------------------
        void DrawSquareUI()
        {
            spacing = EditorGUILayout.FloatField("Spacing", spacing);
            if (!fillInside) return;

            squareRingsCount = EditorGUILayout.IntField("Square Rings Count", Mathf.Max(1, squareRingsCount));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Square Rings"))
                GenerateSquareRings();
            if (GUILayout.Button("Auto Distribute"))
                AutoDistributeSquareRings();
            EditorGUILayout.EndHorizontal();

            if (squareRings != null && squareRings.Count > 0)
            {
                showSquareRingsList = EditorGUILayout.Foldout(showSquareRingsList, $"Square Rings ({squareRings.Count})", true);
                if (showSquareRingsList)
                {
                    squareScroll = EditorGUILayout.BeginScrollView(squareScroll, GUILayout.MaxHeight(250));
                    for (var i = 0; i < squareRings.Count; i++)
                    {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"Ring {i + 1}", EditorStyles.boldLabel);
                        squareRings[i].elements = EditorGUILayout.IntField("Elements", Mathf.Max(0, squareRings[i].elements));
                        squareRings[i].offset = EditorGUILayout.Slider("Offset", squareRings[i].offset, 0f, 1f);
                        squareRings[i].angleOffset = EditorGUILayout.Slider("Angle Offset", squareRings[i].angleOffset, 0f, 360f);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        // ----------------------------------------------------------
        // Circle helpers
        // ----------------------------------------------------------
        void GenerateRings()
        {
            rings = new List<RingData>();
            for (var i = 0; i < ringsCount; i++)
            {
                rings.Add(new RingData
                {
                    elements = elementsPerRing,
                    angleOffset = 0f,
                    radiusFactor = (i + 1f) / ringsCount
                });
            }
            AutoDistributeElementsToRings();
        }

        void AutoDistributeElementsToRings()
        {
            if (rings == null || rings.Count == 0 || objects.Count == 0) return;
            var totalObjects = objects.Count;
            var baseCount = totalObjects / rings.Count;
            var rem = totalObjects % rings.Count;
            for (var i = 0; i < rings.Count; i++)
                rings[i].elements = baseCount + (i < rem ? 1 : 0);
        }

        // ----------------------------------------------------------
        // Square helpers
        // ----------------------------------------------------------
        void GenerateSquareRings()
        {
            squareRings = new List<SquareRing>();
            for (var i = 0; i < squareRingsCount; i++)
            {
                squareRings.Add(new SquareRing
                {
                    elements = 8,
                    offset = (i + 1f) / squareRingsCount,
                    angleOffset = 0f
                });
            }
            AutoDistributeSquareRings();
        }

        void AutoDistributeSquareRings()
        {
            if (squareRings == null || squareRings.Count == 0 || objects.Count == 0) return;
            var total = objects.Count;
            var baseCount = total / squareRings.Count;
            var rem = total % squareRings.Count;
            for (var i = 0; i < squareRings.Count; i++)
                squareRings[i].elements = baseCount + (i < rem ? 1 : 0);
        }

        // ----------------------------------------------------------
        // Arrange
        // ----------------------------------------------------------
        void ArrangeObjects()
        {
            if (objects == null || objects.Count == 0)
            {
                Debug.LogWarning("No objects assigned!");
                return;
            }

            switch (shape)
            {
                case ShapeType.Circle:
                    ArrangeCircle();
                    break;
                case ShapeType.Ellipse:
                    ArrangeCircle();
                    break;
                case ShapeType.Square:
                    ArrangeSquare();
                    break;
                case ShapeType.Triangle:
                    ArrangeTriangle();
                    break;
                case ShapeType.Line:
                    ArrangeLine();
                    break;
            }

            if (rotateToCenter)
                RotateAllToCalculatedCenter();
            else
                ResetAllRotations();
        }

        void ArrangeCircle()
        {
            if (!fillInside || rings == null || rings.Count == 0)
            {
                var count = objects.Count;
                var step = 360f / count;
                for (var i = 0; i < count; i++)
                {
                    var a = (i * step + angleOffset) * Mathf.Deg2Rad;
                    var pos = center + new Vector3(Mathf.Cos(a) * radiusX, 0, Mathf.Sin(a) * radiusY);
                    Undo.RecordObject(objects[i].transform, "Arrange Circle");
                    objects[i].transform.position = pos;
                }
                return;
            }

            var index = 0;
            for (var r = 0; r < rings.Count && index < objects.Count; r++)
            {
                var ring = rings[r];
                var elems = Mathf.Max(0, ring.elements);
                if (elems == 0) continue;

                var ringRadiusX = radiusX * ring.radiusFactor;
                var ringRadiusY = radiusY * ring.radiusFactor;
                var step = 360f / elems;

                for (var i = 0; i < elems && index < objects.Count; i++)
                {
                    var a = (i * step + angleOffset + ring.angleOffset) * Mathf.Deg2Rad;
                    var pos = center + new Vector3(Mathf.Cos(a) * ringRadiusX, 0, Mathf.Sin(a) * ringRadiusY);
                    if (float.IsNaN(pos.x)) continue;
                    Undo.RecordObject(objects[index].transform, "Arrange Circle Fill");
                    objects[index].transform.position = pos;
                    index++;
                }
            }
        }

        void ArrangeSquare()
        {
            if (!fillInside || squareRings == null || squareRings.Count == 0)
            {
                ArrangeSquarePerimeter();
                return;
            }

            var index = 0;
            foreach (var ring in squareRings)
            {
                var elems = Mathf.Max(1, ring.elements);
                if (index >= objects.Count) break;

                // Размер текущего кольца
                var half = spacing * ring.offset * squareRings.Count;

                // Углы квадрата
                Vector3[] corners =
                {
                    center + new Vector3(-half, 0, -half), // 1 — нижний левый
                    center + new Vector3(half, 0, -half), // 2 — нижний правый
                    center + new Vector3(half, 0, half), // 3 — верхний правый
                    center + new Vector3(-half, 0, half) // 4 — верхний левый
                };

                // Сначала ставим углы
                for (var c = 0; c < 4 && index < objects.Count; c++)
                {
                    var dir = corners[c] - center;
                    var pos = center + Quaternion.Euler(0, ring.angleOffset, 0) * dir;
                    Undo.RecordObject(objects[index].transform, "Arrange Square Corners");
                    objects[index].transform.position = pos;
                    index++;
                }

                // Теперь распределяем оставшиеся элементы по сторонам
                var remaining = elems - 4;
                if (remaining <= 0) continue;

                var perSide = Mathf.FloorToInt(remaining / 4f);
                var extra = remaining % 4;

                for (var s = 0; s < 4 && index < objects.Count; s++)
                {
                    var start = corners[s];
                    var end = corners[(s + 1) % 4];
                    var countOnThisSide = perSide + (s < extra ? 1 : 0);

                    for (var i = 0; i < countOnThisSide && index < objects.Count; i++)
                    {
                        var t = (float)(i + 1) / (countOnThisSide + 1); // равномерное распределение между углами
                        var pos = Vector3.Lerp(start, end, t);
                        var dir = pos - center;
                        pos = center + Quaternion.Euler(0, ring.angleOffset, 0) * dir;

                        Undo.RecordObject(objects[index].transform, "Arrange Square Sides");
                        objects[index].transform.position = pos;
                        index++;
                    }
                }
            }
        }


        void ArrangeSquarePerimeter()
        {
            var count = objects.Count;
            var perSide = Mathf.CeilToInt(count / 4f);
            var index = 0;
            for (var x = 0; x < perSide && index < count; x++)
                objects[index++].transform.position = center + new Vector3((x - perSide / 2f) * spacing, 0, -perSide / 2f * spacing);

            for (var z = 0; z < perSide && index < count; z++)
                objects[index++].transform.position = center + new Vector3(perSide / 2f * spacing, 0, (z - perSide / 2f) * spacing);

            for (var x = perSide; x > 0 && index < count; x--)
                objects[index++].transform.position = center + new Vector3((x - perSide / 2f) * spacing, 0, perSide / 2f * spacing);

            for (var z = perSide; z > 0 && index < count; z--)
                objects[index++].transform.position = center + new Vector3(-perSide / 2f * spacing, 0, (z - perSide / 2f) * spacing);
        }

        void ArrangeTriangle()
        {
            var count = objects.Count;
            var rowsCalc = Mathf.CeilToInt(Mathf.Sqrt(count * 2f));
            var index = 0;
            for (var row = 0; row < rowsCalc && index < count; row++)
            {
                var itemsInRow = row + 1;
                var rowWidth = width * ((float)(row + 1) / rowsCalc);
                for (var i = 0; i < itemsInRow && index < count; i++)
                {
                    var t = (itemsInRow == 1) ? 0.5f : (float)i / (itemsInRow - 1);
                    var x = Mathf.Lerp(-rowWidth / 2f, rowWidth / 2f, t);
                    var z = -((float)row / rowsCalc) * height;
                    Undo.RecordObject(objects[index].transform, "Arrange Triangle");
                    objects[index].transform.position = center + new Vector3(x, 0, z);
                    index++;
                }
            }
        }

        void ArrangeLine()
        {
            var count = objects.Count;
            for (var i = 0; i < count; i++)
            {
                var pos = center + new Vector3((i - (count - 1) / 2f) * spacing, 0, 0);
                Undo.RecordObject(objects[i].transform, "Arrange Line");
                objects[i].transform.position = pos;
            }
        }

        // ----------------------------------------------------------
        // Utils
        // ----------------------------------------------------------
        void RotateAllToCalculatedCenter()
        {
            var avg = Vector3.zero;
            var count = 0;
            foreach (var obj in objects)
            {
                if (obj) { avg += obj.transform.position; count++; }
            }
            if (count == 0) return;
            avg /= count;
            foreach (var obj in objects)
            {
                if (obj)
                {
                    Undo.RecordObject(obj.transform, "Rotate to Center");
                    obj.transform.LookAt(avg);
                }
            }
        }

        void ResetAllRotations()
        {
            foreach (var obj in objects)
            {
                if (obj)
                {
                    Undo.RecordObject(obj.transform, "Reset Rotation");
                    obj.transform.rotation = Quaternion.identity;
                }
            }
        }
    }
}