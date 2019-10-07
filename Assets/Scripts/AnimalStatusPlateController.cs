using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pincushion.LD45 {
	public class AnimalStatusPlateController : MonoBehaviour {
		public Image hungerCastBar;
        public Image matingCastBar;
        public Image specialCastBar;

        public void SetHungerValue(float value) {
            hungerCastBar.fillAmount = value;
		}

        public void SetMatingValue(float value)
        {
            matingCastBar.fillAmount = value;
        }

        public void SetSpecialValue(float value)
        {
            if (specialCastBar != null)
            {
                specialCastBar.fillAmount = value;
            }
        }
    }
}