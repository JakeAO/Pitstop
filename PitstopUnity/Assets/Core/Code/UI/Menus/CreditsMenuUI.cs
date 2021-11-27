using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SadPumpkin.Game.Pitstop
{
    public class CreditsMenuUI : MonoBehaviour, IPointerClickHandler
    {
        public TextAsset CreditsFile;
        public TMP_Text CreditsLabel;

        private void Start()
        {
            CreditsLabel.richText = true;
            CreditsLabel.text = CreditsFile.text;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.IsScrolling() || eventData.IsPointerMoving() || eventData.dragging)
            {
                return;
            }
            
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(CreditsLabel, eventData.position, eventData.pressEventCamera);
            if (linkIndex >= 0)
            {
                TMP_LinkInfo linkInfo = CreditsLabel.textInfo.linkInfo[linkIndex];
                string rawUrl = linkInfo.GetLinkID();
                if (!rawUrl.StartsWith("https://"))
                {
                    rawUrl = "https://" + rawUrl;
                }

                Application.OpenURL(rawUrl);
            }
        }
    }
}