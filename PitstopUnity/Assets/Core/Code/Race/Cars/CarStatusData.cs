using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race.Cars
{
    [CreateAssetMenu]
    public class CarStatusData : ScriptableObject
    {
        public float MaxBodyCondition;
        public AnimationCurve BodyDamageBySpeedRatio;
        public AnimationCurve BodyDamageByTurnRatio;
        public AnimationCurve BodyDamageByHitVelocity;
        public AnimationCurve MaxSpeedMultiplierByBodyPercentage;
        
        public float MaxFuel;
        public AnimationCurve FuelConsumptionBySpeedRatio;
        public AnimationCurve MaxSpeedMultiplierByFuelPercentage;
    }
}