using SadPumpkin.Game.Pitstop.Core.Code.Util;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public class ChaseCamera : MonoBehaviour
    {
        public float FollowDistance = 10f;
        public float FollowHeight = 5f;
        public float MovementSpeed = 20f;
        public float MovementSmooth = 1f;
        public float RotationSpeed = 30f;

        private Transform _transform;
        private CarComponent _targetCar;

        private Vector3 _currentVelocity;
        
        public void Init(CarComponent targetCar)
        {
            _transform = transform;
            _targetCar = targetCar;

            _transform.position = _targetCar.transform.position + Vector3.up * FollowHeight + _targetCar.CarForward * -FollowDistance;
            _transform.LookAt(_targetCar.transform);
        }

        public void UpdateCamera(float timeStep)
        {
            Vector3 goalPos = _targetCar.transform.position + Vector3.up * FollowHeight + _targetCar.CarForward * -FollowDistance;
            Quaternion carForwardRot = Quaternion.LookRotation(
                _targetCar.DriveForward.IsApproximately(Vector3.zero)
                    ? _targetCar.RaceForward.IsApproximately(Vector3.zero)
                        ? _targetCar.CarForward
                        : _targetCar.RaceForward
                    : _targetCar.DriveForward,
                Vector3.up);
            Quaternion toCarRot = Quaternion.LookRotation(_targetCar.transform.position - _transform.position, Vector3.up);
            Quaternion goalRot = Quaternion.Lerp(carForwardRot, toCarRot, 0.5f);

            _transform.position = Vector3.SmoothDamp(_transform.position, goalPos, ref _currentVelocity, MovementSmooth, MovementSpeed);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, goalRot, RotationSpeed * timeStep);
        }
    }
}