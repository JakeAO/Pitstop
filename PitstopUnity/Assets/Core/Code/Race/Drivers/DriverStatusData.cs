using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race
{
    [CreateAssetMenu]
    public class DriverStatusData : ScriptableObject
    {
        public float MaxStamina;
        public AnimationCurve StaminaLossBySpeedRatio;
        public AnimationCurve StaminaLossByHitVelocity;
    }
}