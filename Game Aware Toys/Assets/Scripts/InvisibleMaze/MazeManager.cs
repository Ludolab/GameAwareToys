using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

namespace InvisibleMaze {
    public enum TileType {
        Safe,
        Danger
    }

    public enum GemColors {
        Blue,
        Green,
        Purple,
        Yellow
    }

    public enum Direction {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
        Center = 4
    }

    public class MazeManager : ToyControls {

        private static MazeManager _instance;
        public static MazeManager Instance {
            get { return _instance; }
            private set { _instance = value; }
        }

        [Header("Maze Settings")]
        public Vector3Int mazePosition;
        public int nearGemSpawnY;
        public int farGemSpawnY;
        public int mazeWidth;
        public int mazeHeight;

        [Header("References")]
        public Tile dangerTile;
        public Tile safeTile;
        public Tile riskyTile;

        public Tile itemStand;
        public GameObject[] gemPrefabs;

        public Vector3Int leftDoorPosition;
        public Vector3Int rightDoorPosition;
        public Tile closedDoorLeft;
        public Tile closedDoorRight;
        public Tile openDoorLeft;
        public Tile openDoorRight;


        private Tilemap mazeMap;
        private Tilemap decorationMap;
        private Grid grid;
        private MazeTile[,] maze;
        private PedestalBehavior[] pedestals;

        // Start is called before the first frame update
        void Start() {
            Instance = this;
            mazeMap = GameObject.Find("MazeMap").GetComponent<Tilemap>();
            decorationMap = GameObject.Find("Decorations").GetComponent<Tilemap>();
            grid = GameObject.Find("Grid").GetComponent<Grid>();           

            //shuffle colors
            gemPrefabs = gemPrefabs.OrderBy(x => Random.value).ToArray();

            //determine near and far gem spawn points
            Vector2Int nearGemPos = new Vector2Int(Random.Range(2, mazeWidth - 2), nearGemSpawnY);
            Vector2Int farGemPos = new Vector2Int(Random.Range(2, mazeWidth - 2), farGemSpawnY);

            //generate 2 random gem positions within the maze
            List<Vector2Int> gemSeeds = new List<Vector2Int> {
                new Vector2Int(Random.Range(2,mazeWidth-2), Random.Range(2, mazeHeight - 2)),
                new Vector2Int(Random.Range(2,mazeWidth-2), Random.Range(2, mazeHeight - 2)),
            };

            while (gemSeeds[0] == gemSeeds[1]) {
                gemSeeds[1] = new Vector2Int(Random.Range(2, mazeWidth - 2), Random.Range(2, mazeHeight - 2));
            }

            //pass the random positions into the maze generation as additional seeds
            maze = GenerateMaze(mazeWidth, mazeHeight, gemSeeds);
            UpdateMazeTiles();

            //put item stands at each of the spawn points
            decorationMap.SetTile((Vector3Int)nearGemPos + mazePosition, itemStand);
            decorationMap.SetTile((Vector3Int)farGemPos + mazePosition, itemStand);
            foreach(Vector2Int pos in gemSeeds) {
                decorationMap.SetTile((Vector3Int)pos + mazePosition, itemStand);
            }

            List<GemBehavior> gems = new List<GemBehavior> {
                Instantiate(gemPrefabs[0], grid.CellToWorld((Vector3Int)nearGemPos + mazePosition) + mazeMap.tileAnchor, Quaternion.identity).GetComponent<GemBehavior>(),
                Instantiate(gemPrefabs[1], grid.CellToWorld((Vector3Int)gemSeeds[0] + mazePosition) + mazeMap.tileAnchor, Quaternion.identity).GetComponent<GemBehavior>(),
                Instantiate(gemPrefabs[2], grid.CellToWorld((Vector3Int) gemSeeds[1] + mazePosition) + mazeMap.tileAnchor, Quaternion.identity).GetComponent<GemBehavior>(),
                Instantiate(gemPrefabs[3], grid.CellToWorld((Vector3Int) farGemPos + mazePosition) + mazeMap.tileAnchor, Quaternion.identity).GetComponent<GemBehavior>()
            };

            pedestals = FindObjectsOfType<PedestalBehavior>();

            pedestals = pedestals.OrderBy(x => Random.value).ToArray();

            for (int i = 0; i < 4; i++) {
                try {
                    pedestals[i].targetGem = gems[i];
                }
                catch (System.IndexOutOfRangeException e) {
                    Debug.LogErrorFormat("Tried to get index:{0}", i);
                    throw e;
                }

            }

            //actually instantiate the gems
            //collect the pedestals
            //randomly assign the colors to the pedestals
        }

        // function check win
        // if win swap the door tiles to open, maybe instatiate a prefab?
        // once player walks through open doors reload scene

        void CheckWin() {
            bool win = true;
            foreach (PedestalBehavior pedestal in pedestals) {
                if (pedestal.targetGem != pedestal.slottedGem) {
                    win = false;
                    break;
                }
            }
            if(win) {
                decorationMap.SetTile(leftDoorPosition, openDoorLeft);
                decorationMap.SetTile(rightDoorPosition, openDoorRight);
            }
            else {
                decorationMap.SetTile(leftDoorPosition, closedDoorLeft);
                decorationMap.SetTile(rightDoorPosition, closedDoorRight);
            }
        }


