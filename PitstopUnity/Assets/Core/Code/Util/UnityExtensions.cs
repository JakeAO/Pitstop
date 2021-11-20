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
    }
}