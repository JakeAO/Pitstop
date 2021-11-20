using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public class ChaseCamera : MonoBehaviour
    {
        public Vector3 Offset = new Vector3(0f, 20f, -15f);
        public float MovementSpeed = 20f;

        private CarComponent _targetCar;

        public void Init(CarComponent targetCar)
        {
            _targetCar = targetCar;

            transform.position = _targetCar.transform.position + Offset;
            transform.LookAt(_targetCar.transform.position);
        }

        public void UpdateCamera(float timeStep)
        {
            Vector3 targetPos = _targetCar.transform.position;
            Vector3 cameraGoalPos = targetPos + Offset;

            transform.position = Vector3.MoveTowards(transform.position, cameraGoalPos, MovementSpeed * timeStep);
            transform.LookAt(targetPos);
        }
    }
}