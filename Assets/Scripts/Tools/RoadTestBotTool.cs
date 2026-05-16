#if UNITY_EDITOR && ROAD_TEST_BOT_ENABLED
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public sealed class RoadTestBotTool : EditorWindow
    {
        private const string HelperName = "__RoadTestBotRoute";

        private RoadTestBotRoute _route;
        private SerializedObject _serializedRoute;
        private Vector2 _scrollPosition;

        [MenuItem("Tools/Road Test Bot")]
        public static void ShowWindow()
        {
            GetWindow<RoadTestBotTool>("Road Test Bot");
        }

        private void OnEnable()
        {
            EnsureRoute();
            SceneView.duringSceneGui += DrawSceneRouteGizmos;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DrawSceneRouteGizmos;
        }

        private void OnGUI()
        {
            EnsureRoute();

            if (_route == null)
            {
                EditorGUILayout.HelpBox("Road Test Bot helper could not be created.", MessageType.Error);
                return;
            }

            if (_serializedRoute == null || _serializedRoute.targetObject != _route)
                _serializedRoute = new SerializedObject(_route);

            _serializedRoute.Update();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Road Test Bot", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This window owns the hidden scene helper. Configure the route here, then spawn/start the bot in Play Mode.", MessageType.None);

            DrawToolState();
            DrawRouteSource();
            DrawBot();
            DrawRoute();
            DrawDriving();
            DrawGizmos();
            DrawCache();
            DrawRuntimeControls();

            EditorGUILayout.EndScrollView();

            if (_serializedRoute.ApplyModifiedProperties())
                SceneView.RepaintAll();
        }

        private void DrawToolState()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tool State", EditorStyles.boldLabel);

            var enabledProperty = _serializedRoute.FindProperty("_toolEnabled");
            var previousEnabled = enabledProperty.boolValue;
            EditorGUILayout.PropertyField(enabledProperty, new GUIContent("Tool Enabled"));

            if (previousEnabled != enabledProperty.boolValue)
            {
                _serializedRoute.ApplyModifiedProperties();
                _route.SetToolEnabled(enabledProperty.boolValue);
                EditorUtility.SetDirty(_route);
                SceneView.RepaintAll();
            }

            using (new EditorGUI.DisabledScope(!enabledProperty.boolValue))
            {
                if (GUILayout.Button("Disable Tool For Road Editing"))
                {
                    enabledProperty.boolValue = false;
                    _serializedRoute.ApplyModifiedProperties();
                    _route.SetToolEnabled(false);
                    EditorUtility.SetDirty(_route);
                    SceneView.RepaintAll();
                }
            }

            if (!_route.ToolEnabled)
                EditorGUILayout.HelpBox("Tool is disabled: gizmos, bot spawn/start and cache rebuild are blocked. Enable it after editing the road, then rebuild cache.", MessageType.Warning);
        }

        private void DrawRouteSource()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Route Source", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_routeSource"));

            var source = (RoadTestBotRoute.RouteSource)_serializedRoute.FindProperty("_routeSource").enumValueIndex;
            if (source == RoadTestBotRoute.RouteSource.UnitySpline)
            {
                EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_mapSplineProvider"));
                EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_splineContainer"));
            }
            else
            {
                EditorGUILayout.HelpBox("EasyRoads support was removed. Use UnitySpline route source.", MessageType.Warning);
            }

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(!_route.ToolEnabled))
            {
                if (GUILayout.Button("Auto Find Route Source"))
                {
                    _serializedRoute.ApplyModifiedProperties();
                    Undo.RecordObject(_route, "Auto Find Route Source");
                    _route.AutoFindRouteSource();
                    EditorUtility.SetDirty(_route);
                    SceneView.RepaintAll();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBot()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Bot", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_carPrefab"));
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_autoStart"));
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_loop"));
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_spawnHeightOffset"));
        }

        private void DrawRoute()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Route", EditorStyles.boldLabel);
            DrawRoutePercentRange();
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_lateralOffsetMeters"));
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_reverseRoute"));
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_lookAheadMeters"));
        }

        private void DrawRoutePercentRange()
        {
            var startProperty = _serializedRoute.FindProperty("_startPercent");
            var endProperty = _serializedRoute.FindProperty("_endPercent");

            var start = Mathf.Clamp(startProperty.floatValue, 0f, 100f);
            var end = Mathf.Clamp(endProperty.floatValue, 0f, 100f);
            if (end < start)
                end = start;

            EditorGUILayout.MinMaxSlider("Route Percent", ref start, ref end, 0f, 100f);

            EditorGUILayout.BeginHorizontal();
            start = EditorGUILayout.FloatField("Start", start);
            end = EditorGUILayout.FloatField("End", end);
            EditorGUILayout.EndHorizontal();

            start = Mathf.Clamp(start, 0f, 100f);
            end = Mathf.Clamp(end, 0f, 100f);
            if (end < start)
                end = start;

            startProperty.floatValue = start;
            endProperty.floatValue = end;
        }

        private void DrawDriving()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Driving", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_targetSpeedKmh"));
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_motorTorque"));
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_brakeTorque"));
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_brakeSpeedToleranceKmh"));
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_maxSteerAngle"));
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_steeringSpeed"));
        }

        private void DrawGizmos()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gizmos", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_drawGizmos"));
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_gizmoSegments"));
            EditorGUILayout.PropertyField(_serializedRoute.FindProperty("_pointRadius"));
        }

        private void DrawCache()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Route Cache", EditorStyles.boldLabel);

            var routeSource = (RoadTestBotRoute.RouteSource)_serializedRoute.FindProperty("_routeSource").enumValueIndex;
            using (new EditorGUI.DisabledScope(routeSource != RoadTestBotRoute.RouteSource.EasyRoads || !_route.ToolEnabled))
            {
                if (GUILayout.Button("Rebuild Route Cache"))
                {
                    _serializedRoute.ApplyModifiedProperties();
                    Undo.RecordObject(_route, "Rebuild Road Test Bot Route Cache");
                    _route.BuildRouteCache();
                    EditorUtility.SetDirty(_route);
                    SceneView.RepaintAll();
                }
            }

            var status = _route.EasyRoadCacheValid
                ? $"Cache ready: {_route.CachedPointCount} points, {_route.CachedTotalLength:0.0} m"
                : "Cache is empty. EasyRoads support was removed.";
            EditorGUILayout.HelpBox(status, _route.EasyRoadCacheValid ? MessageType.None : MessageType.Warning);
        }

        private void DrawRuntimeControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(!Application.isPlaying || !_route.ToolEnabled))
            {
                if (GUILayout.Button("Spawn Bot"))
                    _route.SpawnBot();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Start"))
                    _route.StartBot();

                if (GUILayout.Button("Restart"))
                    _route.RestartBot();

                if (GUILayout.Button("Stop"))
                    _route.StopBot();
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Destroy Bot"))
                    _route.StopAndDestroyBot();
            }

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Enter Play Mode to spawn and run the bot. Route setup and gizmos work in Edit Mode.", MessageType.Info);
        }

        private void EnsureRoute()
        {
            if (_route != null)
                return;

            _route = FindExistingRoute();
            if (_route != null)
            {
                NormalizeHelper(_route);
                return;
            }

            var go = new GameObject(HelperName);
            _route = go.AddComponent<RoadTestBotRoute>();
            NormalizeHelper(_route);
            _route.AutoFindRouteSource();
            EditorUtility.SetDirty(go);
        }

        private static void NormalizeHelper(RoadTestBotRoute route)
        {
            route.gameObject.name = HelperName;
            route.gameObject.hideFlags = HideFlags.HideInHierarchy;
            EditorUtility.SetDirty(route.gameObject);
        }

        private static RoadTestBotRoute FindExistingRoute()
        {
            var routes = Resources.FindObjectsOfTypeAll<RoadTestBotRoute>();
            for (var i = 0; i < routes.Length; i++)
            {
                var route = routes[i];
                if (route == null || route.gameObject == null)
                    continue;

                if (route.gameObject.scene.IsValid() && route.gameObject.name == HelperName)
                    return route;
            }

            for (var i = 0; i < routes.Length; i++)
            {
                var route = routes[i];
                if (route != null && route.gameObject != null && route.gameObject.scene.IsValid())
                    return route;
            }

            return null;
        }

        private void DrawSceneRouteGizmos(SceneView sceneView)
        {
            if (_route == null || !_route.ToolEnabled || !_route.DrawGizmosEnabled || !_route.HasRoute())
                return;

            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            DrawRouteLine(false, new Color(1f, 1f, 1f, 0.45f));
            DrawRouteLine(true, Color.cyan);

            var start = _route.ApplyLateralOffset(_route.GetWorldPoint(_route.DriveStartT), _route.DriveStartT);
            var end = _route.ApplyLateralOffset(_route.GetWorldPoint(_route.DriveEndT), _route.DriveEndT);

            Handles.color = Color.green;
            Handles.SphereHandleCap(0, start, Quaternion.identity, _route.PointRadius, EventType.Repaint);

            Handles.color = Color.red;
            Handles.SphereHandleCap(0, end, Quaternion.identity, _route.PointRadius, EventType.Repaint);
        }

        private void DrawRouteLine(bool withOffset, Color color)
        {
            Handles.color = color;

            var previousT = _route.DriveStartT;
            var previous = withOffset
                ? _route.ApplyLateralOffset(_route.GetWorldPoint(previousT), previousT)
                : _route.GetWorldPoint(previousT);

            for (var i = 1; i <= _route.GizmoSegments; i++)
            {
                var t = Mathf.Lerp(_route.DriveStartT, _route.DriveEndT, i / (float)_route.GizmoSegments);
                var current = withOffset
                    ? _route.ApplyLateralOffset(_route.GetWorldPoint(t), t)
                    : _route.GetWorldPoint(t);

                Handles.DrawLine(previous, current);
                previous = current;
            }
        }
    }

    [CustomEditor(typeof(RoadTestBotRoute))]
    public sealed class RoadTestBotRouteEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Use Tools > Road Test Bot. This helper is managed by the tool window.", MessageType.Info);

            if (GUILayout.Button("Open Road Test Bot Tool"))
                RoadTestBotTool.ShowWindow();

            DrawDefaultInspector();
        }
    }
}
#endif
