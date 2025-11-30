using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WheelStiffnessDebugGizmo : MonoBehaviour
{
    [Header("Wheels")]
    public WheelCollider[] wheels;

    [Header("Draw settings")]
    public float heightOffset   = 0.4f;
    public float lineSpacing    = 0.18f; // расстояние между строками
    public float equalThreshold = 0.01f; // допуск, когда считаем "как сток"

#if UNITY_EDITOR
    private bool _initialized;
    private float[] _baseForwardStiffness;
    private float[] _baseSidewaysStiffness;

    // Можно включить, чтобы заново захватить "стоковые" значения
    public bool recaptureBaseValues;

    private void Init()
    {
        if (wheels == null || wheels.Length == 0)
            return;

        _baseForwardStiffness  = new float[wheels.Length];
        _baseSidewaysStiffness = new float[wheels.Length];

        for (int i = 0; i < wheels.Length; i++)
        {
            var w = wheels[i];
            if (w == null)
                continue;

            var f = w.forwardFriction;
            var s = w.sidewaysFriction;

            _baseForwardStiffness[i]  = f.stiffness;
            _baseSidewaysStiffness[i] = s.stiffness;
        }

        _initialized = true;
    }

    private void OnDrawGizmos()
    {
        if (wheels == null || wheels.Length == 0)
            return;

        if (!_initialized || recaptureBaseValues)
        {
            recaptureBaseValues = false;
            Init();
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            var w = wheels[i];
            if (w == null)
                continue;

            Vector3 basePos = w.transform.position + Vector3.up * heightOffset;

            var f = w.forwardFriction;
            var s = w.sidewaysFriction;

            float baseF = _baseForwardStiffness != null && i < _baseForwardStiffness.Length
                ? _baseForwardStiffness[i]
                : f.stiffness;

            float baseS = _baseSidewaysStiffness != null && i < _baseSidewaysStiffness.Length
                ? _baseSidewaysStiffness[i]
                : s.stiffness;

            // ---- цвет для forward отдельно ----
            Color colorF = Color.white;
            if (f.stiffness > baseF + equalThreshold)
                colorF = Color.green;
            else if (f.stiffness < baseF - equalThreshold)
                colorF = Color.red;

            // ---- цвет для sideways отдельно ----
            Color colorS = Color.white;
            if (s.stiffness > baseS + equalThreshold)
                colorS = Color.green;
            else if (s.stiffness < baseS - equalThreshold)
                colorS = Color.red;

            // ---- рисуем две строки ----
            GUIStyle styleF = new GUIStyle(EditorStyles.boldLabel);
            styleF.normal.textColor = colorF;
            string labelF = $"F: {f.stiffness:F2}";

            GUIStyle styleS = new GUIStyle(EditorStyles.boldLabel);
            styleS.normal.textColor = colorS;
            string labelS = $"S: {s.stiffness:F2}";

            // первая строка (forward)
            Handles.Label(basePos, labelF, styleF);

            // вторая строка (sideways) ниже/выше первой
            Vector3 posS = basePos - Vector3.up * lineSpacing;
            Handles.Label(posS, labelS, styleS);
        }
    }
#endif
}