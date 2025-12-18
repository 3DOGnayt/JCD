using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class CarEffects
    {
        [Header("CAR EFFECTS")]
        [Space] 
        public TrailRenderer[] TracesWheels;
        [Space]
        public GameObject[] SmokeWheels;
        [Space]
        public ParticleSystem[] DamageSparks;
    }
}