using Components;
using Scellecs.Morpeh;
using UnityEngine;
using UnityEngine.Splines;

namespace Systems.Car
{
    public sealed partial class OpponentRailSystem
    {
        public void OnUpdate(float deltaTime)
        {
            foreach (var opponent in _opponents)
            {
                var tr = _transformStash.Get(opponent).Value;
                var rb = _rigidbodyStash.Get(opponent).Value;
                if (tr == null || rb == null)
                    continue;

                ref var follow = ref _followStash.Get(opponent);
                var opponentIndex = _gameSelectionParameters != null ? _gameSelectionParameters.SelectedOpponentIndex : 0;

                if (!TryResolveSpline(opponentIndex, out var container, out var spline))
                    continue;

                var isFinished = _unitRaceTimerService != null && _unitRaceTimerService.IsFinished(opponent);
                var isStopped = !_raceStarted;

                InitializeProgressIfNeeded(container, spline, tr.position, ref follow);
                if (!TryGetCurrentDirection(container, spline, follow.ProgressT, tr.forward, out var currentDir))
                    continue;

                var sampledSpeedKmh = SampleSpeedKmh(rb, currentDir);
                SyncRailSpeed(ref follow, sampledSpeedKmh, isStopped);

                var targetSpeed = ResolveTargetSpeed(isStopped, isFinished, spline, ref follow);
                var speedKmh = isStopped
                    ? 0f
                    : UpdateRailSpeed(follow.CurrentSpeedKmh, targetSpeed, deltaTime, isFinished, ref follow);

                ApplyInputs(opponent, isStopped, isFinished, speedKmh, targetSpeed);

                if (!isStopped)
                    AdvanceProgress(spline, speedKmh, deltaTime, ref follow);

                UpdateRailMovement(rb, tr, container, spline, follow.ProgressT, speedKmh);
            }
        }

        private static void InitializeProgressIfNeeded(
            SplineContainer container,
            Spline spline,
            Vector3 worldPosition,
            ref OpponentSplineFollowComponent follow)
        {
            if (follow.RailInitialized)
                return;

            InitializeRailProgress(container, spline, worldPosition, ref follow);
        }

        private static bool TryGetCurrentDirection(
            SplineContainer container,
            Spline spline,
            float progressT,
            Vector3 fallbackForward,
            out Vector3 direction)
        {
            direction = fallbackForward;

            if (!container.Evaluate(spline, progressT, out _, out var currentTangent, out _))
                return false;

            direction = ResolveDirection((Vector3)currentTangent, fallbackForward);
            return true;
        }

        private static Vector3 ResolveDirection(Vector3 tangent, Vector3 fallbackForward)
        {
            if (tangent.sqrMagnitude < 0.001f)
                return fallbackForward;

            return tangent.normalized;
        }

        private static float SampleSpeedKmh(Rigidbody rb, Vector3 direction)
        {
            var speedMps = Vector3.Dot(rb.velocity, direction);
            return Mathf.Abs(speedMps) * 3.6f;
        }

        private static void SyncRailSpeed(ref OpponentSplineFollowComponent follow, float sampledSpeedKmh, bool isStopped)
        {
            if (follow.CurrentSpeedKmh <= 0f && sampledSpeedKmh > 0f)
                follow.CurrentSpeedKmh = sampledSpeedKmh;

            if (isStopped)
                follow.CurrentSpeedKmh = 0f;
        }

        private static float ResolveTargetSpeed(bool isStopped, bool isFinished, Spline spline, ref OpponentSplineFollowComponent follow)
        {
            if (isStopped)
                return 0f;

            if (isFinished)
                return 0f;

            var targetSpeed = Mathf.Max(0f, follow.TargetSpeedKmh);
            return targetSpeed * ResolveBrakeSpeedScale(spline, follow.ProgressT, ref follow);
        }

        private void ApplyInputs(Entity opponent, bool isStopped, bool isFinished, float speedKmh, float targetSpeedKmh)
        {
            var verticalInput = isStopped ? 0f : ResolveSpeedInput(speedKmh, targetSpeedKmh);
            _verticalStash.Get(opponent).Value = verticalInput;
            _horizontalStash.Get(opponent).Value = 0f;
            _handbrakeStash.Get(opponent).Value = isStopped || isFinished;
        }

        private static void UpdateRailMovement(
            Rigidbody rigidbody,
            Transform transform,
            SplineContainer container,
            Spline spline,
            float progressT,
            float speedKmh)
        {
            if (!container.Evaluate(spline, progressT, out var position, out var tangent, out var upVector))
                return;

            var moveDir = ResolveDirection((Vector3)tangent, transform.forward);
            var rotation = ResolveRailRotation(container, spline, progressT, moveDir, (Vector3)upVector);

            rigidbody.MovePosition((Vector3)position);
            rigidbody.MoveRotation(rotation);
            rigidbody.velocity = moveDir * (speedKmh / 3.6f);
            rigidbody.angularVelocity = Vector3.zero;
        }

        private static float UpdateRailSpeed(
            float currentSpeedKmh,
            float targetSpeedKmh,
            float deltaTime,
            bool isFinished,
            ref OpponentSplineFollowComponent follow)
        {
            const float defaultChangePerSecond = 20f;

            var desiredSpeed = Mathf.Max(0f, targetSpeedKmh);
            var maxChangePerSecond = defaultChangePerSecond;
            if (isFinished && currentSpeedKmh > 0f)
            {
                maxChangePerSecond = currentSpeedKmh / 0.5f;
                var finishedSpeed = Mathf.MoveTowards(currentSpeedKmh, 0f, maxChangePerSecond * deltaTime);
                follow.CurrentSpeedKmh = finishedSpeed;
                return finishedSpeed;
            }

            var input = Mathf.Clamp((desiredSpeed - currentSpeedKmh) / 10f, -1f, 1f);
            var maxChange = Mathf.Abs(input) * maxChangePerSecond * deltaTime;
            var nextSpeed = Mathf.MoveTowards(currentSpeedKmh, desiredSpeed, maxChange);
            if (nextSpeed < 0.05f)
                nextSpeed = 0f;

            follow.CurrentSpeedKmh = nextSpeed;
            return nextSpeed;
        }

        private static float ResolveSpeedInput(float speedKmh, float targetSpeedKmh)
        {
            var speedDelta = targetSpeedKmh - speedKmh;
            if (speedDelta >= 0f)
                return Mathf.Clamp(speedDelta / 10f, 0f, 1f);

            return Mathf.Clamp(speedDelta / 10f, -1f, 0f);
        }
    }
}