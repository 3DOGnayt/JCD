using Configs.Impl;
using Data;
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

            preset.WheelInfos.Clear();

            var wheelsModelsRoot = car.transform.Find("WheelsModels");
            var wheelsCollidersRoot = car.transform.Find("WheelsColliders");

            if (wheelsModelsRoot != null && wheelsCollidersRoot != null)
            {
                var frontInfo = new WheelInfo
                {
                    LeftWheel  = wheelsCollidersRoot.Find("FL_Collider")?.GetComponent<WheelCollider>(),
                    RightWheel = wheelsCollidersRoot.Find("FR_Collider")?.GetComponent<WheelCollider>(),
                    LeftVisual  = wheelsModelsRoot.Find("Wheel_FL_Model"),
                    RightVisual = wheelsModelsRoot.Find("Wheel_FR_Model"),
                    Motor = true,
                    Steering = true
                };
                preset.WheelInfos.Add(frontInfo);

                var backInfo = new WheelInfo
                {
                    LeftWheel  = wheelsCollidersRoot.Find("BL_Collider")?.GetComponent<WheelCollider>(),
                    RightWheel = wheelsCollidersRoot.Find("BR_Collider")?.GetComponent<WheelCollider>(),
                    LeftVisual  = wheelsModelsRoot.Find("Wheel_BL_Model"),
                    RightVisual = wheelsModelsRoot.Find("Wheel_BR_Model"),
                    Motor = false,
                    Steering = false
                };
                preset.WheelInfos.Add(backInfo);
            }
            else
            {
                Debug.LogWarning($"⚠️ У {car.name} нет WheelsModels или WheelsColliders → WheelInfos не заполнен.");
            }

            EditorUtility.SetDirty(preset);
            Debug.Log("✅ WheelInfos сохранены: передние + задние колёса");
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