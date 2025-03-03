using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x, y; // Grid position
    public bool isObstacle = false;
    public int cost = 0; // Pathfinding cost

    private Vector3 originalPosition;
    private Vector3 hoverPosition;
    private bool isHovered = false;
    public TextMeshPro costText; // Display cost
    private MeshRenderer meshRenderer;

    void Start()
    {
        originalPosition = transform.position;
        hoverPosition = originalPosition + Vector3.up * 0.2f;



        meshRenderer = GetComponent<MeshRenderer>();


    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && GridManager.instance.debugMode)
        {
            if (GridManager.instance.isPlayerMoving)
            {
                for (int i = 0; i < GridManager.instance.pathFinder.shortestPath.Count - 1; i++)
                {
                    GridManager.instance.pathFinder.shortestPath[i].HighlightPath();
                }
            }
            else if (GridManager.instance.startTile != null
                && GridManager.instance.goalTile != null
                && !GridManager.instance.isPlayerMoving)
            {
                PathFinder pathfinder = GridManager.instance.pathFinder;
                List<Tile> path = pathfinder.FindPath(GridManager.instance.startTile, GridManager.instance.goalTile);

                if (path != null)
                {
                    for (int i = 0; i < GridManager.instance.pathFinder.shortestPath.Count - 1; i++)
                    {
                        GridManager.instance.pathFinder.shortestPath[i].HighlightPath();
                    }
                    Debug.Log("Path found!");
                }
                else
                {
                    Debug.Log("No valid path found.");
                }
            }
        }
    }

    public void SetObstacle(bool obstacleStatus)
    {
        isObstacle = obstacleStatus;

        if (isObstacle)
        {
            GetComponent<Renderer>().material.color = Color.black; // Set obstacle color
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.white; // Normal tile color
        }
    }
    public void HighlightPath()
    {
        meshRenderer.material.color = Color.blue;
    }
    public void Setup(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void SetCost(int x, int y, int newCost)
    {
        cost = newCost;
        //costText.text = cost.ToString(); // Update text
        costText.text = $"P:({x},{y})\nCost:{cost}\nF:0 G:0 H:0";
    }

    public void ToggleDebugView(bool isDebug)
    {
        GridManager.instance.debugMode = isDebug;
        costText.gameObject.SetActive(isDebug);
        UpdateTileColor();
    }
    private void UpdateTileColor()
    {
        if (GridManager.instance.debugMode 
            && meshRenderer.material.color == Color.blue) return; // Keep blue tiles unchanged

        if (this == GridManager.instance.startTile && GridManager.instance.debugMode)
            meshRenderer.material.color = Color.green;  // Start Tile
        else if (this == GridManager.instance.goalTile && GridManager.instance.debugMode)
            meshRenderer.material.color = Color.red;  // Goal Tile
        else if (GridManager.instance.debugMode && !isObstacle)
            meshRenderer.material.color = Color.gray;  // Debug Mode On
        else if (isObstacle)
            meshRenderer.material.color = Color.black;
        else
            meshRenderer.material.color = Color.white;  // Default
    }
    void OnMouseEnter()
    {
        if (!isHovered)
        {
            transform.position = hoverPosition;
            isHovered = true;
        }
    }

    void OnMouseExit()
    {
        if (isHovered)
        {
            transform.position = originalPosition;
            isHovered = false;
        }
    }

    void OnMouseOver()
    {
        if (!GridManager.instance.debugMode || GridManager.instance.isPlayerMoving) return; // Only allow clicks in Debug Mode

        if (Input.GetMouseButtonDown(0)) // Left Click for Start Tile
        {
            if (this != GridManager.instance.goalTile) // Prevent same tile as Goal
            {
                if (GridManager.instance.startTile != null)
                    GridManager.instance.startTile.ResetTile(); // Clear previous Start Tile

                GridManager.instance.startTile = this;
                Debug.Log($"Start Tile set to: {x}, {y}");
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right Click for Goal Tile
        {
            if (this != GridManager.instance.startTile) // Prevent same tile as Start
            {
                if (GridManager.instance.goalTile != null)
                    GridManager.instance.goalTile.ResetTile(); // Clear previous Goal Tile

                GridManager.instance.goalTile = this;
                Debug.Log($"Goal Tile set to: {x}, {y}");
                RecalculateCosts();
            }
        }

        UpdateTileColor();
    }

    void ResetTile()
    {
        if (GridManager.instance.debugMode)
        {
            PathFinder pathFinder = GridManager.instance.pathFinder;
            if (GridManager.instance.pathFinder.shortestPath.Count > 0)
            {
                for (int i = 0; i < GridManager.instance.pathFinder.shortestPath.Count - 1; i++)
                {
                    GridManager.instance.pathFinder.shortestPath[i].GetComponent<MeshRenderer>().material.color = Color.gray;
                }
            }
            meshRenderer.material.color = Color.gray; // Reset color
        }
        else
        {
            meshRenderer.material.color = Color.white; // Reset color
        }
    }

    void RecalculateCosts()
    {
        if (GridManager.instance.goalTile == null) return;

        // Example: Manhattan Distance Calculation
        foreach (GameObject tileObj in GameObject.FindGameObjectsWithTag("Tile"))
        {
            Tile tile = tileObj.GetComponent<Tile>();
            int heuristicCost = Mathf.Abs(GridManager.instance.goalTile.x - tile.x) + Mathf.Abs(GridManager.instance.goalTile.y - tile.y);
            tile.SetCost(tile.x, tile.y, heuristicCost);
        }
    }
}
