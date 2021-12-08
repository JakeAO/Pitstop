using System.Collections.Generic;
using SadPumpkin.Game.Pitstop.Core.Code.Camera;
using SadPumpkin.Game.Pitstop.Core.Code.Race;
using SadPumpkin.Game.Pitstop.Core.Code.Race.Cars;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Pawns;
using SadPumpkin.Game.Pitstop.Core.Code.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SadPumpkin.Game.Pitstop.Core.Code.UI.Gameplay
{
    public class RaceHUD : MonoBehaviour
    {
        public string CarLapTextFormat = "{0}<size=22>/{1}</size>";
        
        public TMP_Text MetalStockLabel;
        public TMP_Text OilStockLabel;
        public TMP_Text PawnStockLabel;

        public RenderTexture MinimapRenderTexture;
        public UnityEngine.Camera MinimapCamera;

        public Toggle FollowCarToggle;
        public Toggle PitstopToggle;
        public Image SpeedLimiterFill;
        public Image SpeedIndicator;
        public TMP_Text CarPositionLabel;
        public TMP_Text CarLapLabel;

        public Image BodyIndicatorImage;
        public TMP_Text BodyIndicatorLabel;

        public Image FuelIndicatorImage;
        public TMP_Text FuelIndicatorLabel;

        public Image StaminaIndicatorImage;
        public TMP_Text StaminaIndicatorLabel;

        public Gradient StatusColorGradient;

        private TeamData _localTeam = null;
        private CarComponent _carInstance = null;
        private TeamPawnController _teamController = null;
        private RaceController _raceController = null;
        private CameraController _cameraController = null;
        
        public void Initialize(CarComponent carInstance, RaceController raceController, TeamPawnController teamController, CameraController cameraController)
        {
            _carInstance = carInstance;
            _localTeam = teamController.Team;
            _teamController = teamController;
            _raceController = raceController;
            _cameraController = cameraController;

            FollowCarToggle.isOn = cameraController.Mode == CameraController.CameraMode.Chase;
        }
        
        private void OnEnable()
        {
            FollowCarToggle.onValueChanged.AddListener(OnFollowToggled);
            PitstopToggle.onValueChanged.AddListener(OnPitstopToggled);
        }

        private void OnDisable()
        {
            FollowCarToggle.onValueChanged.RemoveListener(OnFollowToggled);
            PitstopToggle.onValueChanged.RemoveListener(OnPitstopToggled);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                FollowCarToggle.isOn = !FollowCarToggle.isOn;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                PitstopToggle.isOn = !PitstopToggle.isOn;
            }
        }

        private void OnFollowToggled(bool isOn)
        {
            _cameraController.SwitchCameraMode(isOn
                ? CameraController.CameraMode.Chase
                : CameraController.CameraMode.Gameplay);
        }

        private void OnPitstopToggled(bool isOn)
        {
            if (isOn)
            {
                if (_carInstance.CurrentGoal == CarGoal.Race)
                {
                    _carInstance.CurrentGoal = CarGoal.GoToPit;
                }
            }
            else
            {
                _carInstance.CurrentGoal = CarGoal.Race;
            }
        }

        public void UpdateStatus()
        {
            switch (_carInstance.CurrentGoal)
            {
                case CarGoal.Race:
                    PitstopToggle.isOn = false;
                    break;
                case CarGoal.GoToPit:
                case CarGoal.PitStop:
                    PitstopToggle.isOn = true;
                    break;
            }

            MetalStockLabel.text = Mathf.RoundToInt(_teamController.CurrentMetal).ToString();
            OilStockLabel.text = Mathf.RoundToInt(_teamController.CurrentOil).ToString();
            PawnStockLabel.text = $"{_teamController.NumPawnsAtWork}/{_teamController.NumPawnsAtHome}";

            MinimapCamera.targetTexture = MinimapRenderTexture;

            _cameraController.ChaseCamera.gameObject.UpdateActive(FollowCarToggle.isOn);

            float currentSpeedPercent = _carInstance.CurrentSpeed / _carInstance.MaxSpeed;
            float currentLimitedPercent = 1f - _carInstance.CurrentMaxSpeed / _carInstance.MaxSpeed;
            SpeedIndicator.rectTransform.localEulerAngles = Vector3.forward * currentSpeedPercent.Remap(0f, 1f, 90f, -90f);
            SpeedLimiterFill.fillAmount = currentLimitedPercent;
            
            int playerCarPosition = 0;
            IReadOnlyList<CarComponent> positions = _raceController.GetRacePositions();
            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i] == _carInstance)
                {
                    playerCarPosition = i + 1;
                    break;
                }
            }

            CarPositionLabel.text = $"#{playerCarPosition}";
            CarLapLabel.text = string.Format(CarLapTextFormat, Mathf.Clamp(_carInstance.Lap, 0, _raceController.RaceLaps), _raceController.RaceLaps);
            
            float bodyPercent = _carInstance.CurrentCarBody / _carInstance.MaxBody;
            BodyIndicatorImage.fillAmount = bodyPercent;
            BodyIndicatorImage.color = StatusColorGradient.Evaluate(bodyPercent);
            BodyIndicatorLabel.text = bodyPercent.ToString("P0");

            float fuelPercent = _carInstance.CurrentCarFuel / _carInstance.MaxFuel;
            FuelIndicatorImage.fillAmount = fuelPercent;
            FuelIndicatorImage.color = StatusColorGradient.Evaluate(fuelPercent);
            FuelIndicatorLabel.text = fuelPercent.ToString("P0");

            float staminaPercent = _carInstance.CurrentDriverStamina / _carInstance.MaxStamina;
            StaminaIndicatorImage.fillAmount = staminaPercent;
            StaminaIndicatorImage.color = StatusColorGradient.Evaluate(staminaPercent);
            StaminaIndicatorLabel.text = staminaPercent.ToString("P0");
        }
    }
}