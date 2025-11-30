using System;
using Core;
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
        
        public void SetAllParameters(CarView carView)
        {
            for (var index = 0; index < carView.CarWheelInfos.Count; index++)
            {
                var wheelInfo = carView.CarWheelInfos[index];
                wheelInfo.LeftWheel = LeftWheel;
                wheelInfo.RightWheel = RightWheel;  
                wheelInfo.LeftVisual = LeftVisual;
                wheelInfo.RightVisual = RightVisual;
                wheelInfo.Motor = Motor;
                wheelInfo.Steering = Steering;
            }
        }

        public void SetFrontWheelParameters(CarView carView)
        {
            carView.CarWheelInfos[0].LeftWheel = LeftWheel;
            carView.CarWheelInfos[0].RightWheel = RightWheel;
            carView.CarWheelInfos[0].LeftVisual = LeftVisual;
            carView.CarWheelInfos[0].RightVisual = RightVisual;
            carView.CarWheelInfos[0].Motor = Motor;
            carView.CarWheelInfos[0].Steering = Steering;
        }
        
        public void SetBackWheelParameters(CarView carView)
        {
            carView.CarWheelInfos[1].LeftWheel = LeftWheel;
            carView.CarWheelInfos[1].RightWheel = RightWheel;
            carView.CarWheelInfos[1].LeftVisual = LeftVisual;
            carView.CarWheelInfos[1].RightVisual = RightVisual;
            carView.CarWheelInfos[1].Motor = Motor;
            carView.CarWheelInfos[1].Steering = Steering;
        }
    }
}