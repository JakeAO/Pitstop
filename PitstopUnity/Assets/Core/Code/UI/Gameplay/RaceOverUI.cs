using SadPumpkin.Game.Pitstop.Core.Code.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SadPumpkin.Game.Pitstop
{
    public class RaceOverUI : MonoBehaviour
    {
        public GameObject WinnerObject;
        public GameObject LoserObject;

        public void Show(bool winner)
        {
            gameObject.UpdateActive(true);
            WinnerObject.UpdateActive(winner);
            LoserObject.UpdateActive(!winner);
        }

        public void OnContinueButtonPressed()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}