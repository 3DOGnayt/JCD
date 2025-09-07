using Scellecs.Morpeh;
using UnityEngine;

namespace Components
{
    //CommonComponents
    public struct TransformComponent : IComponent { public Transform Value; }
    public struct PositionComponent : IComponent { public Vector3 Value; }
    public struct RotationComponent : IComponent { public Vector3 Value; }
    public struct ScaleComponent : IComponent { public float Value; }
    
    //Tags
    public struct PlayerTagComponent : IComponent { }
}