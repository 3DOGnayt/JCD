using System;
using Configs.Impl;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class WheelInfo
    {
        public WheelCollider LeftWheel;
        public WheelCollider RightWheel;
        public Transform LeftVisual;
        public Transform RightVisual;
        public bool Motor;
        public bool Steering;
        
        public void SetAllParameters(CarPreset carPreset)
        {
            for (var index = 0; index < carPreset.WheelInfos.Count; index++)
            {
                var wheelInfo = carPreset.WheelInfos[index];
                wheelInfo.LeftWheel = LeftWheel;
                wheelInfo.RightWheel = RightWheel;  
                wheelInfo.LeftVisual = LeftVisual;
                wheelInfo.RightVisual = RightVisual;
                wheelInfo.Motor = Motor;
                wheelInfo.Steering = Steering;
            }
        }

        public void SetFrontWheelParameters(CarPreset carPreset)
        {
            carPreset.WheelInfos[0].LeftWheel = LeftWheel;
            carPreset.WheelInfos[0].RightWheel = RightWheel;
            carPreset.WheelInfos[0].LeftVisual = LeftVisual;
            carPreset.WheelInfos[0].RightVisual = RightVisual;
            carPreset.WheelInfos[0].Motor = Motor;
            carPreset.WheelInfos[0].Steering = Steering;
        }
        
        public void SetBackWheelParameters(CarPreset carPreset)
        {
            carPreset.WheelInfos[1].LeftWheel = LeftWheel;
            carPreset.WheelInfos[1].RightWheel = RightWheel;
            carPreset.WheelInfos[1].LeftVisual = LeftVisual;
            carPreset.WheelInfos[1].RightVisual = RightVisual;
            carPreset.WheelInfos[1].Motor = Motor;
            carPreset.WheelInfos[1].Steering = Steering;
        }
    }
}