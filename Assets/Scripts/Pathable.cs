using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Pincushion.LD45 {
	public class Pathable : MonoBehaviour {
		// settings
		public bool persist { get; set; } // continue until destination is found
		public float movementSpeed = 2.0f;

		// state flags (probably should change to a single enum
		public bool pathing { get {return _pathing; } }
		public bool paused { get {return _paused; } }
		public bool stopped { get {return _stopped; } }

		public bool _pathing = false;
		public bool _paused = false;
		public bool _stopped = true;

		// delegates
		public delegate void PathingStateChanged();
		public event PathingStateChanged OnPathingStateChanged;

		// private
		private Vector3[] path;
		private int pathPosition = -1;


		void Update() {
			//if (Fall ()) {
				// after falling the path will have to be updated
				//StopPathing();
			//} else {
				UpdatePath ();
			//}
		}

		/// <summary>
		/// updates pathing operations
		/// currently public so the job manager can access, but ideally it'd be private
		/// </summary>
		public void UpdatePath() {
            // If a path is set, move the npc
            if (pathPosition > -1) {
				Vector3 testPosition = path [pathPosition];
				if(transform.position.Equals(testPosition)) {
					pathPosition++; // set next path
				}

				if (pathPosition >= path.Length) {
					StopPathing();
				}
				if (pathPosition > 0) {
					float delta = Time.deltaTime; 
					Vector3 previous = transform.position;
					Vector3 destination = path [pathPosition];
					Vector3 currentPosition = Vector3.MoveTowards (previous, destination, delta * movementSpeed);

					transform.position = currentPosition;

					Vector3 lookAtPos = new Vector3();
					lookAtPos.x = destination.x;
					lookAtPos.y = transform.position.y;
					lookAtPos.z = destination.z;
					gameObject.transform.LookAt (lookAtPos);
				}
			}
		}

        public Vector2Int GetTilePosition()
        {
            int layerMask = 1 << 10;

            // find the cell below the pathable
            RaycastHit hitInfo = new RaycastHit();
            Vector3 rayPos = transform.position;
            rayPos.y += 2f;
            Ray ray = new Ray(rayPos, Vector3.down);
            if (!Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
            {//, LayerMask.GetMask("Terrain"))) {
             // there's nothing below

                // there's an issue here
                return new Vector2Int(-1,-1);
            }

            HexTile tile = SceneManager.Instance.terrain.GetTile(hitInfo);

            if (tile != null)
            {
                return tile.tilePosition;
            }

            // didn't find a cell
            return new Vector2Int(-1,-1);
        }

		/// <summary>
		/// Paths to destination
		/// </summary>
		/// <param name="destination">Destination, in cell space</param>
		public void PathTo(Vector2Int destination) {
			if (path != null) {
                pathPosition = -1;
                path = null;
			}

            // find the cell below the pathable
            Vector2Int start = GetTilePosition();
			if (start == null) {
				// there's noi cell below the pathable
				return;
			}

            // this should be cached
            Vector3[] hexpath = SceneManager.Instance.terrain.GetPath(start, destination);

			if (hexpath == null || hexpath.Length == 0) {
				// could not find the path
				pathPosition = -1;
				path = null;
				return;
			}

			// the path was found, initialize the position
			pathPosition = 0;



            int i = 0;
            path = new Vector3[hexpath.Length + 1];
            path[i++] = transform.position;
            
            foreach (Vector3 pathItem in hexpath)
            {
                path[i++] = pathItem;
            }


			//transform.position = path [pathPosition];

			// change state
			_pathing = true;
			_paused = false;
			_stopped = false;
			if (OnPathingStateChanged != null) {
				OnPathingStateChanged ();
			}
		}
    

		public void StopPathing() {
			path = null;
			pathPosition = -1; // stop the npc

			// change state
			_pathing = false;
			_paused = false;
			_stopped = true;
			if (OnPathingStateChanged != null) {
				OnPathingStateChanged ();
			}
		}

		#region gravity

		private float gravity = 20f;


		/// <summary>
		/// If there is nothing below the game object, fall. Return true if falling, false otherwise
		/// </summary>
		private bool Fall() {
			// find the floor - ensure we're not going below it
			RaycastHit hitInfo = new RaycastHit ();
			Vector3 rayPos = transform.position;
			rayPos.y += 1f;

			//Debug.DrawRay (rayPos, Vector3.down, Color.red, 1);
			if (!Physics.Raycast (rayPos, Vector3.down, out hitInfo, Mathf.Infinity, LayerMask.GetMask("Terrain"))) {
				// there's nothing below the npc
				Debug.LogError("NPC can't find the floor");
				return false;
			}

			Vector3 pos = transform.position;
			if(pos.y == hitInfo.point.y) {
				// the character is grounded.. need to fix IsGrounded()
				return false;
			}

			pos.y -= gravity * Time.deltaTime;

			// if we're past the floor, equal the floor height
			if (pos.y < hitInfo.point.y) {
				pos.y = hitInfo.point.y;
			} 
			transform.position = pos;

			return true;
		}


		#endregion
    }
}