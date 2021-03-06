using SadPumpkin.Game.Pitstop.Core.Code.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SadPumpkin.Game.Pitstop.Core.Code.UI.Gameplay
{
    public class RaceOverUI : MonoBehaviour
    {
        public string MainMenuScene;
        
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
            SceneManager.LoadScene(MainMenuScene);
        }
    }
}