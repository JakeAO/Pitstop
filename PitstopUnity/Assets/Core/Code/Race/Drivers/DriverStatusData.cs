using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race.Drivers
{
    [CreateAssetMenu]
    public class DriverStatusData : ScriptableObject
    {
        public float MaxStamina;
        public AnimationCurve StaminaLossBySpeedRatio;
        public AnimationCurve StaminaLossByHitVelocity;
        public AnimationCurve VisionModifierByStaminaRatio;
        public float StaminaRecovery;
    }
}