        void UpdateMazeTiles() {
            for (int x = 0; x < mazeWidth; x++) {
                for (int y = 0; y < mazeHeight; y++) {
                    if (!maze[x, y].exposed) {
                        mazeMap.SetTile(new Vector3Int(x, y, 0) + mazePosition, riskyTile);
                    }
                    else {
                        mazeMap.SetTile(new Vector3Int(x, y, 0) + mazePosition, maze[x, y].type == TileType.Safe ? safeTile : dangerTile);
                    }
                }
            }
        }

        public MazeTile VisitMazeTile(Vector3 worldPosition) {
            Vector3 cellPos = mazeMap.WorldToCell(worldPosition) - mazePosition;
            if (cellPos.x >= 0 && cellPos.x < maze.GetLength(0) && cellPos.y >= 0 && cellPos.y < maze.GetLength(1)) {
                MazeTile tile = maze[(int)cellPos.x, (int)cellPos.y];
                tile.exposed = true;
                UpdateMazeTiles();
                return tile;
            }
            else {
                return null;
            }
        }


        public MazeTile CheckTile(Vector3 worldPosition) {
            Vector3 cellPos = mazeMap.WorldToCell(worldPosition) - mazePosition;
            if (cellPos.x >= 0 && cellPos.x < maze.GetLength(0) && cellPos.y >= 0 && cellPos.y < maze.GetLength(1)) {
                return maze[(int)cellPos.x, (int)cellPos.y];
            }
            else {
                return null;
            }
        }

        public override void ToyControlGUI() {
           /* if (GUILayout.Button("Regenerate Maze")) {
                maze = GenerateMaze(mazeWidth, mazeHeight);
                UpdateMazeTiles();
            }*/
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Expose Maze")) {
                foreach (MazeTile tile in maze) {
                    tile.exposed = true;
                }
                UpdateMazeTiles();
            }
            if (GUILayout.Button("Hide Maze")) {
                foreach (MazeTile tile in maze) {
                    tile.exposed = false;
                }
                UpdateMazeTiles();
            }

            GUILayout.EndHorizontal();

            GUILayout.Label("Backing Maze");
            GUIPrintMaze(maze);
        }

        void GUIPrintMaze(MazeTile[,] maze) {
            for (int y = 0; y < mazeHeight; y++) {
                string row = "";
                for (int x = 0; x < mazeWidth; x++) {
                    row += maze[x, y].type == TileType.Safe ? '#' : '=';
                }
                GUILayout.Label(row);
            }
        }

        public Bounds GetTileBounds(Vector2Int pos) {
            Vector3Int pos3 = new Vector3Int(pos.x, pos.y, 0) + mazePosition;
            Bounds bound = grid.GetBoundsLocal(pos3);
            Vector3 worldPost = grid.GetCellCenterWorld(pos3);
            return new Bounds(worldPost, bound.size);
        }

