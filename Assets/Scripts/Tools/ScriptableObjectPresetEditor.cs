using Core.Configs.Impl;
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

            if (GUILayout.Button("Setup parameters from Car to SO"))
                SaveFromCar(preset);

            if (GUILayout.Button("Setup parameters from SO to Car")) 
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
            if (wheels.Length > 0)
            {
                var wc = wheels[0];

                preset.WheelParameters.Mass = wc.mass;
                preset.WheelParameters.Radius = wc.radius;
                preset.WheelParameters.DampingRate = wc.wheelDampingRate;
                preset.WheelParameters.SuspensionDistance = wc.suspensionDistance;
                preset.WheelParameters.ForceAppPointDistance = wc.forceAppPointDistance;
                preset.WheelParameters.Center = wc.center;

                var spring = wc.suspensionSpring;
                preset.WheelSubParameters.Spring = spring.spring;
                preset.WheelSubParameters.Damper = spring.damper;
                preset.WheelSubParameters.TargetPosition = spring.targetPosition;

                var fFriction = wc.forwardFriction;
                preset.WheelSubParameters.ForwardFriction.ExtremumSlip = fFriction.extremumSlip;
                preset.WheelSubParameters.ForwardFriction.ExtremumValue = fFriction.extremumValue;
                preset.WheelSubParameters.ForwardFriction.AsymptoteSlip = fFriction.asymptoteSlip;
                preset.WheelSubParameters.ForwardFriction.AsymptoteValue = fFriction.asymptoteValue;
                preset.WheelSubParameters.ForwardFriction.Stiffness = fFriction.stiffness;

                var sFriction = wc.sidewaysFriction;
                preset.WheelSubParameters.SidewaysFriction.ExtremumSlip = sFriction.extremumSlip;
                preset.WheelSubParameters.SidewaysFriction.ExtremumValue = sFriction.extremumValue;
                preset.WheelSubParameters.SidewaysFriction.AsymptoteSlip = sFriction.asymptoteSlip;
                preset.WheelSubParameters.SidewaysFriction.AsymptoteValue = sFriction.asymptoteValue;
                preset.WheelSubParameters.SidewaysFriction.Stiffness = sFriction.stiffness;
            }

            EditorUtility.SetDirty(preset);
            Debug.Log("✅ Parameters saved in ScriptableObject");
        }

        private void ApplyToCar(CarPreset preset)
        {
            var car = preset.Car;
            var rigidbody = car.GetComponentInChildren<Rigidbody>();
            if (rigidbody != null)
            {
                preset.CarParameters.SetCarParameters(rigidbody);
            }

            var wheels = car.GetComponentsInChildren<WheelCollider>();
            foreach (var wc in wheels)
            {
                preset.WheelParameters.SetWheelParameters(wc);
                preset.WheelSubParameters.SetParameters(wc);
            }

            Debug.Log("✅ Parameters from ScriptableObject applied to car");
        }
    }
}