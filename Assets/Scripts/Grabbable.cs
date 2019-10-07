using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD45
{
    public class Grabbable : MonoBehaviour
    {
        private bool grabbed = false; // player is placing this Goat. It's effectively inactive.
        public bool Grabbed
        {
            get { return grabbed; }
            set { grabbed = value; }
        }
    }
}