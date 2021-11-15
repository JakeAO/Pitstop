using Sirenix.OdinInspector;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public class PawnController : MonoBehaviour
    {
        [ReadOnly]
        public PawnComponent[] PawnInstances;

        public void UpdatePawns(float timeStep)
        {
            foreach (PawnComponent pawnInstance in PawnInstances)
            {
                pawnInstance.UpdatePawn(timeStep);
            }
        }
    }
}