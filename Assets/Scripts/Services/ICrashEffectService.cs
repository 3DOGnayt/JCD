using UnityEngine;

namespace Services
{
    public interface ICrashEffectService
    {
        void UpdateCrash(Rigidbody rigidbody, bool hasContact, Vector3 contactPoint);
        void ResetPool();
    }
}