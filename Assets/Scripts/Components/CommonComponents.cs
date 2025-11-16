using Scellecs.Morpeh;
using UnityEngine;

namespace Components
{
    //CommonComponents
    public struct TransformComponent : IComponent { public Transform Value; }
    public struct PositionComponent : IComponent { public Vector3 Value; }
    public struct RotationComponent : IComponent { public Quaternion Value; }
    public struct ScaleComponent : IComponent { public float Value; }
    public struct RigidbodyComponent : IComponent { public Rigidbody Value; }

    
    //Tags
    public struct PlayerTagComponent : IComponent { }
    
    //Car sub components
    public struct VerticalInputComponent : IComponent { public float Value; }
    public struct HorizontalInputComponent : IComponent { public float Value; }
}