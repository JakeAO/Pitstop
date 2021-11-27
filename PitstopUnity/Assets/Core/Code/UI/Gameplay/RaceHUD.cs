using System.Collections.Generic;
using DG.Tweening;
using SadPumpkin.Game.Pitstop.Core.Code.Race;
using SadPumpkin.Game.Pitstop.Core.Code.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SadPumpkin.Game.Pitstop
{
    public class RaceHUD : MonoBehaviour
    {
        public TMP_Text MetalStockLabel;
        public TMP_Text OilStockLabel;
        public TMP_Text PawnStockLabel;

        public RenderTexture MinimapRenderTexture;
        public Camera MinimapCamera;

        public Toggle FollowCarToggle;
        public Image SpeedLimiterFill;
        public Image SpeedIndicator;
        public TMP_Text CarPositionLabel;

        public Image BodyIndicatorImage;
        public TMP_Text BodyIndicatorLabel;

        public Image FuelIndicatorImage;
        public TMP_Text FuelIndicatorLabel;

        public Image StaminaIndicatorImage;
        public TMP_Text StaminaIndicatorLabel;

        public Gradient StatusColorGradient;
        
        private TeamPawnController _teamController;
        private RaceController _raceController = null;
        private CameraController _cameraController = null;
        
        public void Initialize(RaceController raceController, TeamPawnController teamController, CameraController cameraController)
        {
            _teamController = teamController;
            _raceController = raceController;
            _cameraController = cameraController;

            FollowCarToggle.isOn = cameraController.Mode == CameraController.CameraMode.Chase;
        }
        
        private void OnEnable()
        {
            FollowCarToggle.onValueChanged.AddListener(OnFollowToggled);
        }

        private void OnDisable()
        {
            FollowCarToggle.onValueChanged.RemoveListener(OnFollowToggled);
        }

        private void OnFollowToggled(bool isOn)
        {
            _cameraController.SwitchCameraMode(isOn
                ? CameraController.CameraMode.Chase
                : CameraController.CameraMode.Gameplay);
        }

        public void UpdateStatus()
        {
            MetalStockLabel.text = Mathf.RoundToInt(_teamController.CurrentMetal).ToString();
            OilStockLabel.text = Mathf.RoundToInt(_teamController.CurrentOil).ToString();
            PawnStockLabel.text = $"{_teamController.NumPawnsAtWork}/{_teamController.NumPawnsAtHome}";

            MinimapCamera.targetTexture = MinimapRenderTexture;

            _cameraController.ChaseCamera.gameObject.UpdateActive(FollowCarToggle.isOn);

            CarComponent playerCar = _raceController.CarController.CarInstances[0];
            float currentSpeedPercent = playerCar.CurrentSpeed / playerCar.MaxSpeed;
            float currentLimitedPercent = 1f - playerCar.CurrentMaxSpeed / playerCar.MaxSpeed;
            SpeedIndicator.rectTransform.localEulerAngles = Vector3.forward * currentSpeedPercent.Remap(0f, 1f, 90f, -90f);
            SpeedLimiterFill.fillAmount = currentLimitedPercent;
            
            int playerCarPosition = 0;
            IReadOnlyList<CarComponent> positions = _raceController.GetRacePositions();
            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i] == playerCar)
                {
                    playerCarPosition = i + 1;
                    break;
                }
            }

            CarPositionLabel.text = $"#{playerCarPosition}";

            float bodyPercent = playerCar.CurrentCarBody / playerCar.MaxBody;
            BodyIndicatorImage.fillAmount = bodyPercent;
            BodyIndicatorImage.color = StatusColorGradient.Evaluate(bodyPercent);
            BodyIndicatorLabel.text = bodyPercent.ToString("P0");

            float fuelPercent = playerCar.CurrentCarFuel / playerCar.MaxFuel;
            FuelIndicatorImage.fillAmount = fuelPercent;
            FuelIndicatorImage.color = StatusColorGradient.Evaluate(fuelPercent);
            FuelIndicatorLabel.text = fuelPercent.ToString("P0");

            float staminaPercent = playerCar.CurrentDriverStamina / playerCar.MaxStamina;
            StaminaIndicatorImage.fillAmount = staminaPercent;
            StaminaIndicatorImage.color = StatusColorGradient.Evaluate(staminaPercent);
            StaminaIndicatorLabel.text = staminaPercent.ToString("P0");
        }
    }
}