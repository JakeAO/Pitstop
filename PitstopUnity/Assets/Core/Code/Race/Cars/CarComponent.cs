using System;
using SadPumpkin.Game.Pitstop.Core.Code.Race;
using SadPumpkin.Game.Pitstop.Core.Code.RTS;
using SadPumpkin.Game.Pitstop.Core.Code.Util;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public class CarComponent : MonoBehaviour
    {
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

        private static int CAR_LAYER;
        private static int TRACK_LAYER;
        private static int OBSTACLE_LAYER;
        private static int GOAL_LAYER;
        private static int GROUND_LAYER;
        
        public Renderer[] Meshes;
        public Renderer MinimapPip;

        public Transform EyePoint;
        public Collider CarCollider;

        [ReadOnly] [ProgressBar(0f, nameof(MaxSpeed))]
        public float CurrentSpeed = 0f;
        public float MaxSpeed => _carControl.MaxSpeed;
        public float CurrentMaxSpeed { get; private set; }

        [ReadOnly] public float CurrentTurnSpeed = 0f;

        [ReadOnly] [ProgressBar(0f, nameof(MaxStamina))]
        public float CurrentDriverStamina = 0f;
        public float MaxStamina => _driverStatus.MaxStamina;
        public float StaminaRecoverySpeed => _driverStatus.StaminaRecovery;

        [ReadOnly] [ProgressBar(0f, nameof(MaxBody))]
        public float CurrentCarBody = 0f;
        public float MaxBody => _carStatus.MaxBodyCondition;

        [ReadOnly] [ProgressBar(0f, nameof(MaxFuel))]
        public float CurrentCarFuel = 0f;
        public float MaxFuel => _carStatus.MaxFuel;

        [ReadOnly] public CarStatus CurrentStatus = CarStatus.Idle;
        [ReadOnly] public CarGoal CurrentGoal = CarGoal.Race;

        [ReadOnly] public int Lap = -1;
        
        private Transform _transform;

        private DriverStatusData _driverStatus;
        private DriverVisionData _driverVision;
        private CarStatusData _carStatus;
        private CarControlData _carControl;
        private PitCarLocation _pitLocation;
        private int _lapsInRace;

        private VisionResult[,] _visionRange = new VisionResult[0, 0];
        private Vector3 _raceTargetPoint;
        private Vector3 _drivingTargetPoint;

        public Vector3 CarForward => _transform.forward;
        public Vector3 RaceForward => (_raceTargetPoint - _transform.position).normalized;
        public Vector3 DriveForward => (_drivingTargetPoint - _transform.position).normalized;

        private float _aiUpdateTimer = 0f;
        
        private void Awake()
        {
            _transform = transform;
            
            CAR_LAYER = LayerMask.NameToLayer("Car");
            TRACK_LAYER = LayerMask.NameToLayer("Track");
            OBSTACLE_LAYER = LayerMask.NameToLayer("Obstacle");
            GOAL_LAYER = LayerMask.NameToLayer("InteractionPoint");
            GROUND_LAYER = LayerMask.NameToLayer("Ground");
        }

        public void Init(
            DriverStatusData driverStatusData,
            DriverVisionData driverVisionData,
            CarStatusData carStatusData,
            CarControlData carControlData,
            PitCarLocation pitLocation,
            int raceLaps)
        {
            _driverStatus = driverStatusData;
            _driverVision = driverVisionData;
            _carStatus = carStatusData;
            _carControl = carControlData;
            _pitLocation = pitLocation;
            _lapsInRace = raceLaps;

            CurrentSpeed = 0f;
            CurrentMaxSpeed = _carControl.MaxSpeed;
            CurrentDriverStamina = _driverStatus.MaxStamina;
            CurrentCarBody = _carStatus.MaxBodyCondition;
            CurrentCarFuel = _carStatus.MaxFuel;
        }

        public void UpdateCar(float timeStep, Vector3 raceForwardPoint)
        {
            if (raceForwardPoint != default)
            {
                _raceTargetPoint = raceForwardPoint;
            }

            UpdateHeight(timeStep);
            UpdateCurrentMaxSpeed(timeStep);

            // Update AI
            if (CurrentStatus != CarStatus.Idle)
            {
                _aiUpdateTimer -= timeStep;
                if (_aiUpdateTimer <= 0f)
                {
                    UpdateVision();
                    UpdateGoalPoint();

                    float driverStaminaRatio = CurrentDriverStamina / _driverStatus.MaxStamina;
                    float ticksUpdateMultiplier = _driverStatus.VisionModifierByStaminaRatio.Evaluate(driverStaminaRatio);
                    float ticksPerSec = _driverVision.VisionUpdatesPerSecond * ticksUpdateMultiplier;
                    _aiUpdateTimer += 1f / ticksPerSec;
                }
            }

            // Update Movement
            UpdateSteering(timeStep);
            UpdateVelocity(timeStep);

            // Update Driver & Car Status
            UpdateDriverVitals(timeStep);
            UpdateCarVitals(timeStep);
        }

        private void UpdateVision()
        {
            Vector3 carPosition = _transform.position;

            // Periphery extends in both directions from the center point
            int fullPeripheralWidth = _driverVision.TotalPeripheralSamples * 2 + 1;

            // Ensure the vision range array is the right size and clear of stale data
            if (_visionRange == null ||
                _visionRange.GetLength(0) != fullPeripheralWidth ||
                _visionRange.GetLength(1) != _driverVision.TotalForwardSamples)
            {
                _visionRange = new VisionResult[fullPeripheralWidth, _driverVision.TotalForwardSamples];
            }
            else
            {
                Array.Clear(_visionRange, 0, _visionRange.Length);
            }

            // Calculate the 'origin' point where the car currently exists
            (int x, int y) originIndex = (_visionRange.GetLength(0) / 2, 0);
            Vector3 originPos = EyePoint.localPosition;

            // Calculate view direction for car
            Vector3 vectorToGoal = _drivingTargetPoint - carPosition;
            Vector3 directionToGoal = vectorToGoal.normalized;
            Quaternion towardGoal = Quaternion.LookRotation(directionToGoal, Vector3.up);
            Vector3 vectorToRaceForward = _raceTargetPoint - carPosition;
            Vector3 directionToRaceForward = vectorToRaceForward.normalized;
            Quaternion towardRaceForward = Quaternion.LookRotation(directionToRaceForward, Vector3.up);
            Quaternion lookRotation = Quaternion.Lerp(
                towardGoal,
                towardRaceForward,
                0.9f);
            EyePoint.rotation = lookRotation;

            // Iterate through vision points
            RaycastHit hit;
            for (int y = 0; y < _visionRange.GetLength(1); y++)
            {
                float percDistForward = y.Remap(0, _visionRange.GetLength(1), 0, 1);
                float sampleRadius = _driverVision.ForwardSampleRadiusByStep.Evaluate(percDistForward);
                float forwardOffset = _driverVision.ForwardSampleOffsetByStep.Evaluate(percDistForward);
                float peripheralOffset = _driverVision.PeripheralSampleOffsetByForwardStep.Evaluate(percDistForward);
                for (int x = 0; x < _visionRange.GetLength(0); x++)
                {
                    float forwardOffsetMultiplier = _driverVision.ForwardSampleOffsetByPeripheralStep.Evaluate(Mathf.Abs(x - originIndex.x) / (float)originIndex.x);
                    Vector3 samplePos = new Vector3(
                        originPos.x + (x - originIndex.x) * peripheralOffset,
                        originPos.y + sampleRadius * _driverVision.SampleStartRadiusMultipler,
                        originPos.z + (y - originIndex.y) * forwardOffset * forwardOffsetMultiplier);
                    samplePos = EyePoint.TransformPoint(samplePos);

                    if (Physics.SphereCast(samplePos, sampleRadius, Vector3.down, out hit, sampleRadius * _driverVision.SampleLengthRadiusMultiplier))
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
                if (result.ResultType == VisionResult.Result.Track)
                    return true;
                if (CurrentGoal == CarGoal.GoToPit &&
                    result.ResultType == VisionResult.Result.Goal &&
                    Lap < _lapsInRace - 1)
                    return true;
                return false;
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

            _drivingTargetPoint = _transform.position + _transform.forward;

            for (int x = 0; x < _visionRange.GetLength(0); x++)
            {
                int maxPassable = MaxPassableFromColumn(_visionRange, x);
                if (maxPassable > 0)
                {
                    Vector3 drivingPoint = _visionRange[x, maxPassable - 1].SamplePos;

                    if (Vector3.Distance(_transform.position, _drivingTargetPoint) <
                        Vector3.Distance(_transform.position, drivingPoint))
                    {
                        _drivingTargetPoint = new Vector3(drivingPoint.x, _transform.position.y, drivingPoint.z);
                    }
                }
            }

            // If we're trying to get to the pit, target it
            if (CurrentGoal == CarGoal.GoToPit &&
                Vector3.Distance(_transform.position, _pitLocation.transform.position) < 20f)
            {
                _drivingTargetPoint = _pitLocation.transform.position;
            }
        }

        private void UpdateSteering(float timeStep)
        {
            Vector3 vectorToGoal = _drivingTargetPoint - _transform.position;
            Vector3 directionToGoal = vectorToGoal.normalized;

            Quaternion towardGoal = Quaternion.LookRotation(directionToGoal, Vector3.up);
            Quaternion carRotation = Quaternion.RotateTowards(_transform.rotation, towardGoal, _carControl.MaxTurnSpeed * timeStep);

            CurrentTurnSpeed = Quaternion.Angle(_transform.rotation, carRotation);

            _transform.rotation = carRotation;
        }

        private void UpdateVelocity(float timeStep)
        {
            Vector3 position = _transform.position;

            float accelerationMultiplier = 1f;
            float speedAsPercOfMaxSpeed = CurrentSpeed / _carControl.MaxTurnSpeed;
            float speedGoal = CurrentSpeed;

            if (CurrentStatus == CarStatus.Idle)
            { 
                speedGoal = Mathf.Lerp(speedGoal, 0f, timeStep);
            }
            else if (CurrentGoal == CarGoal.PitStop)
            {
                speedGoal = 0f;
                accelerationMultiplier = 5f;
            }
            else
            {
                // Gauge distance to goal point
                Vector3 maxPredictPoint = _visionRange[_driverVision.TotalPeripheralSamples, _driverVision.TotalForwardSamples - 1].SamplePos;
                maxPredictPoint.y = position.y;
                float distanceToGoal = Vector3.Distance(position, _drivingTargetPoint);
                float distanceToMaxPredict = Vector3.Distance(position, maxPredictPoint);
                float percPredictDistance = distanceToGoal / distanceToMaxPredict;
                float speedRatioForGoalDistance = _carControl.TargetSpeedRatioByPredictDistanceRatio.Evaluate(percPredictDistance);

                // Gauge turning radius
                Vector3 vectorToGoal = _drivingTargetPoint - position;
                Vector3 directionToGoal = vectorToGoal.normalized;
                Quaternion towardGoal = Quaternion.LookRotation(directionToGoal, Vector3.up);
                float turnAngle = Quaternion.Angle(_transform.rotation, towardGoal);
                float percTurnAngle = turnAngle / _carControl.MaxTurnSpeed;
                float speedRatioForTurnAngle = _carControl.TargetSpeedRatioByTurnRatio.Evaluate(percTurnAngle);

                float speedRatioGoal = Mathf.Min(speedRatioForGoalDistance, speedRatioForTurnAngle);
                speedGoal = speedRatioGoal * MaxSpeed;
            }

            // Accelerate/Decelerate towards goal
            if (speedGoal > CurrentSpeed)
            {
                float maxAccelAtSpeed = _carControl.MaxAccelerationBySpeed.Evaluate(speedAsPercOfMaxSpeed);
                CurrentSpeed += maxAccelAtSpeed * timeStep * accelerationMultiplier;
            }
            else
            {
                float maxDecelAtSpeed = _carControl.MaxDecelerationBySpeed.Evaluate(speedAsPercOfMaxSpeed);
                CurrentSpeed -= maxDecelAtSpeed * timeStep * accelerationMultiplier;
            }

            CurrentSpeed = Mathf.Clamp(CurrentSpeed, 0f, CurrentMaxSpeed);
            _transform.position += _transform.forward * CurrentSpeed * timeStep;
        }

        private void UpdateHeight(float timeStep)
        {
            Vector3 position = _transform.localPosition;
            position.y = 0f;
            _transform.localPosition = position;
        }

        private void UpdateCurrentMaxSpeed(float timeStep)
        {
            float damagePercent = CurrentCarBody / MaxBody;
            float fuelPercent = CurrentCarFuel / MaxFuel;

            float damageMultiplier = _carStatus.MaxSpeedMultiplierByBodyPercentage.Evaluate(damagePercent);
            float fuelMultiplier = _carStatus.MaxSpeedMultiplierByFuelPercentage.Evaluate(fuelPercent);

            CurrentMaxSpeed = Mathf.Lerp(CurrentMaxSpeed, MaxSpeed * damageMultiplier * fuelMultiplier, timeStep);
        }

        private void UpdateDriverVitals(float timeStep)
        {
            float staminaLoss = _driverStatus.StaminaLossBySpeedRatio.Evaluate(CurrentSpeed / MaxSpeed);

            CurrentDriverStamina = Mathf.Max(0f, CurrentDriverStamina - staminaLoss * timeStep);
        }

        private void UpdateCarVitals(float timeStep)
        {
            float fuelLossFromSpeed = _carStatus.FuelConsumptionBySpeedRatio.Evaluate(CurrentSpeed / MaxSpeed);
            float bodyLossFromSpeed = _carStatus.BodyDamageBySpeedRatio.Evaluate(CurrentSpeed / MaxSpeed);
            float bodyLossFromTurn = _carStatus.BodyDamageByTurnRatio.Evaluate(CurrentTurnSpeed / _carControl.MaxTurnSpeed);
            float totalBodyLoss = bodyLossFromSpeed + bodyLossFromTurn;

            CurrentCarFuel = Mathf.Max(0f, CurrentCarFuel - fuelLossFromSpeed * timeStep);
            CurrentCarBody = Mathf.Max(0f, CurrentCarBody - totalBodyLoss * timeStep);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody != null &&
                collision.rigidbody.GetComponent<CarComponent>() is { } cc)
            {
                Debug.LogWarning($"Car \"{name}\" hit \"{cc.name}\" at a velocity of {collision.relativeVelocity.magnitude}!");

                float collisionMagnitude = collision.relativeVelocity.magnitude;
                float staminaLoss = _driverStatus.StaminaLossByHitVelocity.Evaluate(collisionMagnitude);
                float bodyLoss = _carStatus.BodyDamageByHitVelocity.Evaluate(collisionMagnitude);

                CurrentDriverStamina = Mathf.Max(0f, CurrentDriverStamina - staminaLoss);
                CurrentCarBody = Mathf.Max(0f, CurrentDriverStamina - bodyLoss);
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (collider.GetComponent<TrackInteractionPoint>() is { } ip)
            {
                Debug.Log($"Car \"{name}\" entered InteractionPoint type {ip.Type}");
                switch (ip.Type)
                {
                    case TrackInteractionPointType.FinishLine:
                        Lap++;
                        break;
                    case TrackInteractionPointType.PitEntrance:
                        break;
                    case TrackInteractionPointType.PitExit:
                        break;
                    case TrackInteractionPointType.HACK_TEMP_PITSTOP:
                        CurrentDriverStamina = MaxStamina;
                        CurrentCarBody = MaxBody;
                        CurrentCarFuel = MaxFuel;
                        break;
                }
            }
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
                        Vector3.Lerp(visionResult.SamplePos, visionResult.SamplePos + Vector3.down * visionResult.SampleRadius * _driverVision.SampleLengthRadiusMultiplier, 0.5f),
                        visionResult.SampleRadius);
                }
            }

            // Draw Drive Target
            Gizmos.color = Color.Lerp(Color.white, Color.clear, 0.5f);
            Gizmos.DrawLine(transform.position, _drivingTargetPoint);
            if (CarCollider is MeshCollider asMeshCollider)
            {
                Gizmos.DrawWireMesh(asMeshCollider.sharedMesh, _drivingTargetPoint, CarCollider.transform.rotation, transform.localScale);
            }
            else
            {
                Gizmos.DrawWireCube(_drivingTargetPoint, transform.localScale);
            }

            // Draw Race Forward
            Gizmos.color = Color.Lerp(Color.magenta, Color.clear, 0.5f);
            Gizmos.DrawLine(transform.position, _raceTargetPoint);
        }
    }
}