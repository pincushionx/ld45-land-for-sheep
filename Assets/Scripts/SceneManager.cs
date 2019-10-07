using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pincushion.LD45
{
    public class SceneManager : MonoBehaviourSingleton<SceneManager>
    {
        public PlayerStats player;

        public OverlayController overlayController;
        public new Camera camera;
        public EscMenuController escMenuController;
        public Hexmap terrain;

        private List<Goat> goats = new List<Goat>();
        private List<Cow> cows = new List<Cow>();
        private List<Wolf> wolves = new List<Wolf>();


        public bool modalMode = false;
        public bool IsExpansionMode
        {
            get { return terrain.DrawEmpty; }
        }

        // prefabs
        GameObject goatPrefab;
        GameObject cowPrefab;
        GameObject wolfPrefab;

        // placing something
        public Grabbable grabbed;


        private bool paused;
        private bool resumeFromPausedAsModal = false;
        public bool Paused {
            get
            {
                return paused;
            }
            set
            {
                // only handle switches
                if (value != paused) {
                    // 
                    if (paused)
                    {
                        modalMode = resumeFromPausedAsModal;
                    }
                    else
                    {
                        resumeFromPausedAsModal = modalMode;
                        modalMode = true;
                    }
                    
                    paused = value;
                }
            }
        }

        private void Awake()
        {
            player = new PlayerStats();

            EnableEscMenu(false);
            camera = FindObjectOfType<Camera>();

            goatPrefab = Resources.Load("Models/Goat") as GameObject;
            cowPrefab = Resources.Load("Models/Cow") as GameObject;
            wolfPrefab = Resources.Load("Models/Wolf") as GameObject;

            UpdatePotentialEnergy();
        }

        private void Start()
        {
            //terrain.GenerateTiles();
            //terrain.DrawGrid();

            StartCoroutine("CheckForFailure");
        }

        public void UpdatePotentialEnergy()
        {
            float pe = GetPotentialEnergy();
            overlayController.SetPotentialEnergyValue(pe);
        }
        public float GetPotentialEnergy()
        {
            float pe = goats.Count * PlayerStats.potentialEnergyGoat;
            pe += cows.Count * PlayerStats.potentialEnergyCow;
            pe += wolves.Count * PlayerStats.potentialEnergyWolf;
            return pe;
        }

        #region Goat

        public Goat AddGoat(Vector2Int position)
        {
            GameObject goatGo = Instantiate(goatPrefab);
            Goat goat = goatGo.GetComponent<Goat>();
            HexTile tile = terrain.GetTile(position);


            // Don't spend energy here - this is used for babies
            // spend the energy
            //player.energy -= PlayerStats.costGoat;
            //overlayController.SetEnergyValue(player.energy);


            if (tile != null)
            {
                goats.Add(goat);
                goatGo.transform.position = tile.center;

                UpdatePotentialEnergy();
            }
            else
            {
                // something went wrong (possibly the tile was removed). Just pretend it never happened.
                Destroy(goatGo);
            }

            return goat;
        }
        public Grabbable GrabNewGoat()
        {
           /* if (grabbed != null)
            {
                Destroy(grabbed.gameObject);
                grabbed = null;
            }*/

            GameObject go = Instantiate(goatPrefab);
            grabbed = go.GetComponent<Grabbable>();

            // spend the energy
            player.energy -= PlayerStats.costGoat;
            overlayController.SetEnergyValue(player.energy);

            // don't show it until the player hovers over the map
            go.transform.position = new Vector3(-1000, -1000f, -1000f);

            grabbed.Grabbed = true;

            return grabbed;
        }

        public void RemoveGoat(Goat goat)
        {
            goats.Remove(goat);
            UpdatePotentialEnergy();
        }

        public bool GoatExists(Goat goat)
        {
            return goats.Contains(goat);
        }

        public Goat FindNearestGoat(Vector3 position, Goat sourceGoat = null, float maxDistance = 100f)
        {
            float minDistance = maxDistance;
            Goat goatAtMinDistance = null;
            foreach (Goat goat in goats)
            {
                if (goat != null && goat != sourceGoat)
                {
                    float distance = Vector3.Distance(goat.transform.position, position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        goatAtMinDistance = goat;
                    }
                }
            }
            return goatAtMinDistance;
        }

        #endregion
        #region Cow

        public Cow AddCow(Vector2Int position)
        {
            GameObject cowGo = Instantiate(cowPrefab);
            Cow cow = cowGo.GetComponent<Cow>();
            HexTile tile = terrain.GetTile(position);

            if (tile != null)
            {
                cows.Add(cow);
                cowGo.transform.position = tile.center;

                UpdatePotentialEnergy();
            }
            else
            {
                // something went wrong (possibly the tile was removed). Just pretend it never happened.
                Destroy(cowGo);
            }

            return cow;
        }

        public Grabbable GrabNewCow()
        {
            GameObject go = Instantiate(cowPrefab);
            grabbed = go.GetComponent<Grabbable>();

            // spend the energy
            player.energy -= PlayerStats.costCow;
            overlayController.SetEnergyValue(player.energy);

            // don't show it until the player hovers over the map
            go.transform.position = new Vector3(-1000, -1000f, -1000f);

            grabbed.Grabbed = true;

            return grabbed;
        }

        public void RemoveCow(Cow cow)
        {
            cows.Remove(cow);
            UpdatePotentialEnergy();
        }

        public bool CowExists(Cow cow)
        {
            return cows.Contains(cow);
        }

        public Cow FindNearestCow(Vector3 position, Cow sourceCow = null, float maxDistance = 100f)
        {
            float minDistance = maxDistance;
            Cow cowAtMinDistance = null;
            foreach (Cow cow in cows)
            {
                if (cow != null && cow != sourceCow)
                {
                    float distance = Vector3.Distance(cow.transform.position, position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        cowAtMinDistance = cow;
                    }
                }
            }
            return cowAtMinDistance;
        }

        #endregion

        #region Wolf

        public Wolf AddWolf(Vector2Int position)
        {
            GameObject wolfGo = Instantiate(wolfPrefab);
            Wolf wolf = wolfGo.GetComponent<Wolf>();
            HexTile tile = terrain.GetTile(position);

            if (tile != null)
            {
                wolves.Add(wolf);
                wolfGo.transform.position = tile.center;

                UpdatePotentialEnergy();
            }
            else
            {
                // something went wrong (possibly the tile was removed). Just pretend it never happened.
                Destroy(wolfGo);
            }

            return wolf;
        }

        public Grabbable GrabNewWolf()
        {
            GameObject go = Instantiate(wolfPrefab);
            grabbed = go.GetComponent<Grabbable>();

            // spend the energy
            player.energy -= PlayerStats.costWolf;
            overlayController.SetEnergyValue(player.energy);

            // don't show it until the player hovers over the map
            go.transform.position = new Vector3(-1000, -1000f, -1000f);

            grabbed.Grabbed = true;

            return grabbed;
        }

        public void RemoveWolf(Wolf wolf)
        {
            wolves.Remove(wolf);
            UpdatePotentialEnergy();
        }

        public Wolf FindNearestWolf(Vector3 position, Wolf sourceWolf = null, float maxDistance = 100f)
        {
            float minDistance = maxDistance;
            Wolf wolfAtMinDistance = null;
            foreach (Wolf wolf in wolves)
            {
                if (wolf != null && wolf != sourceWolf)
                {
                    float distance = Vector3.Distance(wolf.transform.position, position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        wolfAtMinDistance = wolf;
                    }
                }
            }
            return wolfAtMinDistance;
        }

        #endregion



        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EnableEscMenu(!Paused);
            }
            else if (!Paused)
            {
                // place something
                if (grabbed != null)
                {
                    Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Terrain")))
                    {
                        Vector3 placePosition = hit.point;

                        grabbed.transform.position = placePosition;

                        if (Input.GetMouseButtonDown(0))
                        {
                            HexTile tile = GetTileAtMousePosition();

                            if (tile != null)
                            {
                                ///TODO Clean this up
                                Goat goat = grabbed.GetComponent<Goat>();
                                if (goat != null)
                                {
                                    goats.Add(goat);
                                    UpdatePotentialEnergy();
                                }

                                Cow cow = grabbed.GetComponent<Cow>();
                                if (cow != null)
                                {
                                    cows.Add(cow);
                                    UpdatePotentialEnergy();
                                }

                                Wolf wolf = grabbed.GetComponent<Wolf>();
                                if (wolf != null)
                                {
                                    wolves.Add(wolf);
                                    UpdatePotentialEnergy();
                                }

                                grabbed.Grabbed = false;
                                grabbed = null;
                            }
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            // operation was cancelled
                            Destroy(grabbed.gameObject);
                            grabbed = null;
                        }
                    }
                }
                else if (IsExpansionMode)
                {
                    Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Terrain")))
                    {
                        Vector3 placePosition = hit.point;

                        if (Input.GetMouseButtonDown(0))
                        {
                            HexTile tile = GetTileAtMousePosition();

                            if (tile != null)
                            {
                                if (tile.IsEmpty)
                                {
                                    HexTile.MaterialEnum material = (terrain.TileCount < 9) ? HexTile.MaterialEnum.Grass : HexTile.MaterialEnum.Rock;

                                    terrain.SetTile(tile.tilePosition, material);
                                    SetExpansionMode(false);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, 100f))
                        {
                            Goat goat = hit.collider.gameObject.GetComponentInParent<Goat>();
                            if (goat != null && goats.Contains(goat))
                            {
                                // consume the goat
                                player.energy += PlayerStats.potentialEnergyGoat;
                                overlayController.SetEnergyValue(player.energy);

                                // using the single dead function here
                                // may want to also have a sacrifice animation
                                goat.GotDead();
                            }
                            Cow cow = hit.collider.gameObject.GetComponentInParent<Cow>();
                            if (cow != null && cows.Contains(cow))
                            {
                                // consume the cow
                                player.energy += PlayerStats.potentialEnergyCow;
                                overlayController.SetEnergyValue(player.energy);

                                // kill the cow
                                cow.GotDead();
                            }
                            Wolf wolf = hit.collider.gameObject.GetComponentInParent<Wolf>();
                            if (wolf != null && wolves.Contains(wolf))
                            {
                                // consume the cow
                                player.energy += PlayerStats.potentialEnergyWolf;
                                overlayController.SetEnergyValue(player.energy);

                                // kill the cow
                                wolf.GotDead();
                            }
                        }
                    }
                }

            }
        }

        #region Tile

        public void SetExpansionMode(bool enable)
        {
            if (enable)
            {
                if (terrain.DrawEmpty == false)
                {
                    // this is a parallel to placing animals, so charge the player here
                    player.energy -= PlayerStats.costRockTile;
                    overlayController.SetEnergyValue(player.energy);

                    terrain.EnsureEmptyLayerOfCells();
                    terrain.DrawEmpty = true;
                    terrain.DrawGrid();
                }
            }
            else if (terrain.DrawEmpty != false)
            {
                terrain.DrawEmpty = false;
                terrain.DrawGrid();
            }
        }

        // TODO find a home
        public HexTile GetTileAtMousePosition()
        {
            Camera camera = FindObjectOfType<Camera>();
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Terrain")))
            {
                return terrain.GetTile(hit);
            }
            return null;
        }

        #endregion


        public void PlayerDied()
        {
            overlayController.ShowLosingConidtionPrompt();
            modalMode = true;
        }

        public void ResetScene()
        {
            EnableEscMenu(false);
        }

        public void EnableEscMenu(bool enable)
        {
            Paused = enable;
            overlayController.gameObject.SetActive(!Paused);
            escMenuController.gameObject.SetActive(Paused);
        }


        IEnumerator CheckForFailure()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (goats.Count == 0 && cows.Count == 0 && wolves.Count == 0 && player.energy < PlayerStats.costGoat)
                {
                    // the player can't buy a goat - end the game
                    PlayerDied();
                }
            }
        }
    }
}