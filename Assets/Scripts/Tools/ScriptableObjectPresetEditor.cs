using Configs.Impl;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [CustomEditor(typeof(CarPreset))]
    public class ScriptableObjectPresetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var preset = (CarPreset)target;

            if (preset.Car == null)
            {
                EditorGUILayout.HelpBox("Set gameObject from prefab to Car!", MessageType.Warning);
                return;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Setup parameters FROM Car TO SO"))
                SaveFromCar(preset);

            if (GUILayout.Button("Setup parameters FROM SO TO Car")) 
                ApplyToCar(preset);
        }

        private void SaveFromCar(CarPreset preset)
        {
            var car = preset.Car;
            var rb = car.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                preset.CarParameters.Mass = rb.mass;
                preset.CarParameters.AutomaticCenterOfMass = rb.automaticCenterOfMass;
                preset.CarParameters.CenterOfMass = rb.centerOfMass;
            }

            var wheels = car.GetComponentsInChildren<WheelCollider>();
            if (wheels.Length >= 4)
            {
                preset.FrontWheelParameters.SaveFromWheel(wheels[0]);
                preset.FrontWheelParameters.SaveFromWheel(wheels[1]);

                preset.BackWheelParameters.SaveFromWheel(wheels[wheels.Length - 2]);
                preset.BackWheelParameters.SaveFromWheel(wheels[wheels.Length - 1]);
            }

            EditorUtility.SetDirty(preset);
            Debug.Log("✅ Parameters saved in ScriptableObject");
        }

        private void ApplyToCar(CarPreset preset)
        {
            var car = preset.Car;
            var rb = car.GetComponentInChildren<Rigidbody>();
            if (rb != null)
                preset.CarParameters.SetCarParameters(rb);

            var wheels = car.GetComponentsInChildren<WheelCollider>();
            if (wheels.Length >= 4)
            {
                preset.FrontWheelParameters.SetAllParameters(wheels[0]);
                preset.FrontWheelParameters.SetAllParameters(wheels[1]);

                preset.BackWheelParameters.SetAllParameters(wheels[wheels.Length - 2]);
                preset.BackWheelParameters.SetAllParameters(wheels[wheels.Length - 1]);
            }

            Debug.Log("✅ Parameters from ScriptableObject applied to car");
        }
    }
}