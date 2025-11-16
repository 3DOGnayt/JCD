using Configs.Impl;
using Core;
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
                EditorGUILayout.HelpBox("Set GameObject (car prefab) into Car field!", MessageType.Warning);
                return;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Save parameters FROM Car → TO SO"))
                SaveFromCar(preset);

            if (GUILayout.Button("Apply parameters FROM SO → TO Car")) 
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

            var carView = car.GetComponent<CarView>();
            if (carView != null && carView.CarSetup != null)
            {
                var setup = carView.CarSetup;
                preset.CarSetup.CurrentSpeed = setup.CurrentSpeed;
                preset.CarSetup.Gearbox = setup.Gearbox;
                preset.CarSetup.CurrentBackSpeed = setup.CurrentBackSpeed;
                preset.CarSetup.AccelerationMultiplier = setup.AccelerationMultiplier;
                preset.CarSetup.DecelerationMultiplier = setup.DecelerationMultiplier;
                preset.CarSetup.CurrentSteeringAngle = setup.CurrentSteeringAngle;
                preset.CarSetup.SteeringSpeed = setup.SteeringSpeed;
                preset.CarSetup.BrakeForce = setup.BrakeForce;
                preset.CarSetup.DriftMultiplier = setup.DriftMultiplier;
                preset.CarSetup.CurrentEngineRpm = setup.CurrentEngineRpm;
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
            EditorUtility.SetDirty(car);
            PrefabUtility.RecordPrefabInstancePropertyModifications(car);

            Debug.Log($"✅ {car.name}: данные сохранены в CarPreset");
        }

        private void ApplyToCar(CarPreset preset)
        {
            var car = preset.Car;
            var rb = car.GetComponentInChildren<Rigidbody>();
            if (rb != null)
                preset.CarParameters.SetCarParameters(rb);

            var carView = car.GetComponent<CarView>();
            if (carView != null && carView.CarSetup != null)
            {
                var setup = carView.CarSetup;
                setup.CurrentSpeed = preset.CarSetup.CurrentSpeed;
                setup.Gearbox = preset.CarSetup.Gearbox;
                setup.CurrentBackSpeed = preset.CarSetup.CurrentBackSpeed;
                setup.AccelerationMultiplier = preset.CarSetup.AccelerationMultiplier;
                setup.DecelerationMultiplier = preset.CarSetup.DecelerationMultiplier;
                setup.CurrentSteeringAngle = preset.CarSetup.CurrentSteeringAngle;
                setup.SteeringSpeed = preset.CarSetup.SteeringSpeed;
                setup.BrakeForce = preset.CarSetup.BrakeForce;
                setup.DriftMultiplier = preset.CarSetup.DriftMultiplier;
                setup.CurrentEngineRpm = preset.CarSetup.CurrentEngineRpm;
            }

            var wheels = car.GetComponentsInChildren<WheelCollider>();
            if (wheels.Length >= 4)
            {
                preset.FrontWheelParameters.SetAllParameters(wheels[0]);
                preset.FrontWheelParameters.SetAllParameters(wheels[1]);
                preset.BackWheelParameters.SetAllParameters(wheels[wheels.Length - 2]);
                preset.BackWheelParameters.SetAllParameters(wheels[wheels.Length - 1]);
            }

            EditorUtility.SetDirty(car);
            PrefabUtility.RecordPrefabInstancePropertyModifications(car);

            Debug.Log($"✅ {car.name}: данные из CarPreset применены на Car");
        }
    }
}