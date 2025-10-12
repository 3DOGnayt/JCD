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

        // --- Новый функционал для треугольников ---
        [System.Serializable]
        public class TriangleRing
        {
            public int elements = 8;
            public float offset = 0f;       // от 0 (центр) до 1 (внешний контур)
            public float angleOffset = 0f;  // поворот треугольника
        }

        public int triangleRingsCount = 1;
        public List<TriangleRing> triangleRings = new List<TriangleRing>();

        private bool showTriangleRingsList = true;
        private Vector2 triangleScroll;


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
                    DrawTriangleUI();
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
        
        void DrawTriangleUI()
        {
            width = EditorGUILayout.FloatField("Base Width", width);
            height = EditorGUILayout.FloatField("Height", height);

            if (!fillInside) return;

            triangleRingsCount = EditorGUILayout.IntField("Triangle Rings Count", Mathf.Max(1, triangleRingsCount));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Triangle Rings"))
                GenerateTriangleRings();
            if (GUILayout.Button("Auto Distribute"))
                AutoDistributeTriangleRings();
            EditorGUILayout.EndHorizontal();

            if (triangleRings != null && triangleRings.Count > 0)
            {
                showTriangleRingsList = EditorGUILayout.Foldout(showTriangleRingsList, $"Triangle Rings ({triangleRings.Count})", true);
                if (showTriangleRingsList)
                {
                    triangleScroll = EditorGUILayout.BeginScrollView(triangleScroll, GUILayout.MaxHeight(250));
                    for (int i = 0; i < triangleRings.Count; i++)
                    {
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"Ring {i + 1}", EditorStyles.boldLabel);
                        triangleRings[i].elements = EditorGUILayout.IntField("Elements", Mathf.Max(1, triangleRings[i].elements));
                        triangleRings[i].offset = EditorGUILayout.Slider("Offset", triangleRings[i].offset, 0f, 1f);
                        triangleRings[i].angleOffset = EditorGUILayout.Slider("Angle Offset", triangleRings[i].angleOffset, 0f, 360f);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
        }
        
        void GenerateTriangleRings()
        {
            triangleRings = new List<TriangleRing>();
            for (int i = 0; i < triangleRingsCount; i++)
            {
                triangleRings.Add(new TriangleRing
                {
                    elements = 6,
                    offset = (i + 1f) / triangleRingsCount, // по умолчанию равномерно от центра к внешнему
                    angleOffset = 0f
                });
            }
            AutoDistributeTriangleRings();
        }

        void AutoDistributeTriangleRings()
        {
            if (triangleRings == null || triangleRings.Count == 0 || objects.Count == 0) return;
            int total = objects.Count;
            int baseCount = total / triangleRings.Count;
            int rem = total % triangleRings.Count;
            for (int i = 0; i < triangleRings.Count; i++)
                triangleRings[i].elements = baseCount + (i < rem ? 1 : 0);
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
            int count = objects.Count;
            if (count == 0) return;

            // вершины внешнего треугольника (A - верхняя, B - лев., C - прав.)
            Vector3 A_out = center + new Vector3(0, 0, height / 2f);
            Vector3 B_out = center + new Vector3(-width / 2f, 0, -height / 2f);
            Vector3 C_out = center + new Vector3(width / 2f, 0, -height / 2f);

            // если нет заполнения — контур: первые три на вершины, потом по рёбрам (как уже устроили)
            if (!fillInside)
            {
                if (count <= 3)
                {
                    if (count > 0)
                    {
                        Undo.RecordObject(objects[0].transform, "Arrange Triangle A");
                        objects[0].transform.position = A_out;
                    }

                    if (count > 1)
                    {
                        Undo.RecordObject(objects[1].transform, "Arrange Triangle B");
                        objects[1].transform.position = B_out;
                    }

                    if (count > 2)
                    {
                        Undo.RecordObject(objects[2].transform, "Arrange Triangle C");
                        objects[2].transform.position = C_out;
                    }

                    return;
                }

                // верши ны A,B,C
                Undo.RecordObjects(objects.ToArray(), "Arrange Triangle Contour");
                objects[0].transform.position = A_out;
                objects[1].transform.position = B_out;
                objects[2].transform.position = C_out;

                int remaining = count - 3;
                int perSide = remaining / 3;
                int extra = remaining % 3;
                int idx = 3;

                // локальная функция вставки между двумя вершинами
                void PlaceBetween(Vector3 start, Vector3 end, int countOnSide)
                {
                    for (int i = 0; i < countOnSide && idx < objects.Count; i++)
                    {
                        float t = (i + 1f) / (countOnSide + 1f);
                        Vector3 pos = Vector3.Lerp(start, end, t);
                        Undo.RecordObject(objects[idx].transform, "Arrange Triangle Edge");
                        objects[idx].transform.position = pos;
                        idx++;
                    }
                }

                PlaceBetween(A_out, B_out, perSide + (extra > 0 ? 1 : 0));
                PlaceBetween(B_out, C_out, perSide + (extra > 1 ? 1 : 0));
                PlaceBetween(C_out, A_out, perSide);
                return;
            }

            // --------- fillInside == true: кольца треугольников ----------
            if (triangleRings == null || triangleRings.Count == 0)
            {
                // если колец нет — просто сделать одно внешнее
                triangleRings = new List<TriangleRing>
                    { new TriangleRing { elements = count, offset = 1f, angleOffset = 0f } };
            }

            // считаем центроид (центр масс) внешнего треугольника
            Vector3 centroid = (A_out + B_out + C_out) / 3f;

            int objIndex = 0;

            for (int ringIdx = 0; ringIdx < triangleRings.Count && objIndex < count; ringIdx++)
            {
                var ring = triangleRings[ringIdx];
                int elems = Mathf.Max(1, ring.elements);

                // вычисляем вершины текущего кольца вдоль векторов от центроида к внешним вершинам
                // offset ожидается в 0..1 (0 центр, 1 внешний контур)
                float f = Mathf.Clamp01(ring.offset);

                Vector3 A = centroid + (A_out - centroid) * f;
                Vector3 B = centroid + (B_out - centroid) * f;
                Vector3 C = centroid + (C_out - centroid) * f;

                // применяем поворот кольца вокруг центроида, если задан
                if (Mathf.Abs(ring.angleOffset) > 0.0001f)
                {
                    Quaternion rot = Quaternion.Euler(0, ring.angleOffset, 0);
                    A = centroid + rot * (A - centroid);
                    B = centroid + rot * (B - centroid);
                    C = centroid + rot * (C - centroid);
                }

                // Сначала ставим вершины (A,B,C) если есть места
                int placed = 0;
                for (int v = 0; v < 3 && objIndex < count && placed < elems; v++)
                {
                    Vector3 pos = (v == 0) ? A : (v == 1) ? B : C;
                    Undo.RecordObject(objects[objIndex].transform, "Arrange Triangle Ring Vertex");
                    objects[objIndex].transform.position = pos;
                    objIndex++;
                    placed++;
                }

                // оставшиеся элементы для этого кольца — распределяем по сторонам A-B, B-C, C-A
                int remainingThisRing = elems - placed;
                if (remainingThisRing > 0)
                {
                    int perSide = remainingThisRing / 3;
                    int extra = remainingThisRing % 3;

                    void PlaceBetweenRing(Vector3 s, Vector3 e, int cnt)
                    {
                        for (int i = 0; i < cnt && objIndex < objects.Count; i++)
                        {
                            float t = (i + 1f) / (cnt + 1f);
                            Vector3 pos = Vector3.Lerp(s, e, t);
                            Undo.RecordObject(objects[objIndex].transform, "Arrange Triangle Ring Edge");
                            objects[objIndex].transform.position = pos;
                            objIndex++;
                        }
                    }

                    PlaceBetweenRing(A, B, perSide + (extra > 0 ? 1 : 0));
                    PlaceBetweenRing(B, C, perSide + (extra > 1 ? 1 : 0));
                    PlaceBetweenRing(C, A, perSide);
                }

                // следующий ring
            }

            // Если остались объекты (больше числа заданных элементов в кольцах), положим их на внешний контур в порядке A-B-C...
            if (objIndex < count)
            {
                // используем внешний контур (offset = 1)
                Vector3[] cornersOuter = new[] { A_out, B_out, C_out };
                int idxExtra = 0;
                while (objIndex < count)
                {
                    Vector3 corner = cornersOuter[idxExtra % 3];
                    Undo.RecordObject(objects[objIndex].transform, "Arrange Triangle Extra");
                    objects[objIndex].transform.position = corner;
                    objIndex++;
                    idxExtra++;
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