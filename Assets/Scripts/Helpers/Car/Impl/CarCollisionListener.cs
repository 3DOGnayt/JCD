using UnityEngine;

namespace Helpers.Car.Impl
{
    public sealed class CarCollisionListener : MonoBehaviour
    {
        [Header("Collision settings")] 
        [SerializeField] private float _minRelativeVelocity = 3f;
        [SerializeField] private float _maxRelativeVelocity = 15f;
        [SerializeField] private LayerMask _damageableMask;

        public bool HasCollision { get; private set; }
        public Vector3 Point { get; private set; }
        public Vector3 Normal { get; private set; }
        public float Intensity { get; private set; }

        public void Clear() => HasCollision = false;

        private bool IsDamageableLayer(int layer) => (_damageableMask.value & (1 << layer)) != 0;
        private void OnCollisionEnter(Collision collision) => ProcessCollision(collision);
        private void OnCollisionStay(Collision collision) => ProcessCollision(collision);

        private void ProcessCollision(Collision collision)
        {
            if (!IsDamageableLayer(collision.gameObject.layer))
                return;

            var relVel = collision.relativeVelocity.magnitude;
            if (relVel < _minRelativeVelocity)
                return;

            if (collision.contactCount == 0)
                return;

            var contact = collision.GetContact(0);

            HasCollision = true;
            Point = contact.point;
            Normal = contact.normal;

            Intensity = Mathf.InverseLerp(_minRelativeVelocity, _maxRelativeVelocity, relVel);
        }
    }
}