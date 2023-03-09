using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class InvisibleMazeManager : ToyControls {

    public enum TileType {
        Safe,
        Danger
    }

    public class MazeTile {
        public bool exposed;
        public int bridge;
        public TileType type;

        public MazeTile(TileType type) {
            this.type = type;
            this.exposed = false;
            this.bridge = -1;
        }
    }

    public Tile dangerTile;
    public Tile safeTile;
    public Tile riskyTile;

    public Vector3Int mazeStartPosition;
    public int mazeWidth;
    public int mazeHeight;

    private Tilemap mazeMap;

    private MazeTile[,] maze;
    private int[,] backingMaze;
    


    // Start is called before the first frame update
    void Start() {
        GameObject mazeMapObj = GameObject.Find("MazeMap");
        mazeMap = mazeMapObj.GetComponent<Tilemap>();
        maze = GenerateMaze(mazeWidth, mazeHeight);
        UpdateMaze();
    }


    void UpdateMaze() {
        for (int x = 0; x < mazeWidth; x++) {
            for (int y = 0; y < mazeHeight; y++) {
                if (!maze[x, y].exposed) {
                    mazeMap.SetTile(new Vector3Int(x, y, 0) + mazeStartPosition, riskyTile);
                }
                else {
                    mazeMap.SetTile(new Vector3Int(x, y, 0) + mazeStartPosition, maze[x, y].type == TileType.Safe ? safeTile : dangerTile);
                }


            }
        }
    }

    public MazeTile VisitMazeTile(Vector3 worldPosition) {
        Vector3 cellPos = mazeMap.WorldToCell(worldPosition) - mazeStartPosition;
        //Debug.LogFormat("VisitMazeTile: {0}", cellPos);
        if(cellPos.x >= 0 && cellPos.x < maze.GetLength(0) && cellPos.y >= 0 && cellPos.y < maze.GetLength(1)) {
            MazeTile tile = maze[(int)cellPos.x, (int)cellPos.y];
            tile.exposed = true;
            UpdateMaze();
            return tile;
        }
        else {
            return null;
        }
        
    }

    public override void ToyControlGUI() {
        if (GUILayout.Button("Regenerate Maze")) {
            maze = GenerateMaze(mazeWidth, mazeHeight);
            UpdateMaze();
        }
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Expose Maze")) {
            foreach( MazeTile tile in maze) {
                tile.exposed = true;
            }
            UpdateMaze();
        }
        if (GUILayout.Button("Hide Maze")) {
            foreach(MazeTile tile in maze) {
                tile.exposed = false;
            }
            UpdateMaze();
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
    /// <returns></returns>
    private MazeTile[,] GenerateMaze(int width, int height) {
        MazeTile[,] maze = new MazeTile[width, height];
        HashSet<Vector2Int> visted = new HashSet<Vector2Int>();
        HashSet<Vector2Int> unvisted = new HashSet<Vector2Int>();

        //initialize the maze with all walls
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                maze[i, j] = new MazeTile(TileType.Danger);
                unvisted.Add(new Vector2Int(i, j));
            }
        }

        Vector2Int seed1 = new Vector2Int(Random.Range(0, width), 0);
        Vector2Int seed2 = new Vector2Int(Random.Range(0, width), height - 1);

        //add the first cell to the stack
        List<Vector2Int> stack = new List<Vector2Int> {seed1, seed2 };

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
        HashSet<Vector2Int> bridgeA = FloodFillMaze(maze, seed1);
        HashSet<Vector2Int> bridgeB = FloodFillMaze(maze, seed2);
        if(bridgeA.Intersect(bridgeB).Count() == 0) {
            Debug.Log("Bridges not connected, adding a fix");
            // find the closest cells and connect them
            Vector2Int closestA = bridgeA.ToList()[0];
            Vector2Int closestB = bridgeB.ToList()[0];
            foreach(Vector2Int a in bridgeA) {
                foreach(Vector2Int b in bridgeB) {
                    if(Vector2Int.Distance(a, b) < Vector2Int.Distance(closestA, closestB)) {
                        closestA = a;
                        closestB = b;
                    }
                }
            }
            Vector2Int min = Vector2Int.Min(closestA, closestB);
            Vector2Int max = Vector2Int.Max(closestA, closestB);
            for(int x = min.x; x <= max.x; x++) {
                maze[x, min.y].type = TileType.Safe;
            }
            for (int y = min.y; y <= max.y; y++) {
                maze[min.x, y].type = TileType.Safe;
            }

        }
        maze[seed1.x, seed1.y].exposed = true;
        maze[seed2.x, seed2.y].exposed = true;
        return maze;
    }

    /// <summary>
    /// This function was partially generated by Github Copilot
    /// </summary>
    /// <param name="maze"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    private HashSet<Vector2Int> FloodFillMaze(MazeTile[,] maze, Vector2Int seed) {
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
    

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            //get the mouse position in coordinates of the tilemap
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //get the tile at the mouse position
            Vector3Int tilePos = mazeMap.WorldToCell(mousePos);
            VisitMazeTile(tilePos);
            UpdateMaze();
        }
    }
}
