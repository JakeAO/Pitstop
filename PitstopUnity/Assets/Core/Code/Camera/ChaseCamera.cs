using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public class ChaseCamera : MonoBehaviour
    {
        [ReadOnly] public CarComponent CurrentTarget;
        public Vector3 Offset = new Vector3(0f, 20f, -15f);
        public float MovementSpeed = 20f;
        public bool ShowGuiControls = true;
        
        private CarController _carController;

        public void Init(CarController carController)
        {
            _carController = carController;

            CurrentTarget = _carController.CarInstances.First();

            transform.position = CurrentTarget.transform.position + Offset;
            transform.LookAt(CurrentTarget.transform.position);
        }

        public void UpdateCamera(float timeStep)
        {
            Vector3 targetPos = CurrentTarget.transform.position;
            Vector3 cameraGoalPos = targetPos + Offset;

            transform.position = Vector3.MoveTowards(transform.position, cameraGoalPos, MovementSpeed * timeStep);
            transform.LookAt(targetPos);
        }

        public void OnGUI()
        {
            if (ShowGuiControls)
            {
                Rect windowRect = new Rect(0, 0, 100f, 50f);
                Rect prevButtonRect = new Rect(windowRect.x, windowRect.y, 50f, windowRect.height);
                Rect nextButtonRect = new Rect(windowRect.x + 50f, windowRect.y, 50f, windowRect.height);

                // TEMP REPLACE
                GUI.Box(windowRect, string.Empty);
                if (GUI.Button(prevButtonRect, "<-"))
                {
                    int currIndex = Array.IndexOf(_carController.CarInstances, CurrentTarget);
                    int prevIndex = currIndex == 0 ? _carController.CarInstances.Length - 1 : currIndex - 1;
                    CurrentTarget = _carController.CarInstances[prevIndex];
                }

                if (GUI.Button(nextButtonRect, "->"))
                {
                    int currIndex = Array.IndexOf(_carController.CarInstances, CurrentTarget);
                    int nextIndex = (currIndex + 1) % _carController.CarInstances.Length;
                    CurrentTarget = _carController.CarInstances[nextIndex];
                }
            }
        }
    }
}