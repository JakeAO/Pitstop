using UnityEngine;
using UnityEngine.SceneManagement;

namespace SadPumpkin.Game.Pitstop
{
    public class AppInit : MonoBehaviour
    {
        public static AudioSource BackgroundMusicSource { get; private set; }

        public string MainMenuScene;
        public AudioClip BackgroundLoop;

        private void Start()
        {
            var newGo = new GameObject("BackgroundMusicSource");
            DontDestroyOnLoad(newGo);
            BackgroundMusicSource = newGo.AddComponent<AudioSource>();
            BackgroundMusicSource.clip = BackgroundLoop;
            BackgroundMusicSource.loop = true;
            BackgroundMusicSource.volume = 1f;
            BackgroundMusicSource.spatialize = false;
            BackgroundMusicSource.Play();

            SceneManager.LoadScene(MainMenuScene);
        }
    }
}