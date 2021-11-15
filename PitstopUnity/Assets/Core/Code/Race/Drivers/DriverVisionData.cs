using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race
{
    [CreateAssetMenu]
    public class DriverVisionData : ScriptableObject
    {
        public float VisionUpdatesPerSecond = 30f;
        public float SampleStartRadiusMultipler = 2f;
        public float SampleLengthRadiusMultiplier = 4f;
        public int TotalForwardSamples = 5;
        public int TotalPeripheralSamples = 2;
        public AnimationCurve ForwardSampleRadiusByStep = AnimationCurve.Linear(0f, 1f, 1f, 1.5f);
        public AnimationCurve ForwardSampleOffsetByStep = AnimationCurve.Linear(0f, 1.5f, 1f, 1.5f);
        public AnimationCurve PeripheralSampleOffsetByForwardStep = AnimationCurve.Linear(0f, 1.5f, 1f, 3f);
    }
}