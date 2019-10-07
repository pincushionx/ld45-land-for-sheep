using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using UnityEditor;

namespace Pincushion.LD45 {
    public class Hexmap : MonoBehaviour
    {
        int tilesW = 1;
        int tilesH = 1;

        private float tileSize = 1f;

        private bool drawEmpty = false;
        public bool DrawEmpty {
            get { return drawEmpty; }
            set { drawEmpty = value; }
        }

        private HexTile[] tiles;
        public HexTile[] Tiles {
            get { return tiles; }
        }

        public int TileCount
        {
            get {
                int count = 0;
                foreach (HexTile tile in tiles)
                {
                    if (tile != null && !tile.IsEmpty)
                    {
                        count++;
                    }
                }
                return count;
            }
        }
        public Vector2Int Size
        {
            get { return new Vector2Int(tilesW, tilesH); }
        }

        private Dictionary<int, HexTile> tilesByTriangleIndex = new Dictionary<int, HexTile>();
        private List<Vector3> vertices = new List<Vector3>();
        private List<int> triangles = new List<int>();
        private List<Color> colours = new List<Color>();

        private bool redraw = false;
        public bool Redraw {
            set
            {
                redraw = value;
            }
        }

        public HexTile calibrationTile;


        private Vector2Int[] neighbouringCellOffsets = {
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),

        new Vector2Int(1, -1),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
    };


        private Material material;

        private Mesh mesh;
        private GameObject go;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;




        //draw offset
        private Vector3 offset = new Vector3();


        private void Awake()
        {
            InitializeGameObject();

            // initialize the tiles
            tiles = new HexTile[tilesH * tilesW];
            FillEmptyTiles();
        }

        private void Start()
        {
            StartCoroutine("UpdateTiles");
        }

        private void Update()
        {

        }

        IEnumerator UpdateTiles()
        {
            while (tiles == null)
            {
                yield return new WaitForSeconds(1.0f);
            }

            while (true)
            {
                yield return new WaitForSeconds(1.0f);

                if (!SceneManager.Instance.Paused)
                {
                    foreach (HexTile tile in tiles)
                    {
                        if (tile.IsGrassTile && !tile.HasGrass)
                        {
                            tile.regrowGrass -= tile.regrowGrassRate;

                            if (tile.regrowGrass < 0f)
                            {
                                tile.GrowGrass();
                                redraw = true;
                            }
                        }
                    }
                    if (redraw)
                    {
                        redraw = false;
                        DrawGrid();
                    }
                }
            }
        }

        #region GrowGrid

        public void EnsureEmptyLayerOfCells()
        {
            if (HasAnyOuterCells())
            {
                GrowGrid();
                //redraw = true;
            }
        }
        private bool HasAnyOuterCells()
        {
            HexTile tile;
            for (int x = 0; x < tilesW; x++)
            {
                // top row
                tile = tiles[(tilesH - 1) * tilesW + x];
                if (!(tile == null || tile.IsEmpty))
                {
                    return true;
                }
                //bottom row
                /*tile = tiles[x];
                if (!(tile == null || tile.IsEmpty))
                {
                    return true;
                }*/
            }
            for (int z = 0; z < tilesH; z++)
            {
                // left column
               /* tile = tiles[z * tilesW];
                if (!(tile == null || tile.IsEmpty))
                {
                    return true;
                }*/
                // right row
                tile = tiles[(z * tilesW) + (tilesW - 1)];
                if (!(tile == null || tile.IsEmpty))
                {
                    return true;
                }
            }

            return false;
        }
        // adds a layer of tiles all around the grid
        private void GrowGrid()
        {
           // calibrationTile = tiles[0];

            int newTilesW = tilesW + 1;
            int newTilesH = tilesH + 1;
            HexTile[] newTiles = new HexTile[newTilesH * newTilesW];

            for (int z = 0; z < tilesH; z++)
            {
                for (int x = 0; x < tilesW; x++)
                {
                    newTiles[z * newTilesW + x] = tiles[z * tilesW + x];
                }
            }
            tiles = newTiles;
            tilesW = newTilesW;
            tilesH = newTilesH;

            FillEmptyTiles();
        }
        private void FillEmptyTiles()
        {
            Vector2Int position = new Vector2Int();

            for (int z = 0; z < tilesH; z++)
            {
                for (int x = 0; x < tilesW; x++)
                {
                    HexTile tile = tiles[z * tilesW + x];
                    if (tile == null)
                    {
                        position.y = z;
                        position.x = x;
                        SetTile(position, HexTile.MaterialEnum.Empty);
                    }
                }
            }
        }

