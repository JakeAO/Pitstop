using UnityEngine;
using UnityEngine.SceneManagement;

namespace SadPumpkin.Game.Pitstop
{
    public class RaceOverUI : MonoBehaviour
    {
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void OnContinueButtonPressed()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}