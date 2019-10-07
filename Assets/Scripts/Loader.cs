using UnityEngine;
using System.Collections;

namespace Pincushion.LD45
{   
	public class Loader : MonoBehaviour 
	{
		public SceneManager sceneManager;          //GameManager prefab to instantiate.

		void Awake ()
		{
			//Check if a GameManager has already been assigned to static variable GameManager.instance or if it's still null
			if (SceneManager.Instance == null)
				
				//Instantiate gameManager prefab
				Instantiate(sceneManager);
		}
	}
}