        /// <summary>
        /// This GenerateMaze function was synthesizd by Github Copilot using the prompt:
        /// "generate a maze of the given width and height where walls are 1 and open spaces are 0"
        /// 
        /// The generated code did not actually make a maze becuase it lacked the break in the neighbors loop so 
        /// it was modified to randomly walk better and seed the search with a start and endpoint that would 
        /// gaurantee a connected path across the maze. The mazes are *usually* interesting but some times simplistic
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="additionalSeeds"></param>
        /// <returns></returns>
        private MazeTile[,] GenerateMaze(int width, int height, List<Vector2Int> additionalSeeds) {
            MazeTile[,] maze = new MazeTile[width, height];
            HashSet<Vector2Int> visted = new HashSet<Vector2Int>();
            HashSet<Vector2Int> unvisted = new HashSet<Vector2Int>();

            //initialize the maze with all walls
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    var pos = new Vector2Int(i, j);
                    maze[i, j] = new MazeTile(TileType.Danger, pos);
                    unvisted.Add(pos);
                }
            }

            Vector2Int seed1 = new Vector2Int(Random.Range(0, width), 0);
            Vector2Int seed2 = new Vector2Int(Random.Range(0, width), height - 1);

            //add the first cell to the stack
            List<Vector2Int> stack = new List<Vector2Int> { seed1, seed2 };
            stack.AddRange(additionalSeeds);

            //while the stack is not empty
            while (stack.Count > 0) {
                //pick a cell, mark it as part of the maze and add it to the stack
                int randDex = Random.Range(0, stack.Count - 1);
                Vector2Int current = stack[randDex];
                visted.Add(current);
                unvisted.Remove(current);
                maze[current.x, current.y].type = TileType.Safe;
                stack.RemoveAt(randDex);

                //get the neighboring cells
                List<Vector2Int> neighbors = new List<Vector2Int>();
                if (current.x - 2 > 0) {
                    neighbors.Add(new Vector2Int(current.x - 2, current.y));
                }
                if (current.x + 2 < width - 1) {
                    neighbors.Add(new Vector2Int(current.x + 2, current.y));
                }
                if (current.y - 2 > 0) {
                    neighbors.Add(new Vector2Int(current.x, current.y - 2));
                }
                if (current.y + 2 < height - 1) {
                    neighbors.Add(new Vector2Int(current.x, current.y + 2));
                }

                neighbors = neighbors.OrderBy(x => Random.value).ToList();

                //for each neighboring cell
                foreach (Vector2Int neighbor in neighbors) {
                    //if the neighboring cell has not been visited
                    if (maze[neighbor.x, neighbor.y].type == TileType.Danger && !visted.Contains(neighbor)) {
                        //push the neighboring cell to the stack
                        stack.Add(neighbor);
                        //remove the wall between the current cell and the neighboring cell
                        maze[(current.x + neighbor.x) / 2, (current.y + neighbor.y) / 2].type = TileType.Safe;
                        break;
                    }
                }
            }

            //flood fill the maze to test is seed1 is connected to seed2
            HashSet<Vector2Int> bridgeA = FloodFillBridge(maze, seed1);
            HashSet<Vector2Int> bridgeB = FloodFillBridge(maze, seed2);
            bridgeA = ConnectBridges(bridgeA, bridgeB, maze);
            maze[seed1.x, seed1.y].exposed = true;
            maze[seed2.x, seed2.y].exposed = true;


            foreach (Vector2Int seed in additionalSeeds) {
                HashSet<Vector2Int> seedBridge = FloodFillBridge(maze, seed);
                bridgeA = ConnectBridges(bridgeA, seedBridge, maze);
                maze[seed.x, seed.y].exposed = true;
            }
            

            return maze;
        }

        /// <summary>
        /// This function was partially generated by Github Copilot
        /// </summary>
        /// <param name="maze"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        private HashSet<Vector2Int> FloodFillBridge(MazeTile[,] maze, Vector2Int seed) {
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            Queue<Vector2Int> toVisit = new Queue<Vector2Int>();
            toVisit.Enqueue(seed);

            while (toVisit.Count > 0) {
                Vector2Int current = toVisit.Dequeue();
                visited.Add(current);

                List<Vector2Int> neighbors = new List<Vector2Int>();
                if (current.x - 1 > 0) {
                    neighbors.Add(new Vector2Int(current.x - 1, current.y));
                }
                if (current.x + 1 < maze.GetLength(0) - 1) {
                    neighbors.Add(new Vector2Int(current.x + 1, current.y));
                }
                if (current.y - 1 > 0) {
                    neighbors.Add(new Vector2Int(current.x, current.y - 1));
                }
                if (current.y + 1 < maze.GetLength(1) - 1) {
                    neighbors.Add(new Vector2Int(current.x, current.y + 1));
                }

                foreach (Vector2Int neighbor in neighbors) {
                    if (maze[neighbor.x, neighbor.y].type == TileType.Safe && !visited.Contains(neighbor)) {
                        toVisit.Enqueue(neighbor);
                    }
                }
            }

            return visited;
        }

        public HashSet<Vector2Int> ConnectBridges(HashSet<Vector2Int> bridgeA, HashSet<Vector2Int> bridgeB, MazeTile[,] maze) {
            if (bridgeA.Intersect(bridgeB).Count() == 0) {
                Debug.Log("Bridges not connected, adding a fix");
                // find the closest cells and connect them
                Vector2Int closestA = bridgeA.ToList()[0];
                Vector2Int closestB = bridgeB.ToList()[0];
                foreach (Vector2Int a in bridgeA) {
                    foreach (Vector2Int b in bridgeB) {
                        if (Vector2Int.Distance(a, b) < Vector2Int.Distance(closestA, closestB)) {
                            closestA = a;
                            closestB = b;
                        }
                    }
                }
                Vector2Int min = Vector2Int.Min(closestA, closestB);
                Vector2Int max = Vector2Int.Max(closestA, closestB);
                for (int x = min.x; x <= max.x; x++) {
                    maze[x, min.y].type = TileType.Safe;
                }
                for (int y = min.y; y <= max.y; y++) {
                    maze[min.x, y].type = TileType.Safe;
                }
            }
            return new HashSet<Vector2Int>(bridgeA.Union(bridgeB));
        }


        // Update is called once per frame
        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                //get the mouse position in coordinates of the tilemap
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                //get the tile at the mouse position
                Vector3Int tilePos = mazeMap.WorldToCell(mousePos);

                /*Bounds bound = grid.GetBoundsLocal(tilePos);
                Vector3 worldPost = grid.GetCellCenterWorld(tilePos);

                Bounds aggregateBounds = new Bounds(worldPost, bound.size);

                Debug.LogFormat("mousePos:{0}, tilePos:{1}, bounds:{2}, worldPos:{3}, aggBounds:{4}", mousePos, tilePos, bound, worldPost, aggregateBounds);*/

                VisitMazeTile(tilePos);
                UpdateMazeTiles();
            }
        }
    }
}