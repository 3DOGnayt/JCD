using System;

namespace Data.HelperClass
{
    [Serializable]
    public class CarMovementArcadeAssistSetup
    {
        public bool UseArcadeAssist = true;
        public bool UseArcadeAssistInDrift = true;
        public float ArcadeAssistMinSpeedKmh;
        public float ArcadeAssistLerpSpeed = 1f;
        public float DriftAssistForwardSpeedMultiplier = 1f;
    }
}