using System.Collections.Generic;
using UnityEngine;

namespace Services
{
    public interface ISkidSmokeService
    {
        int BeginSkid(List<Vector3> wheelPositions);
        void UpdateSkid(int handle, List<Vector3> wheelPositions);
        void EndSkid(int handle);
        void ResetPool();
    }
}