        #endregion

        public void SetTile(Vector2Int position, HexTile.MaterialEnum material)
        {
            int index = position.y * tilesW + position.x;

            tiles[index] = new HexTile();
            tiles[index].height = Random.value;
            tiles[index].material = material;

            //redraw = true;
        }
        public void RemoveTile(Vector2Int position)
        {
            int index = position.y * tilesW + position.x;

            tiles[index] = null;

           // redraw = true;
        }




        public void GenerateTiles()
        {
            tiles = new HexTile[tilesW * tilesH];

            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new HexTile();
                tiles[i].height = Random.value;// i / 20f;// Random.value * 5f;
                tiles[i].material = (HexTile.MaterialEnum)(i % 3);
            }

            EnsureEmptyLayerOfCells();
        }


        public void InitializeGameObject()
        {
            tilesByTriangleIndex = new Dictionary<int, HexTile>();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color> colours = new List<Color>();




            mesh = new Mesh();

            go = new GameObject(string.Format("Hexmap"));
            meshFilter = go.AddComponent<MeshFilter>();
            meshRenderer = go.AddComponent<MeshRenderer>();
            meshCollider = go.AddComponent<MeshCollider>();

            if (material == null)
            {
                InitializeTerrainTextureArray();
            }

            go.layer = LayerMask.NameToLayer("Terrain");

            // set the game object's position
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
        }

        public HexTile FindNearestGrass(Vector2Int position) //Vector3 position)
        {
            if (tiles == null)
            {
                return null;
            }

            float minDistance = 100f;
            HexTile tileAtMinDistance = null;

            foreach (HexTile tile in tiles)
            {
                if (tile.HasGrass)
                {
                    //float distance = Vector3.Distance(position, tile.center);
                    float distance = Vector2Int.Distance(position, tile.tilePosition);
                    if (distance < minDistance)
                    {
                        tileAtMinDistance = tile;
                        minDistance = distance;
                    }
                }
            }

            return tileAtMinDistance;
        }
        public HexTile FindNearestTileToConvertToGrass(Vector2Int position)
        {
            if (tiles == null)
            {
                return null;
            }

            float minDistance = 100f;
            HexTile tileAtMinDistance = null;

            foreach (HexTile tile in tiles)
            {
                if (tile.CanConvertToGrassTile)
                {
                    //float distance = Vector3.Distance(position, tile.center);
                    float distance = Vector2Int.Distance(position, tile.tilePosition);
                    if (distance < minDistance)
                    {
                        tileAtMinDistance = tile;
                        minDistance = distance;
                    }
                }
            }

            return tileAtMinDistance;
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <returns>The path.</returns>
        public Vector3[] GetPath(Vector2Int fromPosition, Vector2Int destination)
        {
            List<Vector3> path = new List<Vector3>();

            int fromCell = fromPosition.y * tilesW + fromPosition.x;
            int toCell = destination.y * tilesW + destination.x;

            SimplePriorityQueue<int> frontier = new SimplePriorityQueue<int>();
            Dictionary<int, float> testedCells = new Dictionary<int, float>();
            Dictionary<int, int> parentCells = new Dictionary<int, int>();

            int currentCell = -1;
            //IVector3 currentCellPosition;

            int maxTiles = tiles.Length;

            // address out of bounds issues
            if (fromCell < 0 || fromCell >= maxTiles || toCell < 0 || toCell >= maxTiles)
            {
                return null;
            }


            // test whether or not both position are walkable
            if (tiles[fromCell] != null && tiles[fromCell].PathScore > 0
                && tiles[toCell] != null && tiles[toCell].PathScore > 0)
            {

                frontier.Enqueue(fromCell, 0);
                testedCells.Add(fromCell, 0);

                while (frontier.Count > 0)
                {

                    currentCell = frontier.Dequeue();

                    if (currentCell == toCell)
                    {
                        // found the destination
                        break;
                    }

                    // Get neighbours
                    for (int i = 0; i < neighbouringCellOffsets.Length; i++)
                    {
                        //TODO voxel nodes need to return siblings. this will break
                        //VoxelOctreeNode neighbouringCell = voxels.GetNode(currentCellPosition + (neighbouringCellOffsets[i] * voxels.GetScale()));
                        int neighbouringCell = currentCell + (neighbouringCellOffsets[i].y * tilesW + neighbouringCellOffsets[i].x);


                        Vector2Int neighbouringCellPosition = new Vector2Int(
                            (currentCell % tilesW) + neighbouringCellOffsets[i].x,
                            Mathf.FloorToInt(currentCell / tilesW) + neighbouringCellOffsets[i].y);

                        // test if the neighboring cell exists, is walkable 
                        if (neighbouringCell > 0 && neighbouringCell < maxTiles && tiles[neighbouringCell] != null && tiles[neighbouringCell].PathScore > 0)
                        {
                            //IVector3 neighbouringCellPosition = neighbouringCell.bounds.position;
                            float newCost = testedCells[currentCell] + tiles[neighbouringCell].PathScore;

                            // test if the neighboring cell hasn't been tested or has a better score than previously tested
                            if (!testedCells.ContainsKey(neighbouringCell) || newCost < testedCells[neighbouringCell])
                            {
                                // update the cell cost
                                testedCells[neighbouringCell] = newCost;
                                parentCells[neighbouringCell] = currentCell;

                                float guessedCost = newCost + (float)GetHeuristic(neighbouringCellPosition, destination);
                                if (frontier.Contains(neighbouringCell))
                                {
                                    frontier.UpdatePriority(neighbouringCell, guessedCost);
                                }
                                else
                                {
                                    frontier.Enqueue(neighbouringCell, guessedCost);
                                }
                            }
                        }
                    }
                }

                // build the path from parent cells
                while (currentCell > -1)
                {
                    path.Add(tiles[currentCell].center);
                    currentCell = parentCells.ContainsKey(currentCell) ? parentCells[currentCell] : -1;
                }

                // reverse the path, so the first step is first
                path.Reverse();
            }
            return path.ToArray();

            // the following shows all tested cells
            //VoxelCell[] voxelCellList = new VoxelCell[parentCells.Values.Count];
            //parentCells.Values.CopyTo (voxelCellList, 0);
            //return voxelCellList;
        }

        private float GetHeuristic(Vector2Int from, Vector2Int to)
        {
            return new Vector2(to.x - from.x, to.y - from.y).magnitude;
        }

        /* private Vector3 GetTilePosition(int tileIndex)
         {
             if (tileIndex > 0 && tileIndex < tiles.Length)
             {
                 tileIndex 
             }
             // exception?
             return new Vector3();
         }

         */


        public HexTile GetTile(RaycastHit hit)
        {
            if (hit.triangleIndex >= 0 && hit.triangleIndex < tilesByTriangleIndex.Count && tilesByTriangleIndex.ContainsKey(hit.triangleIndex))
            {
                return tilesByTriangleIndex[hit.triangleIndex];
            }
            return null;
        }

        public HexTile GetTile(Vector2Int position)
        {
            int index = position.y * tilesW + position.x;

            if (index < 0 || index >= tiles.Length)
            {
                return null;
            }
            return tiles[index];
        }



        public void DrawGrid()
        {
            float outerRadius = tileSize;
            float innerRadius = outerRadius * 0.866025404f; // thanks catlike coding https://catlikecoding.com/unity/tutorials/hex-map/part-1/

            Vector3[] corners = {
                new Vector3(0f, 0f, outerRadius),                   //     +z (top)
                new Vector3(innerRadius, 0f, 0.5f * outerRadius),   // +x, +z
                new Vector3(innerRadius, 0f, -0.5f * outerRadius),  // +x, -z
                new Vector3(0f, 0f, -outerRadius),                  //     -z (bottom)
                new Vector3(-innerRadius, 0f, -0.5f * outerRadius), // 
                new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
                new Vector3(0f, 0f, outerRadius)
            };

            tilesByTriangleIndex.Clear();
            vertices.Clear();
            triangles.Clear();
            colours.Clear();

            int vertexIndex = 0;

            int maxTiles = tiles.Length;

            Vector2Int[] cornerref;


           

            // after the map grows, this is used to maintain tile coordinates
            if (calibrationTile != null)
            {
                //get zero tile
                int z = 0;
                int x = 0;
                //cornerref = (z % 2 == 1) ? cornerrefEven : cornerrefOdd;

                HexTile tile = tiles[z * tilesW + x];

                float xoffset = (z % 2 == 0) ? 0f : innerRadius;
                Vector3 center = new Vector3(x * innerRadius * 2f + xoffset,0, z * outerRadius * 1.5f);


                for (z = 0; z < tilesH; z++)
                {
                    cornerref = (z % 2 == 1) ? cornerrefEven : cornerrefOdd;
                    for (x = 0; x < tilesW; x++)
                    {
                        tile = tiles[z * tilesW + x];
                        if (tile == calibrationTile)
                        {
                            // then make the hex
                            xoffset = (z % 2 == 0) ? 0f : innerRadius;
                            Vector3 calibrationCenter = new Vector3(x * innerRadius * 2f + xoffset, 0, z * outerRadius * 1.5f);



                            offset += center - calibrationCenter;
                        }
                    }
                }
                calibrationTile = null;
            }

            for (int z = 0; z < tilesH; z++)
            {
                cornerref = (z % 2 == 1) ? cornerrefEven : cornerrefOdd;

                for (int x = 0; x < tilesW; x++)
                {
                    HexTile tile = tiles[z * tilesW + x];

                    // first, should do null check
                    // and then check if we should draw empty cells
                    if (tile == null || (tile.IsEmpty && drawEmpty == false)) {
                        continue;
                    }
                    // then make the hex
                    float xoffset = (z % 2 == 0) ? 0f : innerRadius;
                    Vector3 center = new Vector3(x * innerRadius * 2f + xoffset, tile.height, z * outerRadius * 1.5f) + offset;




                    for (int cornerIndex = 0; cornerIndex < 6; cornerIndex++)
                    {

                        Vector3 p0 = center;
                        Vector3 p1 = center + corners[cornerIndex];
                        Vector3 p2 = center + corners[cornerIndex + 1];







                        Vector2Int corner1Ref = cornerref[cornerIndex * 2];
                        Vector2Int corner2Ref = cornerref[cornerIndex * 2 + 1];

                        int adjacent1Index = (corner1Ref.y + z) * tilesW + x + corner1Ref.x;
                        int adjacent2Index = (corner2Ref.y + z) * tilesW + x + corner2Ref.x;

                        float height = tile.height;
                        int numAdjacentTiles = 1;

                        if (adjacent1Index > 0 && adjacent1Index < maxTiles && tiles[adjacent1Index] != null) {
                            numAdjacentTiles++;
                            height += tiles[adjacent1Index].height;
                        }
                        if (adjacent2Index > 0 && adjacent2Index < maxTiles && tiles[adjacent2Index] != null)
                        {
                            numAdjacentTiles++;
                            height += tiles[adjacent2Index].height;
                        }
                        height /= numAdjacentTiles;


                        p1.y = height;


                        int cornerIndexP1 = cornerIndex + 1;
                        if (cornerIndexP1 == 6)
                        {
                            corner1Ref = cornerref[0];
                            corner2Ref = cornerref[1];
                        }
                        else
                        {
                            corner1Ref = cornerref[cornerIndexP1 * 2];
                            corner2Ref = cornerref[cornerIndexP1 * 2 + 1];
                        }

                        adjacent1Index = (corner1Ref.y + z) * tilesW + x + corner1Ref.x;
                        adjacent2Index = (corner2Ref.y + z) * tilesW + x + corner2Ref.x;

                        height = tile.height;
                        numAdjacentTiles = 1;

                        if (adjacent1Index > 0 && adjacent1Index < maxTiles && tiles[adjacent1Index] != null)
                        {
                            numAdjacentTiles++;
                            height += tiles[adjacent1Index].height;
                        }
                        if (adjacent2Index > 0 && adjacent2Index < maxTiles && tiles[adjacent2Index] != null)
                        {
                            numAdjacentTiles++;
                            height += tiles[adjacent2Index].height;
                        }
                        height /= numAdjacentTiles;

                        p2.y = height;






                        vertices.Add(p0);
                        vertices.Add(p1);
                        vertices.Add(p2);
                        triangles.Add(vertexIndex++);
                        triangles.Add(vertexIndex++);
                        triangles.Add(vertexIndex++);


                        Color color = new Color(((int)tile.material) / 255f, 0, 0);
                        colours.Add(color);
                        colours.Add(color);
                        colours.Add(color);


                        // store info in the tile for later
                        tile.center = center;
                        tile.tilePosition = new Vector2Int(x, z);

                        tilesByTriangleIndex.Add(vertexIndex / 3, tile);
                    }

                }

            }





            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetColors(colours);
            mesh.RecalculateNormals();

            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = material;
            meshCollider.sharedMesh = mesh;            
        }

        private void InitializeTerrainTextureArray()
        {

            // uncomment to build 2d texture array
            /*Texture2D[] textures = Resources.LoadAll<Texture2D>("Materials/TextureArray/Terrain");

            Texture2DArray texArr = new Texture2DArray(1024, 1024, textures.Length, TextureFormat.RGBA32, false);
            for (int i = 0; i < textures.Length; i++)
            {
                texArr.SetPixels32(textures[i].GetPixels32(), i);
            }

            material = Resources.Load("Materials/Terrain") as Material;
            material.SetTexture("_TextureArray", texArr);

            AssetDatabase.CreateAsset(texArr, "Assets/Resources/Materials/TextureArray/Terrain/TerrainTextureArray.asset");
            */


            material = Resources.Load("Materials/Terrain") as Material;
        }










        Vector2Int[] cornerrefOdd = {
            new Vector2Int(-1,  1), new Vector2Int( 0,  1),
            new Vector2Int( 0,  1), new Vector2Int( 1,  0),
            new Vector2Int( 1,  0), new Vector2Int( 0, -1),
            new Vector2Int( 0, -1), new Vector2Int(-1, -1),
            new Vector2Int(-1, -1), new Vector2Int(-1,  0),
            new Vector2Int(-1,  0), new Vector2Int(-1,  1),
        };

        Vector2Int[] cornerrefEven = {
            new Vector2Int( 0,  1), new Vector2Int( 1,  1),
            new Vector2Int( 1,  1), new Vector2Int( 1,  0),
            new Vector2Int( 1,  0), new Vector2Int( 1, -1),
            new Vector2Int( 1, -1), new Vector2Int( 0, -1),
            new Vector2Int( 0, -1), new Vector2Int(-1,  0),
            new Vector2Int(-1,  0), new Vector2Int( 0,  1),
        };
    }

    public class HexTile
    {
        public float height = 0f;
        public MaterialEnum material;

        public float regrowGrass = 1f;
        public float regrowGrassRate = 0.025f;

        public Vector2Int tilePosition;
        public Vector3 center;

        public enum MaterialEnum
        {
            Dirt = 0,
            Grass = 1,
            Rock = 2,
            Empty = 4, // the shader is skipping 3, so we're using 4.
        };

        public float PathScore
        {
            get { return (material == MaterialEnum.Empty) ? 0f : 1f; }
        }
        public void EatGrass()
        {
            material = MaterialEnum.Dirt;
            regrowGrass = 1f;
        }
        public void ConvertToGrass()
        {
            material = MaterialEnum.Dirt;
            regrowGrass = 1f;
        }

        public void GrowGrass()
        {
            material = MaterialEnum.Grass;
        }
        public bool HasGrass
        {
            get
            {
                return material == MaterialEnum.Grass;
            }
        }
        public bool IsGrassTile
        {
            get
            {
                return material == MaterialEnum.Grass || material == MaterialEnum.Dirt;
            }
        }
        // used to determine whether it can be fertilized
        public bool CanConvertToGrassTile
        {
            get
            {
                return material == MaterialEnum.Rock;
            }
        }
        public bool IsEmpty
        {
            get
            {
                return material == MaterialEnum.Empty;
            }
        }
    }
}