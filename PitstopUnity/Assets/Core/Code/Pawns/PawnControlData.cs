using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    [CreateAssetMenu]
    public class PawnControlData : ScriptableObject
    {
        public float MoveSpeed;
        public float Acceleration;
        public float TurnSpeed;
        public float CarryCapacity;
    }
}