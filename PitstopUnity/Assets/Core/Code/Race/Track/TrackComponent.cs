using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public class TrackComponent : MonoBehaviour
    {
        public Transform TrackGeometryRoot;
        public Collider FinishLine;
        public Transform[] PolePositions;
        public Transform[] PitPositions;
        public Collider PitEntrance;
        public Collider PitExit;
    }
}
