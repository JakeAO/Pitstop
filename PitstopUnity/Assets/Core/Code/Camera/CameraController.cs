using SadPumpkin.Game.Pitstop.Core.Code.Util;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Camera
{
    public class CameraController : MonoBehaviour
    {
        public enum CameraMode
        {
            Invalid = 0,
            Gameplay = 1,
            Chase = 2,
        }

        [ReadOnly] public CameraMode Mode;
        public ChaseCamera ChaseCamera;
        public GameplayCamera GameplayCamera;

        public void Initialize()
        {
            
        }
        
        public void UpdateCameras(float timeStep)
        {
            if (ChaseCamera)
                ChaseCamera.UpdateCamera(timeStep);
            if (GameplayCamera)
                GameplayCamera.UpdateCamera(timeStep);
        }

        public void SwitchCameraMode(CameraMode mode)
        {
            Mode = mode;
            switch (mode)
            {
                case CameraMode.Gameplay:
                    ChaseCamera.UpdateActive(false);
                    GameplayCamera.UpdateActive(true);
                    break;
                case CameraMode.Chase:
                    ChaseCamera.UpdateActive(true);
                    GameplayCamera.UpdateActive(false);
                    break;
                default:
                    ChaseCamera.UpdateActive(false);
                    GameplayCamera.UpdateActive(false);
                    break;
            }
        }
    }
}