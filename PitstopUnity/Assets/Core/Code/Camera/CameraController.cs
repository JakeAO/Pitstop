using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public class CameraController : MonoBehaviour
    {
        public ChaseCamera ChaseCamera;
        public GameplayCamera GameplayCamera;

        public void UpdateCameras(float timeStep)
        {
            if (ChaseCamera)
                ChaseCamera.UpdateCamera(timeStep);
            if (GameplayCamera)
                GameplayCamera.UpdateCamera(timeStep);
        }
    }
}