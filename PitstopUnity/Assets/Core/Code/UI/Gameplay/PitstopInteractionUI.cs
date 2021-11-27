using SadPumpkin.Game.Pitstop.Core.Code;
using SadPumpkin.Game.Pitstop.Core.Code.RTS;
using UnityEngine;
using UnityEngine.UI;

namespace SadPumpkin.Game.Pitstop
{
    public class PitstopInteractionUI : MonoBehaviour
    {
        public PitCrewLocation CrewLocation;

        public Toggle PitstopToggle;

        private TeamData _teamData;
        private GameplayCamera _gameplayCamera;

        public void Initialize(TeamData teamData)
        {
            _teamData = teamData;
        }

        private void OnEnable()
        {
            if (!_gameplayCamera)
            {
                _gameplayCamera = FindObjectOfType<GameplayCamera>();
            }

            PitstopToggle.isOn = _teamData.CarInstance.CurrentGoal == CarGoal.GoToPit || _teamData.CarInstance.CurrentGoal == CarGoal.PitStop;
            PitstopToggle.onValueChanged.AddListener(OnStopToggled);
        }

        private void OnDisable()
        {
            PitstopToggle.onValueChanged.RemoveListener(OnStopToggled);
        }

        private void Update()
        {
            PitstopToggle.isOn = _teamData.CarInstance.CurrentGoal == CarGoal.GoToPit;
            
            transform.rotation = _gameplayCamera.transform.rotation;

            Vector3 viewportPosition = _gameplayCamera.Camera.WorldToViewportPoint(transform.position);
            if (viewportPosition.x < 0f || viewportPosition.x > 1f ||
                viewportPosition.y < 0f || viewportPosition.y > 1f)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnStopToggled(bool isOn)
        {
            _teamData.CarInstance.CurrentGoal = isOn
                ? CarGoal.GoToPit
                : CarGoal.Race;
        }
    }
}