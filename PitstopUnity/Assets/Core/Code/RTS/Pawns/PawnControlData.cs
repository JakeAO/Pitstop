using UnityEngine;

namespace SadPumpkin.Game.Pitstop
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
    }
}