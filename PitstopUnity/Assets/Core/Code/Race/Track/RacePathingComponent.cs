using Dreamteck.Splines;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public class RacePathingComponent : MonoBehaviour
    {
        public RacePathingType Type;
        public SplineComputer Spline;
        public float TestRange = 500f;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, TestRange);
        }
    }
}