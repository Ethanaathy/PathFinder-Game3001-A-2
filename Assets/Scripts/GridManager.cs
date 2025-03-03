using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    public GameObject tilePrefab;
    public int gridSizeX = 10;
    public int gridSizeY = 10;
    public float tileSize = 1.1f;
    private Tile[,] grid;

    public PathFinder pathFinder;
    public GameObject startPanel;

    [HideInInspector]public Tile startTile = null;
    [HideInInspector] public Tile goalTile = null;
    [HideInInspector] public bool debugMode = false;

    public GameObject actorPrefab;  // Assign in the Inspector
    private GameObject actorInstance;

    [HideInInspector] public bool isPlayerMoving = false;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //Invoke(nameof(GenerateGrid), 1f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleDebugView();
        }
        if (Input.GetKeyDown(KeyCode.M) && debugMode)
        {
            pathFinder.shortestPath.Clear();
            if (pathFinder.shortestPath.Count > 0)
            {
                MoveActor();
            }
            else
            {
                Debug.Log("Else");
                if (startTile != null && goalTile != null)
                {
                    PathFinder pathfinder = pathFinder;
                    List<Tile> path = pathfinder.FindPath(startTile, goalTile);

                    MoveActor();
                }

            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            debugMode = false;
            startTile = null;
            goalTile = null;
            pathFinder.shortestPath.Clear();
            if (actorInstance != null)
            {
                GameObject objToDestroy = actorInstance;
                actorInstance = null;
                Destroy(objToDestroy);
            }
            foreach(Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            Invoke(nameof(GenerateGrid), 0.5f);
            //GenerateGrid();
        }


    }

    public void GenerateGrid()
    {
        startPanel.SetActive(false);
        grid = new Tile[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 tilePosition = new Vector3(x * tileSize, 0, y * tileSize);
                GameObject newTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity);
                newTile.name = $"Tile_{x}_{y}";
                newTile.transform.parent = transform;

                Tile tileComponent = newTile.GetComponent<Tile>();
                tileComponent.Setup(x, y);
                tileComponent.SetCost(x, y, Random.Range(1, 10)); // Placeholder cost for testing

                bool isObstacle = Random.value < 0.2f; // Randomly set some tiles as obstacles (e.g., 20% chance)
                tileComponent.SetObstacle(isObstacle);
                grid[x, y] = tileComponent; //  No redundant GetComponent call
            }
        }

        pathFinder.InitializeGrid(grid);
    }

    void ToggleDebugView()
    {
        debugMode = !debugMode;
        Debug.Log("Debug Mode: " + debugMode);

        foreach (Tile tile in grid)
        {
            tile.ToggleDebugView(debugMode);
        }
    }
    void MoveActor()
    {
        if (pathFinder.shortestPath == null || pathFinder.shortestPath.Count == 0)
        {
            Debug.LogWarning("No valid path found!");
            return;
        }

        if (actorInstance == null)
        {
            // Instantiate the actor at the Start Tile
            actorInstance = Instantiate(actorPrefab, pathFinder.shortestPath[0].transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        // Start moving the actor along the path
        StartCoroutine(MoveAlongPath(pathFinder.shortestPath));
    }

    IEnumerator MoveAlongPath(List<Tile> path)
    {
        isPlayerMoving = true;
        foreach (Tile tile in path)
        {
            SoundManager.instance.PlaySFX("Moving");
            Vector3 targetPosition = tile.transform.position + Vector3.up * 0.5f;

            while (Vector3.Distance(actorInstance.transform.position, targetPosition) > 0.05f)
            {
                actorInstance.transform.position = Vector3.MoveTowards(actorInstance.transform.position, targetPosition, Time.deltaTime * 3);
                yield return null;
            }
        }
        isPlayerMoving = false;
        Debug.Log("Actor reached the goal!");
    }
}
