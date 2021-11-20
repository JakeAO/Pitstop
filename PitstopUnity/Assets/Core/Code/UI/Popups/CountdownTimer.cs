using TMPro;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public class CountdownTimer : MonoBehaviour
    {
        public TMP_Text Label;

        private float _timer;
        private float _zeroTimer;
        private string _zeroText;

        public void ShowCountdown(int countdownSeconds, int zeroSeconds, string zeroText = null)
        {
            Label.text = countdownSeconds.ToString();

            _timer = countdownSeconds;
            _zeroTimer = zeroSeconds;
            _zeroText = zeroText;

            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (_timer > 0f)
            {
                _timer -= Time.smoothDeltaTime;

                int seconds = Mathf.CeilToInt(_timer);
                Label.text = seconds == 0 ? _zeroText : seconds.ToString();
            }
            else if (_zeroTimer > 0f)
            {
                _zeroTimer -= Time.smoothDeltaTime;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}