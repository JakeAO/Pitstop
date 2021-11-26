using SadPumpkin.Game.Pitstop.Core.Code.RTS;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public class TrackComponent : MonoBehaviour
    {
        public Transform TrackGeometryRoot;
        public Collider FinishLine;
        public Transform[] PolePositions;
        public PitCarLocation[] PitPositions;
        public Collider PitEntrance;
    }
}
