using UnityEngine;

namespace Tools
{
    [ExecuteAlways] // чтобы работало и в Play, и в Edit (не обязательно)
    public class VelocityDebugGizmo : MonoBehaviour
    {
        [Header("Refs")]
        public Rigidbody targetRigidbody;

        [Header("Draw settings")]
        public float arrowLength = 3f;
        public float arrowsHeightOffset = 0.2f;
        public Color forwardColor = Color.green;
        public Color velocityColor = Color.cyan;

        private void Reset()
        {
            targetRigidbody = GetComponent<Rigidbody>();
        }

        private void OnDrawGizmos()
        {
            if (targetRigidbody == null)
                targetRigidbody = GetComponent<Rigidbody>();

            if (targetRigidbody == null)
                return;

            // Точка, откуда рисуем стрелки (чуть выше центра машины)
            Vector3 origin = transform.position + Vector3.up * arrowsHeightOffset;

            // 1) Направление машины (transform.forward)
            Gizmos.color = forwardColor;
            Gizmos.DrawLine(origin, origin + transform.forward * arrowLength);

            // 2) Направление движения (velocity)
            Vector3 vel = targetRigidbody.velocity;
            if (vel.sqrMagnitude > 0.0001f)
            {
                Vector3 velDir = vel.normalized;
                Gizmos.color = velocityColor;
                Gizmos.DrawLine(origin, origin + velDir * arrowLength);
            }
        }
    }
}