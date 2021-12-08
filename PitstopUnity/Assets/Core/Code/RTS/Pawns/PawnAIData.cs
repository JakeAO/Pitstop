using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.RTS.Pawns
{
    [CreateAssetMenu]
    public class PawnAIData : ScriptableObject
    {
        public AnimationCurve CarRepairThreshold;
        public AnimationCurve CarRefuelThreshold;
        public AnimationCurve DriverStaminaThreshold;
        [Space(10f)]
        public AnimationCurve PitCrewWeight;
        public AnimationCurve GatherMetalWeight;
        public AnimationCurve GatherOilWeight;
        [Space(10f)] 
        public AnimationCurve AiEvaluationFrequency;
    }
}