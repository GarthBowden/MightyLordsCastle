using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGrid : MonoBehaviour
{
    public static WorldGrid Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Duplicate " + this + " detected. Deleting myself!");
            Destroy(this);
        }
    }
    public static void DeleteMap()
    {
        if(Instance is null)
        {
            Debug.Log("Delete Map called with null Instance");
        }
        else
        {
            Destroy(Instance);
            Instance = null; // may not be necessary...
        }
    }

    [SerializeField] float _GridSize;
    public float gridSize
    {
        get { return _GridSize; }
    }

    public Vector3 GridToUnityVector3 (int x, int y)
    {
        Vector3 w = new Vector3
        {
            x = (float)x * _GridSize,
            z = (float)y * _GridSize,
            y = 0.0f
        };
        return w;
    }
    public Vector3 GridToWorldVector3(WorldPlaceableObject.WorldGridPosition wgp)
    {
        return GridToUnityVector3(wgp.x, wgp.y);
    }

    public Vector2 UnityVector3ToGrid(Vector3 position)
    {
        float x_approx = position.x / _GridSize;
        float y_approx = position.z / _GridSize;
        return new Vector2
        {
            x = Mathf.RoundToInt(x_approx),
            y = Mathf.RoundToInt(y_approx)
        };
    }

    class TilePlace
    {
        //Whether a path has a wall
        public bool[] path;
        //Whether a path is allowed -- i.e. not facing edge of map
        public bool[] nonEdge;
        public bool isVisited;
        //Count of open paths
        private int _openpaths;
        public int openpaths
        {
            get => _openpaths;
        }
        private int _maxopenpaths;
        public int maxopenpaths
        {
            get => _maxopenpaths;
        }
        public TilePlace()
        {
            path = new bool[4];
            nonEdge = new bool[] { true, true, true, true };
            _openpaths = 0;
            _maxopenpaths = 4;
        }
        public void OpenPath(int pathnumber)
        {
            if(!path[pathnumber] && nonEdge[pathnumber])
            {
                _openpaths++;
                path[pathnumber] = true;
            }
            else
            {
                Debug.Log("attempted to open path that was already open or blocked " + pathnumber + " " + _openpaths);
            }
        }
        public void BlockPath(int pathnumber)
        {
            if (!path[pathnumber] && nonEdge[pathnumber])
            {
                _maxopenpaths--;
                nonEdge[pathnumber] = false;
            }
        }
        public int GetOpenPath(int StartingPath)
        {
            for (int i = 0; i < 4; i++)
            {
                int j = (StartingPath + i) % 4;
                if (path[j])
                {
                    return j;
                }
            }
            Debug.Log("GetOpenPath failed to find an open path!");
            return -1;
        }

    }
    TilePlace[,] placeMap;
    [SerializeField]
    public Vector2Int MapDimensions;
    [SerializeField]
    public int MapSeed;

    public void GenerateEmptyMap()
    {
        if(!(MapDimensions.x > 0 && MapDimensions.y > 0))
        {
            Debug.Log("Invalid dimensions: " + MapDimensions);
        }
        placeMap = new TilePlace[MapDimensions.x, MapDimensions.y];
        for (int i = 0; i < MapDimensions.x; i++)
        {
            for (int j = 0; j < MapDimensions.y; j++)
            {
                placeMap[i, j] = new TilePlace();
                //Block map edges
                if(i == 0)
                {
                    placeMap[i, j].BlockPath(3);
                }
                else if(i == MapDimensions.x - 1)
                {
                    placeMap[i, j].BlockPath(1);
                }
                if (j == 0)
                {
                    placeMap[i, j].BlockPath(0);
                }
                else if (j == MapDimensions.y - 1)
                {
                    placeMap[i, j].BlockPath(2);
                }
            }
        }
    }
    // There might be benefits to refactoring using a class for each edge (ie node) instead of places with open or closed walls
    // However since the actual maze tiles are going to be from a set of squares with predefined edge possibilities,
    // it seems like it would introduce another level of complexity to translate from edges to appropriate tiles.
    class Passage
    {
        public bool IsOpen;
        Vector2Int[] nodes;
        public Passage()
        {
            nodes = new Vector2Int[2];
        }
        public Passage(Vector2Int node1, Vector2Int node2)
        {
            nodes = new Vector2Int[] {node1, node2 };
        }
        public Vector2Int[] GetNodes()
        {
            return nodes;
        }
    }
    class potentialPathPlace : System.IEquatable<potentialPathPlace>
    {
        public Vector2Int place;
        public int connectionDirection;
        public potentialPathPlace()
        {
            place = new Vector2Int();
        }
        public potentialPathPlace(Vector2Int p, int c)
        {
            place = p;
            connectionDirection = c;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as potentialPathPlace);
        }
        // This equality comparison ignores differences in connection direction
        public bool Equals(potentialPathPlace pl)
        {
            if (ReferenceEquals(this, pl)) // same object
            {
                return true;
            }
            else if (pl is null)
            {
                return false;
            }
            else
            {
                return (place == pl.place);
            }
        }
        public override int GetHashCode()
        {
            return place.GetHashCode();
        }
        public static bool operator ==(potentialPathPlace a, potentialPathPlace b)
        {
            if (a is null)
            {
                if (b is null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return a.Equals(b);
            }
        }
        public static bool operator !=(potentialPathPlace a, potentialPathPlace b)
        {
            return !(a == b);
        }

    }
    Vector2Int GetAdjacentPlace(Vector2Int CurrentPlace, int direction)
    {
        // Directional math
        if (direction == 0) // south
        {
            return CurrentPlace + Vector2Int.down;
        }
        else if (direction == 1) // east
        {
            return CurrentPlace + Vector2Int.right;
        }
        else if (direction == 2) // north
        {
            return CurrentPlace + Vector2Int.up;
        }
        else if (direction == 3) // west
        {
            return CurrentPlace + Vector2Int.left;
        }
        else
        {
            Debug.Log("Current place was passed invalid direction: " + direction);
            return Vector2Int.zero;
        }
    }
    List<potentialPathPlace> potentialPathPlaces;
    public void FindPotentialPaths(Vector2Int CurrentPlace)
    {
        for (int i = 0; i < 4; i++)
        {
            //Test not edge of map and not yet opened
            Vector2Int neighborPlace = new Vector2Int();
            TilePlace CurrentTile = placeMap[CurrentPlace.x, CurrentPlace.y];
            if (CurrentTile.nonEdge[i] && !CurrentTile.path[i])
            {
                neighborPlace = GetAdjacentPlace(CurrentPlace, i);
                //Add to list of potentially openable paths IF not already visited
                if (!placeMap[neighborPlace.x, neighborPlace.y].isVisited)
                {
                    potentialPathPlaces.Add(new potentialPathPlace(neighborPlace, (i + 2) % 4));
                }
            }
        }
    }
    public Vector2Int PickPath()
    {
        int i = Random.Range(0, potentialPathPlaces.Count);
        Vector2Int NewCurrentPlace = potentialPathPlaces[i].place;
        placeMap[NewCurrentPlace.x, NewCurrentPlace.y].OpenPath(potentialPathPlaces[i].connectionDirection);
        placeMap[NewCurrentPlace.x, NewCurrentPlace.y].isVisited = true;
        Vector2Int ConnectedPlace = GetAdjacentPlace(NewCurrentPlace, potentialPathPlaces[i].connectionDirection);
        placeMap[ConnectedPlace.x, ConnectedPlace.y].OpenPath((potentialPathPlaces[i].connectionDirection + 2) % 4);
        potentialPathPlace p = potentialPathPlaces[i];
        while (potentialPathPlaces.Remove(p)) ;

        return NewCurrentPlace;
    }
    public void SimpleMazeGen(Vector2Int StartingPlace)
    {
        if(MapSeed != 0)
        {
            Random.InitState(MapSeed);
        }
        potentialPathPlaces = new List<potentialPathPlace>();
        List<Vector2Int> openPathPlaces = new List<Vector2Int>();
        openPathPlaces.Add(StartingPlace);

        Vector2Int CurrentPlace = StartingPlace;
        FindPotentialPaths(StartingPlace);
        while (potentialPathPlaces.Count > 0)
        {
            CurrentPlace = PickPath();
            FindPotentialPaths(CurrentPlace);
        }
    }


    [SerializeField]
    public List<GameObject> mapTilePrefabs;

    Dictionary<Tile.TileType, int[]> prefabVariants = new Dictionary<Tile.TileType, int[]>();

    GameObject SpawnSingleTile(Tile.TileType ttype)
    {
        int[] i = prefabVariants[ttype];
        int j = i[Random.Range(0, i.Length)];
        return Instantiate<GameObject>(mapTilePrefabs[j]);
    }
    void AlignTileOnMap(GameObject tileObject, int orientingpath, int x, int y)
    {
        tileObject.transform.Rotate(0.0f, (float)orientingpath * -90.0f, 0.0f);
        tileObject.transform.localScale *= gridSize;
        tileObject.transform.Translate(new Vector3((float)x * gridSize, 0.0f, (float)y * gridSize), Space.World);
    }

    [SerializeField]
    public Vector2Int EndTile;

    int[,] tileNumbers;
    int CountConnected (int count, Vector2Int currentPlace)
    {
        TilePlace t = placeMap[currentPlace.x, currentPlace.y];
        for (int i = 0; i < 4; i++)
        {
            if(t.path[i])
            {
                Vector2Int p = GetAdjacentPlace(currentPlace, i);
                TilePlace q = placeMap[p.x, p.y];
                if(!q.isVisited)
                {
                    q.isVisited = true;
                    tileNumbers[p.x, p.y] += CountConnected(count + 1, p);
                }
            }
        }
        return count;
    }
    public void GenerateDistances(Vector2Int startPlace)
    {
        //forks for every multiply connected node
        //each path returns on dead end
        //Adds to the tile numbers array. It must be initialized before calling this method
        //tile visited values must be set to false
        foreach (TilePlace t in placeMap)
        {
            t.isVisited = false;
        }
        placeMap[startPlace.x, startPlace.y].isVisited = true;
        tileNumbers[startPlace.x, startPlace.y] = CountConnected(0, startPlace);
    }
    public void SpawnTiles()
    {
        Debug.Log("Spawning tiles with grid size: " + gridSize.ToString());
        //Generate list of variants from prefabs so that they can be randomly spawned
        for (int i = 0; i < System.Enum.GetNames(typeof(Tile.TileType)).Length; i++)
        {
            List<int> matchingIndices = new List<int>();
            for (int j = 0; j < mapTilePrefabs.Count; j++)
            {
                Tile t = mapTilePrefabs[j].GetComponent<Tile>();
                if (t is null)
                {
                    Debug.Log("Tile " + mapTilePrefabs[j].name + " is missing a tile component. Tiles not spawned!");
                    return;
                }
                if (t.ttype == (Tile.TileType)i)
                {
                    matchingIndices.Add(j);
                }
            }
            Debug.Log(System.Enum.GetNames(typeof(Tile.TileType))[i] + ": " + matchingIndices.Count);
            prefabVariants.Add((Tile.TileType)i, matchingIndices.ToArray());
        }
        for (int i = 0; i < MapDimensions.x; i++)
        {
            for (int j = 0; j < MapDimensions.y; j++)
            {
                //calculate the proper prefab to spawn based on the data from placemap
                TilePlace t = placeMap[i, j];
                if(t.openpaths == 0)
                {
                    AlignTileOnMap(SpawnSingleTile(Tile.TileType.Closed), 0, i, j);
                }
                else if(t.openpaths == 1)
                {
                    int path1 = t.GetOpenPath(0);
                    AlignTileOnMap(SpawnSingleTile(Tile.TileType.DeadEnd), path1, i, j);
                }
                else if(t.openpaths == 2)
                {
                    int path1 = t.GetOpenPath(0);
                    int path2 = t.GetOpenPath(path1 + 1);
                    if(path2 - path1 == 2)
                    {
                        AlignTileOnMap(SpawnSingleTile(Tile.TileType.Passage), path1, i, j);
                    }
                    else
                    {
                        if(path2 - path1 == 1)
                        {
                            AlignTileOnMap(SpawnSingleTile(Tile.TileType.Corner), path1, i, j);
                        }
                        else
                        {
                            AlignTileOnMap(SpawnSingleTile(Tile.TileType.Corner), path2, i, j);
                        }
                    }
                }
                else if(t.openpaths == 3)
                {
                    int path1 = t.GetOpenPath(0);
                    int path2 = t.GetOpenPath(path1 + 1);
                    int path3 = t.GetOpenPath(path2 + 1);
                    AlignTileOnMap(SpawnSingleTile(Tile.TileType.OneWall), (7 - path1 - path2 - path3) % 4, i, j);
                }
                else if(t.openpaths == 4)
                {
                    AlignTileOnMap(SpawnSingleTile(Tile.TileType.Open), 0, i, j);
                }
            }
        }
    }
    private void Start()
    {
        GenerateEmptyMap();
        SimpleMazeGen(new Vector2Int(0,0));
        tileNumbers = new int[MapDimensions.x, MapDimensions.y];
        SpawnTiles();
         
    }

    /* TBD - Calculation for side passages:
     * 1. count from starting point up the number of tiles traversed, so that every tile is numbered by distance from starting point
     * 2. Repeat this process for the end point. This forms a path pair
     * 3. The number of each tile along the connecting path for these two points will be constant
     * 4. The numbers of side passages will be higher.
     * 5. Trimming and other types of maze enhancement can then be done by either focusing on main passage or side passages using this generated tile numbering.
    */
}
