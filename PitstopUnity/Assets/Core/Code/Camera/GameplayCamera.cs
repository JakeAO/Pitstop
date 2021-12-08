using SadPumpkin.Game.Pitstop.Core.Code.Util;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class GameplayCamera : MonoBehaviour
    {
        [ReadOnly] public UnityEngine.Camera Camera;
        [ReadOnly] public float ZoomPercent = 0f;

        public float MoveSpeed = 75f;
        public float ScrollSpeed = 7.5f;
        public Vector3 MinimumPosition;
        public Vector3 MaximumPosition;
        public AnimationCurve AngleByZoomPercent;
        public AnimationCurve HeightByZoomPercent;

        private void Awake()
        {
            Camera = GetComponent<UnityEngine.Camera>();
        }

        public void UpdateCamera(float timeStep)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            Vector3 newPos = transform.position;
            Vector3 targetRot = transform.eulerAngles;

            // Scrolling (Y Position)
            ZoomPercent = Mathf.Clamp01(ZoomPercent + ScrollSpeed * Input.mouseScrollDelta.y * timeStep);
            float newHeightPercentByZoom = HeightByZoomPercent.Evaluate(ZoomPercent);
            float newAngleByZoom = AngleByZoomPercent.Evaluate(ZoomPercent);
            float yGoal = newHeightPercentByZoom.Remap(0f, 1f, MinimumPosition.y, MaximumPosition.y);
            newPos.y = Mathf.Lerp(newPos.y, yGoal, MoveSpeed * timeStep);
            targetRot.x = newAngleByZoom;
            
            // WASD (XZ Position)
            Vector2 moveDir = Vector2.zero;
            if (Input.GetKey(KeyCode.W))
            {
                moveDir.y = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                moveDir.y = -1;
            }

            if (Input.GetKey(KeyCode.A))
            {
                moveDir.x = -1;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                moveDir.x = 1;
            }

            moveDir.Normalize();
            newPos.x += moveDir.x * MoveSpeed * timeStep;
            newPos.z += moveDir.y * MoveSpeed * timeStep;

            // Clamp and Apply Position
            newPos.x = Mathf.Clamp(newPos.x, MinimumPosition.x, MaximumPosition.x);
            newPos.y = Mathf.Clamp(newPos.y, MinimumPosition.y, MaximumPosition.y);
            newPos.z = Mathf.Clamp(newPos.z, MinimumPosition.z, MaximumPosition.z);

            transform.position = newPos;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetRot), MoveSpeed * timeStep);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                new Vector3(
                    Mathf.Lerp(MinimumPosition.x, MaximumPosition.x, 0.5f),
                    Mathf.Lerp(MinimumPosition.y, MaximumPosition.y, 0.5f),
                    Mathf.Lerp(MinimumPosition.z, MaximumPosition.z, 0.5f)),
                new Vector3(
                    MaximumPosition.x - MinimumPosition.x,
                    MaximumPosition.y - MinimumPosition.y,
                    MaximumPosition.z - MinimumPosition.z));
        }
    }
}