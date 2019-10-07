using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD45
{
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        private string currentScene = "GameScene";

        public void RestartScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
            SceneManager.Instance.ResetScene();
        }

        public void GoToStartScreen()
        {
            currentScene = "GameScene";
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }

        public void StartGame()
        {
            currentScene = "GameScene";
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }

        public void GoToNextLevel()
        {


            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }
    }
}