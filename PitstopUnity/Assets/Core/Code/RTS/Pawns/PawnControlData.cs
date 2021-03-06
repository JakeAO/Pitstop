using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.RTS.Pawns
{
    [CreateAssetMenu]
    public class PawnControlData : ScriptableObject
    {
        public AnimationCurve MoveSpeed;
        public AnimationCurve Acceleration;
        public AnimationCurve TurnSpeed;
        public AnimationCurve CarryCapacity;
        public AnimationCurve MetalGatherSpeed;
        public AnimationCurve OilGatherSpeed;
        public AnimationCurve RepairSpeed;
        public AnimationCurve RefuelSpeed;
    }
}