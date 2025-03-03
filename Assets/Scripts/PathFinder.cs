using System.Collections.Generic;
using UnityEngine;
public class PathFinder : MonoBehaviour
{
    private Tile[,] grid;
    private List<Tile> openList;
    private HashSet<Tile> closedList;
    private Dictionary<Tile, Tile> cameFrom;
    private Dictionary<Tile, int> gCost;
    private Dictionary<Tile, int> fCost;

    public List<Tile> shortestPath = new List<Tile>(); // ✅ Store the shortest path

    public void InitializeGrid(Tile[,] gridTiles)
    {
        grid = gridTiles;
    }

   
    public List<Tile> FindPath(Tile startTile, Tile goalTile)
    {
        if (startTile == null || goalTile == null) return null;

        shortestPath.Clear(); // ✅ Clear previous path
        openList = new List<Tile> { startTile };
        closedList = new HashSet<Tile>();
        cameFrom = new Dictionary<Tile, Tile>();
        gCost = new Dictionary<Tile, int>();
        fCost = new Dictionary<Tile, int>();

        foreach (Tile tile in grid)
        {
            gCost[tile] = int.MaxValue;
            fCost[tile] = int.MaxValue;
        }

        gCost[startTile] = 0;
        fCost[startTile] = GetHeuristic(startTile, goalTile);

        while (openList.Count > 0)
        {
            Tile current = GetLowestFCostTile(openList);
            if (current == goalTile)
            {
                shortestPath = ReconstructPath(cameFrom, goalTile); // ✅ Store the shortest path
                return shortestPath;
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach (Tile neighbor in GetNeighbors(current))
            {
                if (closedList.Contains(neighbor) || neighbor.isObstacle) continue;

                int tentativeGCost = gCost[current] + 1;
                if (tentativeGCost < gCost[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gCost[neighbor] = tentativeGCost;
                    fCost[neighbor] = gCost[neighbor] + GetHeuristic(neighbor, goalTile);

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }
    private int GetHeuristic(Tile a, Tile b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan Distance
    }

    private Tile GetLowestFCostTile(List<Tile> list)
    {
        Tile lowest = list[0];
        foreach (Tile tile in list)
        {
            if (fCost[tile] < fCost[lowest])
                lowest = tile;
        }
        return lowest;
    }

    
    private List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();
        List<int[]> directions = new List<int[]>
        {
            new int[] { 0, 1 },  // Up
            new int[] { 1, 0 },  // Right
            new int[] { 0, -1 }, // Down
            new int[] { -1, 0 }  // Left
        };
        foreach (var dir in directions)
        {
            int newX = tile.x + dir[0];
            int newY = tile.y + dir[1];

            if (newX >= 0 && newX < grid.GetLength(0) && newY >= 0 && newY < grid.GetLength(1))
            {
                Tile neighbor = grid[newX, newY];

                if (!neighbor.isObstacle) //  Ignore obstacle tiles
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    private List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
    {
        List<Tile> path = new List<Tile>();
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }
}
