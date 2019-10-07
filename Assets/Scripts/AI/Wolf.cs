using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pincushion.LD45
{
    [RequireComponent(typeof(Pathable))]
    [RequireComponent(typeof(Grabbable))]
    public class Wolf : MonoBehaviour
    {
        public AnimalStatusPlateController status;

        public Pathable pathable;
        public Grabbable grabbable;
        public Animator animator;

        public float hunger = 1f;
        public float mating = 1f;

        private float hungerRate = 0.0125f;
        private float matingRate = 0.025f;

        private float matingSearchDistance = 10f;
        private float matingDistance = 0.25f;

        private float hungerEatThreshold = 0.5f;
        private float eatingDistance = 0.125f;

        private bool searchingForMate = false;
        private Wolf mate;
        private HexTile eatingTile;

        private Goat eatingGoat;

        private bool placing = false; // player is placing this Goat. It's effectively inactive.
        public bool Placing
        {
            get { return placing; }
            set { placing = value; }
        }

        private void Awake()
        {
            pathable = GetComponent<Pathable>();
            grabbable = GetComponent<Grabbable>();
            animator = GetComponentInChildren<Animator>();

            pathable.OnPathingStateChanged += PathingStateChanged;
        }

        void Update()
        {
            if (!SceneManager.Instance.Paused || grabbable.Grabbed)
            {
                UpdateHunger();
                UpdateMating();
            }
        }

        private void PathingStateChanged()
        {
            if (pathable.pathing)
            {
                animator.Play("Walk");
                animator.speed = 1f;
            }
            else
            {
                animator.StopPlayback();
            }
        }

        private void UpdateHunger()
        {
            hunger -= hungerRate * Time.deltaTime;

            if (hunger < hungerEatThreshold)
            {
                if (eatingGoat != null)
                {
                    if (!SceneManager.Instance.GoatExists(eatingGoat)) // was already eaten
                    {
                        eatingGoat = null;
                    }
                    else
                    {
                        Vector3 position = transform.position;
                        Vector3 tilepos = eatingTile.center;

                        float distance = Vector3.Distance(position, tilepos);
                        if (distance < eatingDistance)
                        {
                            animator.Play("Eat");
                            animator.speed = 1f;

                            hunger = 1f;

                            eatingGoat.GotDead();

                            pathable.StopPathing();
                        }
                        else if (pathable.stopped)
                        {
                            eatingTile = null;
                        }
                    }
                }
                else
                {
                    //HexTile tile = SceneManager.Instance.terrain.FindNearestGrass(transform.position);
                    eatingGoat = SceneManager.Instance.FindNearestGoat(transform.position);
                    
                    // wolves only eat goats. Cows give them tummy aches.
                    if (eatingGoat != null)
                    {
                        Vector2Int tilePosition = eatingGoat.pathable.GetTilePosition();
                        eatingTile = SceneManager.Instance.terrain.GetTile(tilePosition);
                        if (eatingTile == null || eatingTile.IsEmpty)
                        {
                            eatingTile = null;
                            eatingGoat = null;
                        }
                        else
                        {
                            pathable.PathTo(eatingTile.tilePosition);
                        }
                    }
                }
            }

            if (hunger < 0f)
            {
                status.SetHungerValue(0f);
                GotDead();
            }
            else
            {
                status.SetHungerValue(hunger);
            }
        }

        private void UpdateMating()
        {
            if (mate != null)
            {
                float distance = Vector3.Distance(mate.transform.position, transform.position);
                if (distance < matingDistance)
                {
                    pathable.StopPathing();

                    if (mate.mating == 0f)
                    {
                        Vector2Int position = pathable.GetTilePosition();
                        Cow kid = SceneManager.Instance.AddCow(position);

                        mate = null;
                        mating = 1f;
                        status.SetMatingValue(mating);
                    }
                }
                else if (pathable.stopped)
                {
                    mate = null;
                }
            }
            else if (searchingForMate)
            {
                // searching for mate, do nothing
            }
            else
            {
                mating -= matingRate * Time.deltaTime;

                if (mating < 0f)
                {
                    mating = 0f;
                    status.SetMatingValue(0f);

                    StartCoroutine("SearchForMate");

                    // mate
                    // look for mate
                    // show hearts?
                    // spawn child (1 between 2 goats
                }
                else
                {
                    status.SetMatingValue(mating);
                }
            }
        }

        public void SetPath(Vector2Int destination)
        {
            pathable.PathTo(destination);
        }

        public void GotDead()
        {
            SceneManager.Instance.RemoveWolf(this);
            StartCoroutine("Deadding");
        }

        IEnumerator Deadding()
        {
            float elapsedTime = 0f;
            float transitionTime = 1f;

            while (elapsedTime < transitionTime)
            {
                float percent = elapsedTime / transitionTime;

                Vector3 angle = gameObject.transform.eulerAngles;
                angle.z = Mathf.LerpAngle(angle.z, 90f, Time.deltaTime);
                gameObject.transform.eulerAngles = angle;
                
                elapsedTime += Time.deltaTime;
                yield return 0;
            }

            Destroy(gameObject);
        }

        IEnumerator SearchForMate()
        {
            searchingForMate = true;

            while (mate == null)
            {
                yield return new WaitForSeconds(1.0f);
                mate = SceneManager.Instance.FindNearestWolf(transform.position, this, matingSearchDistance);
            }

            if (mate != null)
            {
                float distance = Vector3.Distance(mate.transform.position, transform.position);
                if (distance > matingDistance)
                {
                    Vector2Int matePosition = mate.pathable.GetTilePosition();
                    pathable.PathTo(matePosition);
                }
            }

            searchingForMate = false;
        }
    }
}