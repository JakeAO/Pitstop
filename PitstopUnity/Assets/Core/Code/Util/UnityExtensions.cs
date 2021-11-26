using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Util
{
    public static class UnityExtensions
    {
        /// <summary>
        /// Call SetActive() on this object ONLY IF it is not already in the target state.
        /// </summary>
        /// <param name="go">Gameobject to activate/deactivate.</param>
        /// <param name="active">Target state.</param>
        public static void UpdateActive(this GameObject go, bool active)
        {
            if (go.activeSelf != active)
            {
                go.SetActive(active);
            }
        }

        /// <summary>
        /// Call SetActive() on this object ONLY IF it is not already in the target state.
        /// </summary>
        /// <param name="c">Component on the gameobject to activate/deactivate.</param>
        /// <param name="active">Target state.</param>
        public static void UpdateActive(this Component c, bool active)
        {
            UpdateActive(c.gameObject, active);
        }

        public static bool IsApproximately(this Vector4 lhs, Vector4 rhs, float maxPerComponent = 0.1f)
        {
            return ((Vector3)lhs).IsApproximately(rhs, maxPerComponent)
                   && Mathf.Abs(lhs.w - rhs.w) <= maxPerComponent;
        }

        public static bool IsApproximately(this Vector3 lhs, Vector3 rhs, float maxPerComponent = 0.1f)
        {
            return ((Vector2)lhs).IsApproximately(rhs, maxPerComponent)
                   && Mathf.Abs(lhs.z - rhs.z) <= maxPerComponent;
        }

        public static bool IsApproximately(this Vector2 lhs, Vector2 rhs, float maxPerComponent = 0.1f)
        {
            return Mathf.Abs(lhs.x - rhs.x) <= maxPerComponent &&
                   Mathf.Abs(lhs.y - rhs.y) <= maxPerComponent;
        }
    }
}