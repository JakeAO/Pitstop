using SadPumpkin.Game.Pitstop.Core.Code.Race.Cars;
using SadPumpkin.Game.Pitstop.Core.Code.Util;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Camera
{
    public class ChaseCamera : MonoBehaviour
    {
        public float FollowDistance = 10f;
        public float FollowHeight = 5f;
        public float MovementSpeed = 20f;
        public float MovementSmooth = 0.1f;
        public float RotationSmooth = 0.1f;

        private Transform _transform;
        private CarComponent _targetCar;

        private Vector3 _currentVelocity;
        private Quaternion _currentAngularVelocity;

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
            _transform.position = Vector3.SmoothDamp(_transform.position, goalPos, ref _currentVelocity, MovementSmooth, MovementSpeed);

            Quaternion cameraToCar = Quaternion.LookRotation(_targetCar.transform.position - _transform.position, Vector3.up);
            Quaternion carDriveForward = Quaternion.LookRotation(_targetCar.DriveForward, Vector3.up);
            Quaternion goalRot = Quaternion.Lerp(cameraToCar, carDriveForward, 0.5f);
            _transform.rotation = QuaternionUtils.SmoothDamp(_transform.rotation, goalRot, ref _currentAngularVelocity, RotationSmooth);
        }
    }
}