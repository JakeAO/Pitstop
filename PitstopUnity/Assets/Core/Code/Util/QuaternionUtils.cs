using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Util
{
    public static class QuaternionUtils
    {
        public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
        {
            if (Time.deltaTime < Mathf.Epsilon)
                return rot;

            // account for double-cover
            float dotProduct = Quaternion.Dot(rot, target);
            float multiplier = dotProduct > 0f ? 1f : -1f;
            target.x *= multiplier;
            target.y *= multiplier;
            target.z *= multiplier;
            target.w *= multiplier;

            // smooth damp (nlerp approx)
            var normalizedResult = new Vector4(
                Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
                Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
                Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
                Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
            ).normalized;

            // ensure deriv is tangent
            var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), normalizedResult);
            deriv.x -= derivError.x;
            deriv.y -= derivError.y;
            deriv.z -= derivError.z;
            deriv.w -= derivError.w;

            return new Quaternion(normalizedResult.x, normalizedResult.y, normalizedResult.z, normalizedResult.w);
        }
    }
}