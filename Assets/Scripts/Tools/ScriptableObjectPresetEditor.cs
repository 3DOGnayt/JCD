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
                preset.CarMassParameters.Mass = rb.mass;
                preset.CarMassParameters.AutomaticCenterOfMass = rb.automaticCenterOfMass;
                preset.CarMassParameters.CenterOfMass = rb.centerOfMass;
            }

            var carView = car.GetComponent<CarView>();
            if (carView != null && carView.CarSetup != null)
            {
                var setup = carView.CarSetup;
                preset.CarSetup.SpeedMax = setup.SpeedMax;
                preset.CarSetup.GearCount = setup.GearCount;
                preset.CarSetup.BackSpeedMax = setup.BackSpeedMax;
                preset.CarSetup.SteeringAngleMax = setup.SteeringAngleMax;
                preset.CarSetup.SteeringSpeed = setup.SteeringSpeed;
                preset.CarSetup.BrakeInput = setup.BrakeInput;
                preset.CarSetup.HandbrakeInput = setup.HandbrakeInput;
                preset.CarSetup.DriftMultiplier = setup.DriftMultiplier;
                preset.CarSetup.EngineRpmMax = setup.EngineRpmMax;
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
                preset.CarMassParameters.SetCarParameters(rb);

            var carView = car.GetComponent<CarView>();
            if (carView != null && carView.CarSetup != null)
            {
                var setup = carView.CarSetup;
                setup.SpeedMax = preset.CarSetup.SpeedMax;
                setup.GearCount = preset.CarSetup.GearCount;
                setup.BackSpeedMax = preset.CarSetup.BackSpeedMax;
                setup.SteeringAngleMax = preset.CarSetup.SteeringAngleMax;
                setup.SteeringSpeed = preset.CarSetup.SteeringSpeed;
                setup.BrakeInput = preset.CarSetup.BrakeInput;
                setup.HandbrakeInput = preset.CarSetup.HandbrakeInput;
                setup.DriftMultiplier = preset.CarSetup.DriftMultiplier;
                setup.EngineRpmMax = preset.CarSetup.EngineRpmMax;
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