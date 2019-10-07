using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pincushion.LD45
{
    public class OverlayController : MonoBehaviour
    {
        public Image energyBar;
        public Image potentialEnergyBar;

        public GameObject loseConditionPrompt;

        public void GoatClicked()
        {
            if (SceneManager.Instance.player.energy > PlayerStats.costGoat)
            {
                SceneManager.Instance.GrabNewGoat();
            }
            else
            {
                // not enough energy
                // give feedback

            // sound, text, flashing energy overlay
            }
        }
        public void CowClicked()
        {
            if (SceneManager.Instance.player.energy > PlayerStats.costCow)
            {
                SceneManager.Instance.GrabNewCow();
            }
            else
            {
                // not enough energy
                // give feedback

                // sound, text, flashing energy overlay
            }
        }
        public void WolfClicked()
        {
            if (SceneManager.Instance.player.energy > PlayerStats.costWolf)
            {
                SceneManager.Instance.GrabNewWolf();
            }
            else
            {
                // not enough energy
                // give feedback

                // sound, text, flashing energy overlay
            }
        }
        public void HexTileClicked()
        {
            if (SceneManager.Instance.player.energy > PlayerStats.costRockTile)
            {
                SceneManager.Instance.SetExpansionMode(true);
            }
            else
            {
                // not enough energy
                // give feedback

                // sound, text, flashing energy overlay
            }
        }

        public void RedrawGridClicked()
        {
            SceneManager.Instance.terrain.GenerateTiles();
            SceneManager.Instance.terrain.DrawGrid();
        }

        public void RestartGameClicked()
        {
            GameManager.Instance.RestartScene();
        }


        public void SetEnergyValue(float value)
        {
            energyBar.fillAmount = value;
        }
        public void SetPotentialEnergyValue(float value)
        {
            potentialEnergyBar.fillAmount = value;
        }

        public void ShowLosingConidtionPrompt()
        {
            loseConditionPrompt.SetActive(true);
        }
    }
}