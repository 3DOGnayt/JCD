using Configs.Impl;
using Helpers.CarView.Impl;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [CustomEditor(typeof(CarPresetParameters))]
    public class ScriptableObjectPresetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var preset = (CarPresetParameters)target;

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

        private void SaveFromCar(CarPresetParameters presetParameters)
        {
            var car = presetParameters.Car;
            var rb = car.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                presetParameters.CarMassSetup.Mass = rb.mass;
                presetParameters.CarMassSetup.AutomaticCenterOfMass = rb.automaticCenterOfMass;
                presetParameters.CarMassSetup.CenterOfMass = rb.centerOfMass;
            }

            var carView = car.GetComponent<CarView>();
            if (carView != null && carView.CarSetup != null)
            {
                var setup = carView.CarSetup;
                presetParameters.CarSetup.SpeedMax = setup.SpeedMax;
                presetParameters.CarSetup.GearCount = setup.GearCount;
                presetParameters.CarSetup.BackSpeedMax = setup.BackSpeedMax;
                presetParameters.CarSetup.SteeringAngleMax = setup.SteeringAngleMax;
                presetParameters.CarSetup.SteeringSpeed = setup.SteeringSpeed;
                presetParameters.CarSetup.BrakeInput = setup.BrakeInput;
                presetParameters.CarSetup.HandbrakeInput = setup.HandbrakeInput;
                presetParameters.CarSetup.DriftMultiplier = setup.DriftMultiplier;
                presetParameters.CarSetup.EngineRpmMax = setup.EngineRpmMax;
            }

            var wheels = car.GetComponentsInChildren<WheelCollider>();
            if (wheels.Length >= 4)
            {
                presetParameters.FrontWheelSetup.SaveFromWheel(wheels[0]);
                presetParameters.FrontWheelSetup.SaveFromWheel(wheels[1]);
                presetParameters.BackWheelSetup.SaveFromWheel(wheels[wheels.Length - 2]);
                presetParameters.BackWheelSetup.SaveFromWheel(wheels[wheels.Length - 1]);
            }

            EditorUtility.SetDirty(presetParameters);
            EditorUtility.SetDirty(car);
            PrefabUtility.RecordPrefabInstancePropertyModifications(car);

            Debug.Log($"✅ {car.name}: данные сохранены в CarPreset");
        }

        private void ApplyToCar(CarPresetParameters presetParameters)
        {
            var car = presetParameters.Car;
            var rb = car.GetComponentInChildren<Rigidbody>();
            if (rb != null)
                presetParameters.CarMassSetup.SetCarParameters(rb);

            var carView = car.GetComponent<CarView>();
            if (carView != null && carView.CarSetup != null)
            {
                var setup = carView.CarSetup;
                setup.SpeedMax = presetParameters.CarSetup.SpeedMax;
                setup.GearCount = presetParameters.CarSetup.GearCount;
                setup.BackSpeedMax = presetParameters.CarSetup.BackSpeedMax;
                setup.SteeringAngleMax = presetParameters.CarSetup.SteeringAngleMax;
                setup.SteeringSpeed = presetParameters.CarSetup.SteeringSpeed;
                setup.BrakeInput = presetParameters.CarSetup.BrakeInput;
                setup.HandbrakeInput = presetParameters.CarSetup.HandbrakeInput;
                setup.DriftMultiplier = presetParameters.CarSetup.DriftMultiplier;
                setup.EngineRpmMax = presetParameters.CarSetup.EngineRpmMax;
            }

            var wheels = car.GetComponentsInChildren<WheelCollider>();
            if (wheels.Length >= 4)
            {
                presetParameters.FrontWheelSetup.SetAllParameters(wheels[0]);
                presetParameters.FrontWheelSetup.SetAllParameters(wheels[1]);
                presetParameters.BackWheelSetup.SetAllParameters(wheels[wheels.Length - 2]);
                presetParameters.BackWheelSetup.SetAllParameters(wheels[wheels.Length - 1]);
            }

            EditorUtility.SetDirty(car);
            PrefabUtility.RecordPrefabInstancePropertyModifications(car);

            Debug.Log($"✅ {car.name}: данные из CarPreset применены на Car");
        }
    }
}