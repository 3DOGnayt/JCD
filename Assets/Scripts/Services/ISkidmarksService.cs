using UnityEngine;

namespace Services
{
    public interface ISkidmarksService
    {
        void UpdateWheel(Rigidbody rb, WheelCollider wheel, bool skidNow);
        void ResetMesh();
    }
}