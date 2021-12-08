using SadPumpkin.Game.Pitstop.Core.Code.RTS.Pit;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race.Track
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
