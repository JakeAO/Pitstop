using System.Collections.Generic;
using SadPumpkin.Game.Pitstop.Core.Code;
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
        public Image SpeedIndicatorImage;
        public TMP_Text CarPositionLabel;

        public Image BodyIndicatorImage;
        public TMP_Text BodyIndicatorLabel;
        
        public Image FuelIndicatorImage;
        public TMP_Text FuelIndicatorLabel;
        
        public Image StaminaIndicatorImage;
        public TMP_Text StaminaIndicatorLabel;

        public void UpdateStatus(RaceController raceController)
        {
            MetalStockLabel.text = "0";
            OilStockLabel.text = "0";
            PawnStockLabel.text = "0/0";

            MinimapCamera.targetTexture = MinimapRenderTexture;

            raceController.CameraController.ChaseCamera.gameObject.UpdateActive(FollowCarToggle.isOn);
            
            CarComponent playerCar = raceController.CarController.CarInstances[0];
            SpeedIndicatorImage.fillAmount = playerCar.CurrentSpeed / playerCar.MaxSpeed;

            int playerCarPosition = 0;
            IReadOnlyList<CarComponent> positions = raceController.GetRacePositions();
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
            BodyIndicatorLabel.text = bodyPercent.ToString("P0");

            float fuelPercent = playerCar.CurrentCarFuel / playerCar.MaxFuel;
            FuelIndicatorImage.fillAmount = fuelPercent;
            FuelIndicatorLabel.text = fuelPercent.ToString("P0");

            float staminaPercent = playerCar.CurrentDriverStamina / playerCar.MaxStamina;
            StaminaIndicatorImage.fillAmount = staminaPercent;
            StaminaIndicatorLabel.text = staminaPercent.ToString("P0");
        }
    }
}