using System;
using SadPumpkin.Game.Pitstop.Core.Code.Util;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public class CarComponent : MonoBehaviour
    {
        [Serializable]
        public class CarVisionData
        {
            public float SampleStartRadiusMultipler = 2f;
            public float SampleLengthRadiusMultiplier = 4f;
            public int TotalForwardSamples = 5;
            public int TotalPeripheralSamples = 2;
            public AnimationCurve ForwardSampleRadiusByStep = AnimationCurve.Linear(0f, 1f, 1f, 1.5f);
            public AnimationCurve ForwardSampleOffsetByStep = AnimationCurve.Linear(0f, 1.5f, 1f, 1.5f);
            public AnimationCurve PeripheralSampleOffsetByForwardStep = AnimationCurve.Linear(0f, 1.5f, 1f, 3f);
        }

        private struct VisionResult
        {
            public enum Result
            {
                // NOTE: Ordered so that higher numbers are MORE IMPORTANT visual indicators.
                Track = 0,
                Goal = 1,
                Car = 2,
                Obstacle = 3,
                Invalid = 4,
            }

            public Vector3 SamplePos;
            public float SampleRadius;
            public Result ResultType;
            public RaycastHit HitInfo;

            public VisionResult(Vector3 samplePos, float sampleRadius, Result result, RaycastHit hitInfo)
            {
                SamplePos = samplePos;
                SampleRadius = sampleRadius;
                ResultType = result;
                HitInfo = hitInfo;
            }
        }

        [Serializable]
        public class CarControlData
        {
            public float MaxTurnSpeed = 3f;
            public float MaxSpeed = 30f;
            public AnimationCurve MaxAccelerationBySpeed = AnimationCurve.EaseInOut(0f, 10f, 1f, 0f);
            public AnimationCurve MaxDecelerationBySpeed = AnimationCurve.EaseInOut(0f, 30f, 1f, 10f);
            public AnimationCurve TargetSpeedRatioByPredictDistanceRatio = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            public AnimationCurve TargetSpeedRatioByTurnRatio = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        }

        private static int CAR_LAYER;
        private static int TRACK_LAYER;
        private static int OBSTACLE_LAYER;
        private static int GOAL_LAYER;

        public Transform LeftWheel;
        public Transform RightWheel;
        public Transform EyePoint;
        public Collider CarCollider;

        public float CurrentSpeed = 0f;

        public CarVisionData VisionSettings = new CarVisionData();
        public CarControlData ControlSettings = new CarControlData();

        private VisionResult[,] _visionRange = new VisionResult[0, 0];
        private Vector3 _drivingTargetPoint;
        
        private void Awake()
        {
            CAR_LAYER = LayerMask.NameToLayer("Car");
            TRACK_LAYER = LayerMask.NameToLayer("Track");
            OBSTACLE_LAYER = LayerMask.NameToLayer("Obstacle");
            GOAL_LAYER = LayerMask.NameToLayer("InteractionPoint");
        }
        
        public void UpdateCar(float timeStep)
        {
            UpdateVision();
            UpdateGoalPoint();
            UpdateSteering(timeStep);
            UpdateVelocity(timeStep);
        }

        private void UpdateVision()
        {
            // Periphery extends in both directions from the center point
            int fullPeripheralWidth = VisionSettings.TotalPeripheralSamples * 2 + 1;

            // Ensure the vision range array is the right size and clear of stale data
            if (_visionRange == null ||
                _visionRange.GetLength(0) != fullPeripheralWidth ||
                _visionRange.GetLength(1) != VisionSettings.TotalForwardSamples)
            {
                _visionRange = new VisionResult[fullPeripheralWidth, VisionSettings.TotalForwardSamples];
            }
            else
            {
                Array.Clear(_visionRange, 0, _visionRange.Length);
            }

            // Calculate the 'origin' point where the car currently exists
            (int x, int y) originIndex = (_visionRange.GetLength(0) / 2, 0);
            Vector3 originPos = EyePoint.localPosition;

            // Calculate view direction for car
            Vector3 vectorToGoal = _drivingTargetPoint - transform.position;
            Vector3 directionToGoal = vectorToGoal.normalized;
            Quaternion towardGoal = Quaternion.LookRotation(directionToGoal, Vector3.up);
            Quaternion lookRotation = Quaternion.Lerp(
                towardGoal,
                transform.rotation,
                0.5f);
            EyePoint.rotation = lookRotation;
            
            // Iterate through vision points
            RaycastHit hit;
            for (int y = 0; y < _visionRange.GetLength(1); y++)
            {
                float percDistForward = y.Remap(0, _visionRange.GetLength(1), 0, 1);
                float sampleRadius = VisionSettings.ForwardSampleRadiusByStep.Evaluate(percDistForward);
                float forwardOffset = VisionSettings.ForwardSampleOffsetByStep.Evaluate(percDistForward);
                float peripheralOffset = VisionSettings.PeripheralSampleOffsetByForwardStep.Evaluate(percDistForward);
                for (int x = 0; x < _visionRange.GetLength(0); x++)
                {
                    Vector3 samplePos = new Vector3(
                        originPos.x + (x - originIndex.x) * peripheralOffset,
                        originPos.y + sampleRadius * VisionSettings.SampleStartRadiusMultipler,
                        originPos.z + (y - originIndex.y) * forwardOffset);
                    samplePos = EyePoint.TransformPoint(samplePos);

                    if (Physics.SphereCast(samplePos, sampleRadius, Vector3.down, out hit, sampleRadius * VisionSettings.SampleLengthRadiusMultiplier))
                    {
                        VisionResult.Result resultType = VisionResult.Result.Invalid;
                        if (hit.collider == CarCollider)
                        {
                            resultType = VisionResult.Result.Track;
                        }
                        else
                        {
                            int hitLayer = hit.collider.gameObject.layer;
                            if (hitLayer == CAR_LAYER)
                            {
                                resultType = VisionResult.Result.Car;
                            }
                            else if (hitLayer == TRACK_LAYER)
                            {
                                resultType = VisionResult.Result.Track;
                            }
                            else if (hitLayer == OBSTACLE_LAYER)
                            {
                                resultType = VisionResult.Result.Obstacle;
                            }
                            else if (hitLayer == GOAL_LAYER)
                            {
                                resultType = VisionResult.Result.Goal;
                            }
                        }

                        _visionRange[x, y] = new VisionResult(samplePos, sampleRadius, resultType, hit);
                    }
                    else
                    {
                        _visionRange[x, y] = new VisionResult(samplePos, sampleRadius, VisionResult.Result.Invalid, default);
                    }
                }
            }

            // Doctor vision points to occlude theoretically blocked points
            for (int x = 0; x < _visionRange.GetLength(0); x++)
            {
                VisionResult.Result currentHighestResultInColumn = VisionResult.Result.Track;
                for (int y = 0; y < _visionRange.GetLength(1); y++)
                {
                    VisionResult visionResult = _visionRange[x, y];
                    if (currentHighestResultInColumn < visionResult.ResultType)
                    {
                        currentHighestResultInColumn = visionResult.ResultType;
                    }
                    else
                    {
                        visionResult.ResultType = currentHighestResultInColumn;
                        _visionRange[x, y] = visionResult;
                    }
                }
            }
        }

        private void UpdateGoalPoint()
        {
            bool GetPassableFromResult(VisionResult result)
            {
                // TODO allow going through pit or other points sometimes
                return result.ResultType == VisionResult.Result.Track;
            }

            int MaxPassableFromColumn(VisionResult[,] vision, int column)
            {
                int length = 0;
                for (int y = 0; y < vision.GetLength(1); y++)
                {
                    if (GetPassableFromResult(vision[column, y]))
                    {
                        length++;
                    }
                    else
                    {
                        break;
                    }
                }

                return length;
            }

            _drivingTargetPoint = transform.position + transform.forward;

            for (int x = 0; x < _visionRange.GetLength(0); x++)
            {
                int maxPassable = MaxPassableFromColumn(_visionRange, x);
                if (maxPassable > 0)
                {
                    Vector3 drivingPoint = _visionRange[x, maxPassable - 1].SamplePos;

                    if (Vector3.Distance(transform.position, _drivingTargetPoint) <
                        Vector3.Distance(transform.position, drivingPoint))
                    {
                        _drivingTargetPoint = new Vector3(drivingPoint.x, transform.position.y, drivingPoint.z);
                    }
                }
            }
        }

        private void UpdateSteering(float timeStep)
        {
            Vector3 vectorToGoal = _drivingTargetPoint - transform.position;
            Vector3 directionToGoal = vectorToGoal.normalized;

            Quaternion towardGoal = Quaternion.LookRotation(directionToGoal, Vector3.up);
            Quaternion carRotation = Quaternion.RotateTowards(transform.rotation, towardGoal, ControlSettings.MaxTurnSpeed * timeStep);

            transform.rotation = carRotation;
        }

        private void UpdateVelocity(float timeStep)
        {
            float speedAsPercOfMaxSpeed = CurrentSpeed / ControlSettings.MaxTurnSpeed;

            // Gauge distance to goal point
            Vector3 maxPredictPoint = _visionRange[VisionSettings.TotalPeripheralSamples, VisionSettings.TotalForwardSamples - 1].SamplePos;
            maxPredictPoint.y = transform.position.y;
            float distanceToGoal = Vector3.Distance(transform.position, _drivingTargetPoint);
            float distanceToMaxPredict = Vector3.Distance(transform.position, maxPredictPoint);
            float percPredictDistance = distanceToGoal / distanceToMaxPredict;
            float speedRatioForGoalDistance = ControlSettings.TargetSpeedRatioByPredictDistanceRatio.Evaluate(percPredictDistance);
            
            // Gauge turning radius
            Vector3 vectorToGoal = _drivingTargetPoint - transform.position;
            Vector3 directionToGoal = vectorToGoal.normalized;
            Quaternion towardGoal = Quaternion.LookRotation(directionToGoal, Vector3.up);
            float turnAngle = Quaternion.Angle(transform.rotation, towardGoal);
            float percTurnAngle = turnAngle / ControlSettings.MaxTurnSpeed;
            float speedRatioForTurnAngle = ControlSettings.TargetSpeedRatioByTurnRatio.Evaluate(percTurnAngle);

            float speedRatioGoal = Mathf.Min(speedRatioForGoalDistance, speedRatioForTurnAngle);
            float speedGoal = Mathf.Clamp(speedRatioGoal * ControlSettings.MaxSpeed, 0f, ControlSettings.MaxSpeed);
            if(speedGoal > CurrentSpeed)
            {
                float maxAccelAtSpeed = ControlSettings.MaxAccelerationBySpeed.Evaluate(speedAsPercOfMaxSpeed);
                CurrentSpeed += maxAccelAtSpeed * timeStep;
            }
            else
            {
                float maxDecelAtSpeed = ControlSettings.MaxDecelerationBySpeed.Evaluate(speedAsPercOfMaxSpeed);
                CurrentSpeed -= maxDecelAtSpeed * timeStep;
            }

            transform.position += transform.forward * CurrentSpeed * timeStep;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw Vision Results
            for (int y = 0; y < _visionRange.GetLength(1); y++)
            {
                for (int x = 0; x < _visionRange.GetLength(0); x++)
                {
                    VisionResult visionResult = _visionRange[x, y];

                    Color gizmoColor = Color.black;
                    switch (visionResult.ResultType)
                    {
                        case VisionResult.Result.Invalid:
                            gizmoColor = Color.black;
                            break;
                        case VisionResult.Result.Obstacle:
                            gizmoColor = Color.magenta;
                            break;
                        case VisionResult.Result.Car:
                            gizmoColor = Color.red;
                            break;
                        case VisionResult.Result.Track:
                            gizmoColor = Color.green;
                            break;
                        case VisionResult.Result.Goal:
                            gizmoColor = Color.blue;
                            break;
                    }

                    Gizmos.color = Color.Lerp(gizmoColor, Color.clear, 0.5f);
                    Gizmos.DrawWireSphere(
                        Vector3.Lerp(visionResult.SamplePos, visionResult.SamplePos + Vector3.down * visionResult.SampleRadius * VisionSettings.SampleLengthRadiusMultiplier, 0.5f),
                        visionResult.SampleRadius);
                }
            }

            // Draw Drive Target
            Gizmos.color = Color.Lerp(Color.white, Color.clear, 0.5f);
            Gizmos.DrawWireMesh(GetComponentInChildren<MeshCollider>().sharedMesh, _drivingTargetPoint, transform.rotation, transform.localScale);
            Gizmos.DrawLine(transform.position, _drivingTargetPoint);
        }
    }
}