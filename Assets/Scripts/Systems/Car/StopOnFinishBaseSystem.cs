using Scellecs.Morpeh;
using UnityEngine;
using Zenject;

namespace Systems.Car
{
    public abstract class StopOnFinishBaseSystem : IFixedSystem
    {
        protected const float StopDurationSeconds = 1f;

        [Inject] public World World { get; set; }

        protected struct StopState
        {
            public bool IsStopping;
            public float StopStartTime;
            public Rigidbody TargetRigidbody;
            public Vector3 StartLinearVelocity;
            public Vector3 StartAngularVelocity;
        }

        protected void BeginStop(ref StopState state, Rigidbody rigidbody)
        {
            if (rigidbody == null)
                return;

            state.IsStopping = true;
            state.StopStartTime = Time.time;
            state.TargetRigidbody = rigidbody;
            state.StartLinearVelocity = rigidbody.velocity;
            state.StartAngularVelocity = rigidbody.angularVelocity;
        }

        protected void UpdateStop(ref StopState state)
        {
            if (!state.IsStopping || state.TargetRigidbody == null)
                return;

            var elapsed = Time.time - state.StopStartTime;
            var time = Mathf.Clamp01(elapsed / StopDurationSeconds);

            state.TargetRigidbody.velocity = Vector3.Lerp(state.StartLinearVelocity, Vector3.zero, time);
            state.TargetRigidbody.angularVelocity = Vector3.Lerp(state.StartAngularVelocity, Vector3.zero, time);

            if (time >= 1f)
            {
                state.TargetRigidbody.velocity = Vector3.zero;
                state.TargetRigidbody.angularVelocity = Vector3.zero;
                state.TargetRigidbody.Sleep();
                state.IsStopping = false;
                state.TargetRigidbody = null;
            }
        }

        protected void ResetStopState(ref StopState state)
        {
            state.IsStopping = false;
            state.TargetRigidbody = null;
            state.StartLinearVelocity = Vector3.zero;
            state.StartAngularVelocity = Vector3.zero;
        }

        public abstract void OnAwake();
        public abstract void OnUpdate(float deltaTime);
        public abstract void Dispose();
    }
}