using System;
using System.Collections.Generic;

namespace SadPumpkin.Game.Pitstop.Core.Code.Util
{
    public readonly struct RangeFloat
    {
        public static readonly IComparer<RangeFloat> SIZE_COMPARER = new CompareBySize();

        public readonly float From;
        public readonly float To;

        public RangeFloat(float from, float to)
        {
            From = from;
            To = to;
        }

        public static RangeFloat CreateFrom(float from) => new RangeFloat(from, float.MaxValue);
        public static RangeFloat CreateTo(float to) => new RangeFloat(float.MinValue, to);
        public static RangeFloat CreateFromTo(float from, float to) => new RangeFloat(from, to);

        public static RangeFloat Extend(RangeFloat current, float newBound) => new RangeFloat(
            Math.Min(current.From, newBound),
            Math.Max(current.To, newBound));

        public static IEnumerable<RangeFloat> Cutout(RangeFloat target, RangeFloat cut)
        {
            if (target.Overlaps(cut))
            {
                if (target.InRange(cut.From) && target.InRange(cut.To))
                {
                    yield return new RangeFloat(target.From, cut.From);
                    yield return new RangeFloat(cut.To, target.To);
                }
                else if (target.From > cut.To)
                {
                    yield return new RangeFloat(cut.To, target.To);
                }
                else
                {
                    yield return new RangeFloat(target.From, cut.From);
                }
            }
            else
            {
                yield return target;
            }
        }

        public bool InRange(float check) => From < check && check < To;
        public bool Overlaps(RangeFloat other) => InRange(other.From) || InRange(other.To);
        public bool IsZero() => Math.Abs(From - To) < 0.001f;

        public float MidPoint() => (From + To) * 0.5f;
        public float Size() => Math.Abs(To - From);

        private class CompareBySize : IComparer<RangeFloat>
        {
            public int Compare(RangeFloat x, RangeFloat y)
            {
                return y.Size().CompareTo(x.Size());
            }
        }
    }
}