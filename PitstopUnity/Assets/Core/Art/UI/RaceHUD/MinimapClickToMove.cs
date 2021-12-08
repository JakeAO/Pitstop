using SadPumpkin.Game.Pitstop.Core.Code.Camera;
using SadPumpkin.Game.Pitstop.Core.Code.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SadPumpkin.Game.Pitstop
{
    public class MinimapClickToMove : MonoBehaviour, IPointerClickHandler
    {
        public GameplayCamera GameplayCamera;
        public RawImage MinimapRenderer;

        private readonly Vector3[] _minimapImageCorners = new Vector3[4];

        public void OnPointerClick(PointerEventData eventData)
        {
            RectTransform minimapRect = MinimapRenderer.rectTransform;

            minimapRect.GetLocalCorners(_minimapImageCorners);

            Vector2 minimapPoint = minimapRect.InverseTransformPoint(eventData.pointerCurrentRaycast.screenPosition);

            Vector3 jumpPos = GameplayCamera.transform.position;
            jumpPos.x = minimapPoint.x.Remap(
                _minimapImageCorners[0].x, _minimapImageCorners[3].x,
                GameplayCamera.MinimumPosition.x, GameplayCamera.MaximumPosition.x);
            jumpPos.z = minimapPoint.y.Remap(
                _minimapImageCorners[0].y, _minimapImageCorners[1].y,
                GameplayCamera.MinimumPosition.z, GameplayCamera.MaximumPosition.z);
            GameplayCamera.transform.position = jumpPos;
        }
    }
}