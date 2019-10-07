using UnityEngine;
using UnityEngine.UI;

namespace Pincushion.LD45
{
    public class EscMenuController : MonoBehaviour
    {
        public Slider volumeSlider;

        private void Awake()
        {
            volumeSlider.value = AudioListener.volume;
        }

        public void ResumeClicked()
        {
            SceneManager.Instance.EnableEscMenu(false);
        }
        public void StartGameClicked()
        {
            GameManager.Instance.StartGame();
        }
        public void RestartLevelClicked()
        {
            GameManager.Instance.RestartScene();
        }
        public void ExitClicked()
        {
            Application.Quit();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; 
            #endif
        }

        public void VolumeChanged(float value)
        {
            AudioListener.volume = volumeSlider.value;
        }
    }
}