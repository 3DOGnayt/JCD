#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public class RoadBarrierColliderTool : EditorWindow
    {
        private const string MenuPath = "Tools/JCD/Road Barrier Collider Tool";
        private const string GeneratedColliderPrefix = "GeneratedBarrier_";
        private const string GeneratedContainerSuffix = "_GeneratedBarrierColliders";

        private GameObject _targetRoot;
        private MeshFilter _roadMeshFilter;

        private float _edgeHeight = 3f;
        private float _edgeThickness = 0.5f;
        private float _edgeBottomOffset = 0f;
        private float _edgeHorizontalOffset = 0f;
        private float _edgeWeldTolerance = 0.01f;
        private float _edgeMinLength = 0.25f;
        private float _edgeMaxVerticalDelta = 0.25f;
        private float _edgeSimplifyAngleTolerance = 7f;
        private bool _clearGeneratedBeforeBuild = true;
        private bool _mergeAfterBuild = false;

        private float _mergeAngleTolerance = 5f;
        private float _mergeLineOffsetTolerance = 0.35f;
        private float _mergeGapTolerance = 0.4f;
        private float _mergeWidthTolerance = 0.25f;

        private float _height = 3f;
        private float _bottomY = 0f;
        private bool _keepBottomY = true;

        private Vector2 _scroll;
        private string _lastMessage = string.Empty;
        private readonly List<string> _previewLines = new List<string>();

        [MenuItem(MenuPath)]
        public static void Open()
        {
            GetWindow<RoadBarrierColliderTool>("Barrier Colliders");
        }

        [MenuItem(MenuPath + " From Selection")]
        private static void OpenFromSelection()
        {
            var window = GetWindow<RoadBarrierColliderTool>("Barrier Colliders");
            window.UseSelection();
            window.Focus();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Road Barrier Collider Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Generate строит вертикальные BoxCollider по внешнему краю MeshFilter. Merge объединяет соседние прямые коллайдеры. Stretch Height отдельно меняет только высоту.",
                MessageType.Info);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Generate From Mesh Edge", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                _roadMeshFilter = (MeshFilter)EditorGUILayout.ObjectField("Road MeshFilter", _roadMeshFilter, typeof(MeshFilter), true);

                if (GUILayout.Button("Use Selection", GUILayout.Width(110)))
                {
                    UseMeshSelection();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _targetRoot = (GameObject)EditorGUILayout.ObjectField("Collider root", _targetRoot, typeof(GameObject), true);

                if (GUILayout.Button("Use Selection", GUILayout.Width(110)))
                {
                    UseSelection();
                }
            }

            _edgeHeight = Mathf.Max(0.1f, EditorGUILayout.FloatField("Height", _edgeHeight));
            _edgeThickness = Mathf.Max(0.01f, EditorGUILayout.FloatField("Thickness", _edgeThickness));
            _edgeBottomOffset = EditorGUILayout.FloatField("Bottom world offset", _edgeBottomOffset);
            _edgeHorizontalOffset = EditorGUILayout.FloatField("Horizontal offset", _edgeHorizontalOffset);
            _edgeWeldTolerance = Mathf.Max(0.0001f, EditorGUILayout.FloatField("Weld tolerance", _edgeWeldTolerance));
            _edgeMinLength = Mathf.Max(0.01f, EditorGUILayout.FloatField("Min segment length", _edgeMinLength));
            _edgeMaxVerticalDelta = Mathf.Max(0f, EditorGUILayout.FloatField("Max vertical delta", _edgeMaxVerticalDelta));
            _edgeSimplifyAngleTolerance = Mathf.Max(0f, EditorGUILayout.FloatField("Simplify angle tolerance", _edgeSimplifyAngleTolerance));
            _clearGeneratedBeforeBuild = EditorGUILayout.Toggle("Clear previous generated", _clearGeneratedBeforeBuild);
            _mergeAfterBuild = EditorGUILayout.Toggle("Merge after build", _mergeAfterBuild);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Preview Mesh Edges", GUILayout.Height(28)))
                {
                    PreviewMeshEdges();
                }

                if (GUILayout.Button("Generate Edge Colliders", GUILayout.Height(28)))
                {
                    GenerateEdgeColliders();
                }
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Merge", EditorStyles.boldLabel);
            _mergeAngleTolerance = Mathf.Max(0f, EditorGUILayout.FloatField("Angle tolerance", _mergeAngleTolerance));
            _mergeLineOffsetTolerance = Mathf.Max(0f, EditorGUILayout.FloatField("Line offset tolerance", _mergeLineOffsetTolerance));
            _mergeGapTolerance = Mathf.Max(0f, EditorGUILayout.FloatField("Gap tolerance", _mergeGapTolerance));
            _mergeWidthTolerance = Mathf.Max(0f, EditorGUILayout.FloatField("Width tolerance", _mergeWidthTolerance));

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Preview Merge", GUILayout.Height(28)))
                {
                    PreviewMerge();
                }

                if (GUILayout.Button("Merge Collinear Colliders", GUILayout.Height(28)))
                {
                    MergeCollinearColliders();
                }
            }

            if (GUILayout.Button("Force Merge Selected Colliders", GUILayout.Height(28)))
            {
                ForceMergeSelectedColliders();
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Stretch Height", EditorStyles.boldLabel);
            _height = Mathf.Max(0.1f, EditorGUILayout.FloatField("Height", _height));
            _keepBottomY = EditorGUILayout.Toggle("Keep current bottom", _keepBottomY);

            using (new EditorGUI.DisabledScope(_keepBottomY))
            {
                _bottomY = EditorGUILayout.FloatField("Bottom local Y", _bottomY);
            }

            if (GUILayout.Button("Stretch Height", GUILayout.Height(28)))
            {
                StretchHeight();
            }

            if (!string.IsNullOrWhiteSpace(_lastMessage))
            {
                EditorGUILayout.HelpBox(_lastMessage, MessageType.None);
            }

            DrawPreview();
        }

        private void DrawPreview()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField($"Preview ({_previewLines.Count})", EditorStyles.boldLabel);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var line in _previewLines)
            {
                EditorGUILayout.LabelField(line);
            }

            EditorGUILayout.EndScrollView();
        }

        private void UseSelection()
        {
            _targetRoot = Selection.activeGameObject;
            _lastMessage = _targetRoot == null
                ? "Выдели объект с BoxCollider в Hierarchy."
                : string.Empty;
        }

        private void UseMeshSelection()
        {
            _roadMeshFilter = Selection.activeGameObject == null
                ? null
                : Selection.activeGameObject.GetComponent<MeshFilter>();

            _lastMessage = _roadMeshFilter == null
                ? "Выдели объект с MeshFilter в Hierarchy."
                : string.Empty;
        }

        private void PreviewMeshEdges()
        {
            _previewLines.Clear();

            if (!TryBuildColliderSegments(out var segments, out var stats))
            {
                return;
            }

            _previewLines.Add($"Raw boundary edges: {stats.RawEdgeCount}");
            _previewLines.Add($"Chains: {stats.ChainCount}");
            _previewLines.Add($"Simplified points: {stats.SimplifiedPointCount}");
            _previewLines.Add($"Collider segments: {segments.Count}");
            _previewLines.Add($"Total length: {segments.Sum(segment => segment.Length):0.##}");
            _lastMessage = $"Найдено цепочек края: {stats.ChainCount}. Будет создано BoxCollider: {segments.Count}.";
        }

        private void GenerateEdgeColliders()
        {
            if (!TryBuildColliderSegments(out var segments, out var stats))
            {
                return;
            }

            var root = GetOrCreateColliderRoot();
            var container = GetOrCreateGeneratedContainer(root.transform);
            Undo.RegisterFullObjectHierarchyUndo(container, "Generate Road Edge Colliders");

            if (_clearGeneratedBeforeBuild)
            {
                ClearGeneratedColliders(container.transform);
            }

            for (var i = 0; i < segments.Count; i++)
            {
                CreateEdgeCollider(container.transform, segments[i], i);
            }

            _targetRoot = container;

            if (_mergeAfterBuild)
            {
                MergeCollinearColliders();
                _lastMessage = $"Готово. Контейнер: {container.name}. Цепочек: {stats.ChainCount}. Создано BoxCollider: {segments.Count}. {_lastMessage}";
            }
            else
            {
                _previewLines.Clear();
                _previewLines.Add($"Container: {container.name}");
                _previewLines.Add($"Chains: {stats.ChainCount}");
                _previewLines.Add($"Created colliders: {segments.Count}");
                _lastMessage = $"Готово. Контейнер: {container.name}. Создано BoxCollider: {segments.Count}.";
            }
        }

        private void PreviewMerge()
        {
            _previewLines.Clear();

            var groups = BuildMergeGroups(GetColliderInfos());
            foreach (var group in groups.Where(group => group.Count >= 2))
            {
                _previewLines.Add($"{group.Count} -> 1 | {group[0].Collider.name}");
            }

            _lastMessage = _previewLines.Count == 0
                ? "Группы для объединения не найдены."
                : $"Будет объединено групп: {_previewLines.Count}.";
        }

        private void MergeCollinearColliders()
        {
            var infos = GetColliderInfos();
            var groups = BuildMergeGroups(infos)
                .Where(group => group.Count >= 2)
                .ToList();

            if (groups.Count == 0)
            {
                _lastMessage = "Группы для объединения не найдены.";
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(_targetRoot, "Merge Barrier Colliders");

            var removedCount = 0;
            foreach (var group in groups)
            {
                MergeGroup(group);
                removedCount += group.Count - 1;
            }

            _lastMessage = $"Готово. Объединено групп: {groups.Count}, удалено коллайдеров: {removedCount}.";
            PreviewMerge();
        }

        private void ForceMergeSelectedColliders()
        {
            var selectedColliders = Selection.gameObjects
                .Select(gameObject => gameObject.GetComponent<BoxCollider>())
                .Where(collider => collider != null && collider.enabled)
                .Distinct()
                .ToList();

            if (selectedColliders.Count < 2)
            {
                _lastMessage = "Выдели минимум 2 GameObject с BoxCollider.";
                return;
            }

            var undoRoots = selectedColliders
                .Select(collider => collider.transform.root.gameObject)
                .Distinct()
                .Cast<Object>()
                .ToArray();
            Undo.RegisterFullObjectHierarchyUndo(undoRoots[0], "Force Merge Barrier Colliders");
            Undo.RecordObjects(undoRoots, "Force Merge Barrier Colliders");

            var group = selectedColliders
                .Select(CreateColliderInfo)
                .OrderBy(info => info.Min)
                .ToList();

            ForceMergeGroup(group);
            _lastMessage = $"Готово. Принудительно объединено BoxCollider: {selectedColliders.Count} -> 1.";
        }

        private void StretchHeight()
        {
            var colliders = GetColliders();
            if (colliders.Count == 0)
            {
                _lastMessage = "BoxCollider не найдены.";
                return;
            }

            Undo.RecordObjects(colliders.Cast<Object>().ToArray(), "Stretch Barrier Collider Height");

            foreach (var collider in colliders)
            {
                var size = collider.size;
                var center = collider.center;
                var bottom = _keepBottomY
                    ? center.y - size.y * 0.5f
                    : _bottomY;

                size.y = _height;
                center.y = bottom + _height * 0.5f;

                collider.size = size;
                collider.center = center;
                EditorUtility.SetDirty(collider);
            }

            _lastMessage = $"Готово. Высота изменена у BoxCollider: {colliders.Count}.";
        }

        private bool TryGetBoundaryEdges(out List<BoundaryEdge> edges)
        {
            edges = new List<BoundaryEdge>();

            if (_roadMeshFilter == null)
            {
                _lastMessage = "Road MeshFilter не выбран.";
                return false;
            }

            var mesh = _roadMeshFilter.sharedMesh;
            if (mesh == null)
            {
                _lastMessage = "У выбранного MeshFilter нет sharedMesh.";
                return false;
            }

            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var edgeMap = new Dictionary<EdgeKey, BoundaryEdgeAccumulator>();

            for (var i = 0; i < triangles.Length; i += 3)
            {
                AddMeshEdge(edgeMap, vertices, triangles[i], triangles[i + 1]);
                AddMeshEdge(edgeMap, vertices, triangles[i + 1], triangles[i + 2]);
                AddMeshEdge(edgeMap, vertices, triangles[i + 2], triangles[i]);
            }

            foreach (var pair in edgeMap)
            {
                if (pair.Value.Count != 1)
                {
                    continue;
                }

                var start = _roadMeshFilter.transform.TransformPoint(pair.Value.Start);
                var end = _roadMeshFilter.transform.TransformPoint(pair.Value.End);
                var delta = end - start;
                delta.y = 0f;

                if (delta.magnitude < _edgeMinLength || Mathf.Abs(start.y - end.y) > _edgeMaxVerticalDelta)
                {
                    continue;
                }

                edges.Add(new BoundaryEdge(start, end));
            }

            edges = edges
                .OrderBy(edge => edge.Start.x)
                .ThenBy(edge => edge.Start.z)
                .ToList();

            if (edges.Count > 0)
            {
                return true;
            }

            _lastMessage = "Внешние ребра не найдены. Попробуй увеличить Weld tolerance или Max vertical delta.";
            return false;
        }

        private bool TryBuildColliderSegments(out List<ColliderSegment> segments, out BuildStats stats)
        {
            segments = new List<ColliderSegment>();
            stats = default;

            if (!TryGetBoundaryEdges(out var edges))
            {
                return false;
            }

            var chains = BuildBoundaryChains(edges);
            foreach (var chain in chains)
            {
                var points = SimplifyChain(chain.Points, chain.Closed);
                if (points.Count < 2 || (chain.Closed && points.Count < 3))
                {
                    continue;
                }

                stats.SimplifiedPointCount += points.Count;
                segments.AddRange(BuildColliderSegments(points, chain.Closed));
            }

            stats.RawEdgeCount = edges.Count;
            stats.ChainCount = chains.Count;

            if (segments.Count > 0)
            {
                return true;
            }

            _lastMessage = "Не удалось построить collider-сегменты из края меша.";
            return false;
        }

        private List<BoundaryChain> BuildBoundaryChains(IReadOnlyList<BoundaryEdge> edges)
        {
            var pointLookup = new Dictionary<PointKey, Vector3>();
            var adjacency = new Dictionary<PointKey, List<int>>();

            for (var i = 0; i < edges.Count; i++)
            {
                var startKey = PointKey.Create(edges[i].Start, _edgeWeldTolerance);
                var endKey = PointKey.Create(edges[i].End, _edgeWeldTolerance);

                RegisterPoint(pointLookup, startKey, edges[i].Start);
                RegisterPoint(pointLookup, endKey, edges[i].End);
                RegisterEdge(adjacency, startKey, i);
                RegisterEdge(adjacency, endKey, i);
            }

            var used = new bool[edges.Count];
            var chains = new List<BoundaryChain>();

            foreach (var pair in adjacency.Where(pair => pair.Value.Count != 2))
            {
                var chain = WalkBoundaryChain(pair.Key, edges, adjacency, pointLookup, used);
                if (chain.Points.Count >= 2)
                {
                    chains.Add(chain);
                }
            }

            for (var i = 0; i < edges.Count; i++)
            {
                if (used[i])
                {
                    continue;
                }

                var startKey = PointKey.Create(edges[i].Start, _edgeWeldTolerance);
                var chain = WalkBoundaryChain(startKey, edges, adjacency, pointLookup, used);
                if (chain.Points.Count >= 2)
                {
                    chains.Add(chain);
                }
            }

            return chains;
        }

        private BoundaryChain WalkBoundaryChain(
            PointKey startKey,
            IReadOnlyList<BoundaryEdge> edges,
            IReadOnlyDictionary<PointKey, List<int>> adjacency,
            IReadOnlyDictionary<PointKey, Vector3> pointLookup,
            bool[] used)
        {
            var points = new List<Vector3> { pointLookup[startKey] };
            var currentKey = startKey;
            var closed = false;

            while (adjacency.TryGetValue(currentKey, out var connectedEdges))
            {
                var nextEdgeIndex = GetNextUnusedEdgeIndex(connectedEdges, used);
                if (nextEdgeIndex < 0)
                {
                    break;
                }

                used[nextEdgeIndex] = true;
                var edge = edges[nextEdgeIndex];
                var edgeStartKey = PointKey.Create(edge.Start, _edgeWeldTolerance);
                var edgeEndKey = PointKey.Create(edge.End, _edgeWeldTolerance);
                var nextKey = edgeStartKey.Equals(currentKey)
                    ? edgeEndKey
                    : edgeStartKey;

                if (nextKey.Equals(startKey))
                {
                    closed = true;
                    break;
                }

                points.Add(pointLookup[nextKey]);
                currentKey = nextKey;
            }

            return new BoundaryChain(points, closed);
        }

        private static int GetNextUnusedEdgeIndex(IEnumerable<int> connectedEdges, bool[] used)
        {
            foreach (var edgeIndex in connectedEdges)
            {
                if (!used[edgeIndex])
                {
                    return edgeIndex;
                }
            }

            return -1;
        }

        private static void RegisterPoint(Dictionary<PointKey, Vector3> pointLookup, PointKey key, Vector3 point)
        {
            if (!pointLookup.ContainsKey(key))
            {
                pointLookup.Add(key, point);
            }
        }

        private static void RegisterEdge(Dictionary<PointKey, List<int>> adjacency, PointKey key, int edgeIndex)
        {
            if (!adjacency.TryGetValue(key, out var edges))
            {
                edges = new List<int>();
                adjacency.Add(key, edges);
            }

            edges.Add(edgeIndex);
        }

        private List<Vector3> SimplifyChain(IReadOnlyList<Vector3> sourcePoints, bool closed)
        {
            var points = RemoveShortSegments(sourcePoints, closed);
            var changed = true;

            while (changed && points.Count > 2)
            {
                changed = false;
                var start = closed ? 0 : 1;
                var end = closed ? points.Count : points.Count - 1;

                for (var i = start; i < end; i++)
                {
                    var previous = points[WrapIndex(i - 1, points.Count)];
                    var current = points[i];
                    var next = points[WrapIndex(i + 1, points.Count)];
                    var a = ProjectToXZ(current - previous);
                    var b = ProjectToXZ(next - current);

                    if (Vector3.Angle(a, b) > _edgeSimplifyAngleTolerance)
                    {
                        continue;
                    }

                    points.RemoveAt(i);
                    changed = true;
                    break;
                }
            }

            return points;
        }

        private List<Vector3> RemoveShortSegments(IReadOnlyList<Vector3> sourcePoints, bool closed)
        {
            var points = new List<Vector3>();
            foreach (var point in sourcePoints)
            {
                if (points.Count == 0 || HorizontalDistance(points[points.Count - 1], point) >= _edgeMinLength)
                {
                    points.Add(point);
                }
            }

            if (closed && points.Count > 2 && HorizontalDistance(points[0], points[points.Count - 1]) < _edgeMinLength)
            {
                points.RemoveAt(points.Count - 1);
            }

            return points;
        }

        private List<ColliderSegment> BuildColliderSegments(IReadOnlyList<Vector3> points, bool closed)
        {
            var segments = new List<ColliderSegment>();
            if (points.Count < 2 || (closed && points.Count < 3))
            {
                return segments;
            }

            var segmentCount = closed ? points.Count : points.Count - 1;

            for (var i = 0; i < segmentCount; i++)
            {
                var start = points[i];
                var end = points[(i + 1) % points.Count];

                if (HorizontalDistance(start, end) < _edgeMinLength)
                {
                    continue;
                }

                var previous = i > 0
                    ? points[i - 1]
                    : closed
                        ? points[points.Count - 1]
                        : start;
                start = AlignStartCorner(start, previous, end, i > 0 || closed);
                if (HorizontalDistance(start, end) < _edgeMinLength)
                {
                    continue;
                }

                segments.Add(new ColliderSegment(start, end));
            }

            return segments;
        }

        private Vector3 AlignStartCorner(Vector3 corner, Vector3 previous, Vector3 next, bool hasPrevious)
        {
            if (!hasPrevious)
            {
                return corner;
            }

            var previousDirection = ProjectToXZ(corner - previous);
            var currentDirection = ProjectToXZ(next - corner);
            return AlignCorner(corner, previousDirection, currentDirection);
        }

        private Vector3 AlignCorner(Vector3 corner, Vector3 fixedDirection, Vector3 movedDirection)
        {
            var angle = Vector3.Angle(fixedDirection, movedDirection);
            if (angle <= _edgeSimplifyAngleTolerance || Mathf.Abs(180f - angle) <= _edgeSimplifyAngleTolerance)
            {
                return corner;
            }

            var fixedRight = Vector3.Cross(Vector3.up, fixedDirection).normalized;
            var movedRight = Vector3.Cross(Vector3.up, movedDirection).normalized;
            var side = GetSharedCornerSide(fixedDirection, movedDirection);
            var halfThickness = _edgeThickness * 0.5f;
            var aligned = corner + side * (fixedRight - movedRight) * halfThickness;
            aligned.y = corner.y;
            return aligned;
        }

        private static float GetSharedCornerSide(Vector3 fixedDirection, Vector3 movedDirection)
        {
            var turn = Vector3.Cross(fixedDirection, movedDirection).y;
            return turn < 0f ? 1f : -1f;
        }

        private static int WrapIndex(int index, int count)
        {
            if (index < 0)
            {
                return count - 1;
            }

            return index >= count
                ? 0
                : index;
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private void AddMeshEdge(Dictionary<EdgeKey, BoundaryEdgeAccumulator> edgeMap, Vector3[] vertices, int startIndex, int endIndex)
        {
            var start = vertices[startIndex];
            var end = vertices[endIndex];
            var key = EdgeKey.Create(start, end, _edgeWeldTolerance);

            if (edgeMap.TryGetValue(key, out var accumulator))
            {
                accumulator.Count++;
                edgeMap[key] = accumulator;
                return;
            }

            edgeMap.Add(key, new BoundaryEdgeAccumulator(start, end));
        }

        private GameObject GetOrCreateColliderRoot()
        {
            if (_targetRoot != null)
            {
                return _targetRoot;
            }

            var root = new GameObject($"{_roadMeshFilter.name}_BarrierColliders");
            Undo.RegisterCreatedObjectUndo(root, "Create Road Barrier Collider Root");
            root.transform.SetParent(_roadMeshFilter.transform.parent, false);
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
            return root;
        }

        private GameObject GetOrCreateGeneratedContainer(Transform root)
        {
            var containerName = $"{_roadMeshFilter.name}{GeneratedContainerSuffix}";

            if (root.name.EndsWith(GeneratedContainerSuffix))
            {
                return root.gameObject;
            }

            var existing = root.Find(containerName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            var container = new GameObject(containerName);
            Undo.RegisterCreatedObjectUndo(container, "Create Road Barrier Collider Container");
            container.transform.SetParent(root, false);
            container.transform.localPosition = Vector3.zero;
            container.transform.localRotation = Quaternion.identity;
            container.transform.localScale = Vector3.one;
            return container;
        }

        private static void ClearGeneratedColliders(Transform root)
        {
            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                if (child.name.StartsWith(GeneratedColliderPrefix))
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
        }

        private void CreateEdgeCollider(Transform root, ColliderSegment segment, int index)
        {
            var forward = ProjectToXZ(segment.End - segment.Start);
            var right = Vector3.Cross(Vector3.up, forward).normalized;
            var center = (segment.Start + segment.End) * 0.5f + right * _edgeHorizontalOffset;
            center.y = (segment.Start.y + segment.End.y) * 0.5f + _edgeBottomOffset + _edgeHeight * 0.5f;

            var gameObject = new GameObject($"{GeneratedColliderPrefix}{index:0000}");
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Road Edge Collider");
            gameObject.transform.SetParent(root, true);
            gameObject.transform.position = center;
            gameObject.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            gameObject.transform.localScale = Vector3.one;

            var collider = Undo.AddComponent<BoxCollider>(gameObject);
            collider.size = new Vector3(_edgeThickness, _edgeHeight, segment.Length);
            collider.center = Vector3.zero;
            EditorUtility.SetDirty(collider);
        }

        private List<BoxCollider> GetColliders()
        {
            if (_targetRoot == null)
            {
                return new List<BoxCollider>();
            }

            return _targetRoot
                .GetComponentsInChildren<BoxCollider>(true)
                .Where(collider => collider.enabled)
                .ToList();
        }

        private List<ColliderInfo> GetColliderInfos()
        {
            return GetColliders()
                .Select(CreateColliderInfo)
                .Where(info => info.Length > 0.01f)
                .ToList();
        }

        private static ColliderInfo CreateColliderInfo(BoxCollider collider)
        {
            var transform = collider.transform;
            var center = transform.TransformPoint(collider.center);
            var lossyScale = transform.lossyScale;
            var scaledSize = new Vector3(
                Mathf.Abs(collider.size.x * lossyScale.x),
                Mathf.Abs(collider.size.y * lossyScale.y),
                Mathf.Abs(collider.size.z * lossyScale.z));

            var forward = ProjectToXZ(transform.TransformDirection(Vector3.forward));
            var right = ProjectToXZ(transform.TransformDirection(Vector3.right));

            if (scaledSize.x > scaledSize.z)
            {
                forward = right;
                right = ProjectToXZ(transform.TransformDirection(Vector3.forward));
                scaledSize = new Vector3(scaledSize.z, scaledSize.y, scaledSize.x);
            }

            if (forward.z < 0f || (Mathf.Approximately(forward.z, 0f) && forward.x < 0f))
            {
                forward = -forward;
                right = -right;
            }

            var halfLength = scaledSize.z * 0.5f;
            var projection = Vector3.Dot(center, forward);
            var offset = Vector3.Dot(center, right);

            return new ColliderInfo(
                collider,
                center,
                forward,
                right,
                projection - halfLength,
                projection + halfLength,
                offset,
                scaledSize.x,
                scaledSize.y,
                scaledSize.z);
        }

        private List<List<ColliderInfo>> BuildMergeGroups(List<ColliderInfo> infos)
        {
            var groups = new List<List<ColliderInfo>>();
            var used = new HashSet<BoxCollider>();

            foreach (var seed in infos.OrderBy(info => info.Min))
            {
                if (used.Contains(seed.Collider))
                {
                    continue;
                }

                var candidates = infos
                    .Where(info => !used.Contains(info.Collider))
                    .Where(info => CanBeInSameLine(seed, info))
                    .OrderBy(info => info.Min)
                    .ToList();

                var currentGroup = new List<ColliderInfo>();
                var hasPrevious = false;
                var previous = default(ColliderInfo);

                foreach (var candidate in candidates)
                {
                    if (hasPrevious && candidate.Min - previous.Max > _mergeGapTolerance)
                    {
                        AddGroup(currentGroup, groups, used);
                        currentGroup = new List<ColliderInfo>();
                    }

                    currentGroup.Add(candidate);
                    previous = candidate;
                    hasPrevious = true;
                }

                AddGroup(currentGroup, groups, used);
            }

            return groups;
        }

        private bool CanBeInSameLine(ColliderInfo a, ColliderInfo b)
        {
            var angle = Vector3.Angle(a.Forward, b.Forward);
            var sameDirection = angle <= _mergeAngleTolerance || Mathf.Abs(180f - angle) <= _mergeAngleTolerance;
            if (!sameDirection)
            {
                return false;
            }

            return Mathf.Abs(a.Offset - b.Offset) <= _mergeLineOffsetTolerance
                && Mathf.Abs(a.Width - b.Width) <= _mergeWidthTolerance;
        }

        private static void AddGroup(List<ColliderInfo> group, List<List<ColliderInfo>> groups, HashSet<BoxCollider> used)
        {
            if (group.Count == 0)
            {
                return;
            }

            groups.Add(group);

            foreach (var info in group)
            {
                used.Add(info.Collider);
            }
        }

        private static void MergeGroup(List<ColliderInfo> group)
        {
            var first = group[0];
            var min = group.Min(info => info.Min);
            var max = group.Max(info => info.Max);
            var width = group.Max(info => info.Width);
            var height = group.Max(info => info.Height);
            var length = max - min;
            var offset = group.Average(info => info.Offset);
            var y = group.Average(info => info.Center.y);
            var worldCenter = first.Forward * ((min + max) * 0.5f) + first.Right * offset;
            worldCenter.y = y;

            var targetTransform = first.Collider.transform;
            Undo.RecordObject(targetTransform, "Merge Barrier Colliders");
            Undo.RecordObject(first.Collider, "Merge Barrier Colliders");
            targetTransform.position = worldCenter;
            targetTransform.rotation = Quaternion.LookRotation(first.Forward, Vector3.up);
            targetTransform.localScale = Vector3.one;

            first.Collider.center = Vector3.zero;
            first.Collider.size = new Vector3(width, height, length);
            EditorUtility.SetDirty(first.Collider);

            for (var i = 1; i < group.Count; i++)
            {
                DestroyMergedCollider(first.Collider, group[i].Collider);
            }
        }

        private static void ForceMergeGroup(List<ColliderInfo> group)
        {
            var first = group[0];
            var forward = CalculateForceMergeForward(group);
            var right = Vector3.Cross(Vector3.up, forward).normalized;
            var min = float.MaxValue;
            var max = float.MinValue;
            var minOffset = float.MaxValue;
            var maxOffset = float.MinValue;
            var ySum = 0f;
            var height = 0f;

            foreach (var info in group)
            {
                foreach (var corner in GetHorizontalWorldCorners(info.Collider))
                {
                    var lengthProjection = Vector3.Dot(corner, forward);
                    var widthProjection = Vector3.Dot(corner, right);

                    min = Mathf.Min(min, lengthProjection);
                    max = Mathf.Max(max, lengthProjection);
                    minOffset = Mathf.Min(minOffset, widthProjection);
                    maxOffset = Mathf.Max(maxOffset, widthProjection);
                }

                ySum += info.Center.y;
                height = Mathf.Max(height, info.Height);
            }

            var offset = (minOffset + maxOffset) * 0.5f;
            var worldCenter = forward * ((min + max) * 0.5f) + right * offset;
            worldCenter.y = ySum / group.Count;
            var size = new Vector3(maxOffset - minOffset, height, max - min);

            var targetTransform = first.Collider.transform;
            Undo.RecordObject(targetTransform, "Force Merge Barrier Colliders");
            Undo.RecordObject(first.Collider, "Force Merge Barrier Colliders");
            targetTransform.position = worldCenter;
            targetTransform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            targetTransform.localScale = Vector3.one;

            first.Collider.center = Vector3.zero;
            first.Collider.size = size;
            EditorUtility.SetDirty(first.Collider);

            for (var i = 1; i < group.Count; i++)
            {
                DestroyMergedCollider(first.Collider, group[i].Collider);
            }
        }

        private static Vector3[] GetHorizontalWorldCorners(BoxCollider collider)
        {
            var center = collider.center;
            var halfSize = collider.size * 0.5f;
            var transform = collider.transform;

            return new[]
            {
                transform.TransformPoint(center + new Vector3(-halfSize.x, 0f, -halfSize.z)),
                transform.TransformPoint(center + new Vector3(-halfSize.x, 0f, halfSize.z)),
                transform.TransformPoint(center + new Vector3(halfSize.x, 0f, -halfSize.z)),
                transform.TransformPoint(center + new Vector3(halfSize.x, 0f, halfSize.z))
            };
        }

        private static Vector3 CalculateForceMergeForward(IReadOnlyList<ColliderInfo> group)
        {
            var bestDirection = Vector3.zero;
            var bestDistance = 0f;

            for (var i = 0; i < group.Count; i++)
            {
                for (var j = i + 1; j < group.Count; j++)
                {
                    var direction = group[j].Center - group[i].Center;
                    direction.y = 0f;
                    var distance = direction.sqrMagnitude;
                    if (distance <= bestDistance)
                    {
                        continue;
                    }

                    bestDistance = distance;
                    bestDirection = direction;
                }
            }

            if (bestDistance > 0.0001f)
            {
                return NormalizeForward(bestDirection);
            }

            return CalculateAverageRotationForward(group);
        }

        private static Vector3 CalculateAverageRotationForward(IReadOnlyList<ColliderInfo> group)
        {
            var forward = Vector3.zero;
            var reference = group[0].Forward;

            foreach (var info in group)
            {
                var direction = Vector3.Dot(reference, info.Forward) < 0f
                    ? -info.Forward
                    : info.Forward;
                forward += direction;
            }

            return NormalizeForward(forward);
        }

        private static Vector3 NormalizeForward(Vector3 value)
        {
            var forward = ProjectToXZ(value);
            return forward.z < 0f || (Mathf.Approximately(forward.z, 0f) && forward.x < 0f)
                ? -forward
                : forward;
        }

        private static void DestroyMergedCollider(BoxCollider firstCollider, BoxCollider mergedCollider)
        {
            var mergedObject = mergedCollider.gameObject;
            var canDeleteObject = mergedObject != firstCollider.gameObject
                && mergedObject.transform.childCount == 0
                && mergedObject.GetComponents<Component>().Length == 2;

            if (canDeleteObject)
            {
                Undo.DestroyObjectImmediate(mergedObject);
                return;
            }

            Undo.DestroyObjectImmediate(mergedCollider);
        }

        private static Vector3 ProjectToXZ(Vector3 value)
        {
            value.y = 0f;
            return value.sqrMagnitude < 0.0001f
                ? Vector3.forward
                : value.normalized;
        }

        private readonly struct ColliderInfo
        {
            public readonly BoxCollider Collider;
            public readonly Vector3 Center;
            public readonly Vector3 Forward;
            public readonly Vector3 Right;
            public readonly float Min;
            public readonly float Max;
            public readonly float Offset;
            public readonly float Width;
            public readonly float Height;
            public readonly float Length;

            public ColliderInfo(
                BoxCollider collider,
                Vector3 center,
                Vector3 forward,
                Vector3 right,
                float min,
                float max,
                float offset,
                float width,
                float height,
                float length)
            {
                Collider = collider;
                Center = center;
                Forward = forward;
                Right = right;
                Min = min;
                Max = max;
                Offset = offset;
                Width = width;
                Height = height;
                Length = length;
            }
        }

        private readonly struct BoundaryEdge
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly float Length;

            public BoundaryEdge(Vector3 start, Vector3 end)
            {
                Start = start;
                End = end;

                var delta = end - start;
                delta.y = 0f;
                Length = delta.magnitude;
            }
        }

        private readonly struct ColliderSegment
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly float Length;

            public ColliderSegment(Vector3 start, Vector3 end)
            {
                Start = start;
                End = end;

                var delta = end - start;
                delta.y = 0f;
                Length = delta.magnitude;
            }
        }

        private readonly struct BoundaryChain
        {
            public readonly List<Vector3> Points;
            public readonly bool Closed;

            public BoundaryChain(List<Vector3> points, bool closed)
            {
                Points = points;
                Closed = closed;
            }
        }

        private struct BuildStats
        {
            public int RawEdgeCount;
            public int ChainCount;
            public int SimplifiedPointCount;
        }

        private struct BoundaryEdgeAccumulator
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public int Count;

            public BoundaryEdgeAccumulator(Vector3 start, Vector3 end)
            {
                Start = start;
                End = end;
                Count = 1;
            }
        }

        private readonly struct PointKey
        {
            private readonly Vector3Int _value;

            private PointKey(Vector3Int value)
            {
                _value = value;
            }

            public static PointKey Create(Vector3 point, float tolerance)
            {
                return new PointKey(new Vector3Int(
                    Mathf.RoundToInt(point.x / tolerance),
                    Mathf.RoundToInt(point.y / tolerance),
                    Mathf.RoundToInt(point.z / tolerance)));
            }

            public override int GetHashCode()
            {
                return _value.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return obj is PointKey other && _value == other._value;
            }
        }

        private readonly struct EdgeKey
        {
            private readonly Vector3Int _a;
            private readonly Vector3Int _b;

            private EdgeKey(Vector3Int a, Vector3Int b)
            {
                _a = a;
                _b = b;
            }

            public static EdgeKey Create(Vector3 start, Vector3 end, float tolerance)
            {
                var a = Quantize(start, tolerance);
                var b = Quantize(end, tolerance);

                return Compare(a, b) <= 0
                    ? new EdgeKey(a, b)
                    : new EdgeKey(b, a);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_a.GetHashCode() * 397) ^ _b.GetHashCode();
                }
            }

            public override bool Equals(object obj)
            {
                return obj is EdgeKey other && _a == other._a && _b == other._b;
            }

            private static Vector3Int Quantize(Vector3 value, float tolerance)
            {
                return new Vector3Int(
                    Mathf.RoundToInt(value.x / tolerance),
                    Mathf.RoundToInt(value.y / tolerance),
                    Mathf.RoundToInt(value.z / tolerance));
            }

            private static int Compare(Vector3Int a, Vector3Int b)
            {
                if (a.x != b.x)
                {
                    return a.x.CompareTo(b.x);
                }

                if (a.y != b.y)
                {
                    return a.y.CompareTo(b.y);
                }

                return a.z.CompareTo(b.z);
            }
        }
    }
}
#endif
