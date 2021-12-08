using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race.Drivers
{
    [CreateAssetMenu]
    public class DriverVisionData : ScriptableObject
    {
        public Vector3 VisionOffset = Vector3.zero;
        [Range(0.1f, 1f)]
        public float GoalRedirectStrength = 0.5f;
        [Range(10f, 60f)]
        public float VisionUpdatesPerSecond = 30f;
        [Range(0.5f, 5f)]
        public float SampleStartRadiusMultipler = 2f;
        [Range(1f, 8f)]
        public float SampleLengthRadiusMultiplier = 4f;
        [Range(2f, 30f)]
        public int TotalForwardSamples = 5;
        [Range(0f, 5f)]
        public int TotalPeripheralSamples = 2;
        public AnimationCurve ForwardSampleRadiusByStep = AnimationCurve.Linear(0f, 1f, 1f, 1.5f);
        public AnimationCurve ForwardSampleOffsetByStep = AnimationCurve.Linear(0f, 1.5f, 1f, 1.5f);
        public AnimationCurve ForwardSampleOffsetByPeripheralStep = AnimationCurve.Linear(0f, 1.1f, 1f, 1f);
        public AnimationCurve PeripheralSampleOffsetByForwardStep = AnimationCurve.Linear(0f, 1.5f, 1f, 3f);
    }
}