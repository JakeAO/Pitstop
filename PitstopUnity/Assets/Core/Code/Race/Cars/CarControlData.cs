using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race.Cars
{
    [CreateAssetMenu]
    public class CarControlData : ScriptableObject
    {
        public float MaxTurnSpeed = 3f;
        public float MaxSpeed = 30f;
        public AnimationCurve MaxAccelerationBySpeed = AnimationCurve.EaseInOut(0f, 10f, 1f, 0f);
        public AnimationCurve MaxDecelerationBySpeed = AnimationCurve.EaseInOut(0f, 30f, 1f, 10f);
        public AnimationCurve TargetSpeedRatioByPredictDistanceRatio = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve TargetSpeedRatioByTurnRatio = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }
}