using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class CarParameters
    {
        public float Mass;
        public bool AutomaticCenterOfMass;
        public Vector3 CenterOfMass;
        
        public void SetCarParameters(Rigidbody car)
        {
            car.mass = Mass;
            car.automaticCenterOfMass = AutomaticCenterOfMass;
            car.centerOfMass = CenterOfMass;
        }
    }
}