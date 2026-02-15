using System.Collections.Generic;
using Data;
using Data.HelperClass;
using Helpers.CarView.Impl;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    [CustomEditor(typeof(CarView))]
    public class WheelSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var carSetup = (CarView)target;

            if (GUILayout.Button("Auto Fill Wheels"))
            {
                AutoFill(carSetup);
            }
        }

        private void AutoFill(CarView carSetup)
        {
            var axles = new List<WheelInfoSetup>();

            var allChildren = carSetup.transform.GetComponentsInChildren<Transform>();

            var front = new WheelInfoSetup();
            var back = new WheelInfoSetup();

            foreach (var child in allChildren)
            {
                var n = child.name.ToLower();

                if (n.Contains("fl_collider")) front.LeftWheel = child.GetComponent<WheelCollider>();
                if (n.Contains("fr_collider")) front.RightWheel = child.GetComponent<WheelCollider>();
                if (n.Contains("bl_collider")) back.LeftWheel = child.GetComponent<WheelCollider>();
                if (n.Contains("br_collider")) back.RightWheel = child.GetComponent<WheelCollider>();

                if (n.Contains("wheel_fl_model")) front.LeftVisual = child;
                if (n.Contains("wheel_fr_model")) front.RightVisual = child;
                if (n.Contains("wheel_bl_model")) back.LeftVisual = child;
                if (n.Contains("wheel_br_model")) back.RightVisual = child;
            }

            axles.Add(front);
            axles.Add(back);

            carSetup.CarWheelInfos = axles;
            EditorUtility.SetDirty(carSetup);
        }
    }
}