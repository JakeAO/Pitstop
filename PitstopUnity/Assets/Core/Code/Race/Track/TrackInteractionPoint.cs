using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public enum TrackInteractionPointType : int
    {
        FinishLine = 0,
        PitEntrance = 1,
        PitExit = 2,

        HACK_TEMP_PITSTOP = 999,
    }

    public class TrackInteractionPoint : MonoBehaviour
    {
        public TrackInteractionPointType Type;
    }
}
