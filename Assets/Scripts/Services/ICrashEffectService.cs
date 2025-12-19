using UnityEngine;

namespace Services
{
    public interface ICrashEffectService
    {
        void UpdateCrash(Rigidbody rb, bool hasContact, Vector3 contactPoint);
    }
}