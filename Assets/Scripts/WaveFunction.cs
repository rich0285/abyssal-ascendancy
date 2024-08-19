using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    public int maxRandomWalkMoves = 100; // Define the size of your map grid

    private readonly List<GameObject> _tilePrefabs = new List<GameObject>();

    [NotNull] private readonly Dictionary<string, List<GameObject>> _sortedTiles = new Dictionary<string, List<GameObject>>()
    {
        { "up", new List<GameObject>() },
        { "down", new List<GameObject>() },
        { "left", new List<GameObject>() },
        { "right", new List<GameObject>() }
    };

    Dictionary<Vector2Int, GameObject> placedTiles = new Dictionary<Vector2Int, GameObject>();

    void Awake()
    {
        LoadTiles();
        GenerateMapWithRandomWalk(maxRandomWalkMoves);
    }

    void LoadTiles()
    {
        // Load all prefabs from the Room Structures folder
        GameObject[] loadedTilePrefabs = Resources.LoadAll<GameObject>("Prefabs/Corridor");

        // Add loaded prefabs to the list
        _tilePrefabs!.AddRange(loadedTilePrefabs);
        // Sort Tiles
        SortTilePrefabs(loadedTilePrefabs);

        Debug.Log($"Loaded {_tilePrefabs.Count} tile prefabs.");
    }


    void GenerateMapWithRandomWalk(int maxMoves)
    {
        RandomWalk(maxMoves);
    }


    private void SortTilePrefabs(GameObject[] loadedTilePrefabs)
    {
        foreach (GameObject tilePrefab in loadedTilePrefabs)
        {
            var tileScript = tilePrefab.GetComponent<Tile>();
            if (tileScript == null)
            {
                Debug.LogError("Something is wrong since the Prefab here doesn't have the Tile component");
                continue; // Skip this prefab since it doesn't have a Tile component
            }

            // Add the tilePrefab to the corresponding list based on the connection
            if (tileScript.connectsUp)
                _sortedTiles["up"].Add(tilePrefab);
        
            if (tileScript.connectsDown)
                _sortedTiles["down"].Add(tilePrefab);
        
            if (tileScript.connectsLeft)
                _sortedTiles["left"].Add(tilePrefab);
        
            if (tileScript.connectsRight)
                _sortedTiles["right"].Add(tilePrefab);
        }
    }
    

    void EnsureConnectivity(Dictionary<Vector2Int, GameObject> placedTiles)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        // Start flood fill from the first placed tile
        if (placedTiles.Count > 0)
        {
            Vector2Int start = placedTiles.Keys.First();
            queue.Enqueue(start);
            visited.Add(start);
        }

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighbor = current + dir;

                if (placedTiles.ContainsKey(neighbor) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // At this point, 'visited' contains all connected tiles
        // You can check if there are any unvisited tiles in 'placedTiles' to identify disconnected parts
    }


    void RandomWalk(int maxMoves)
    {
        HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();
        List<Vector2Int> walkPath = new List<Vector2Int>();
        Vector2Int currentPos = new Vector2Int(0, 0); // Starting position
        walkPath.Add(currentPos);
        visitedPositions.Add(currentPos);

        if (!placedTiles.Any())
        {
            // Place the first Tile
            GameObject firstTile = GetRandomCompatibleTile(true, true, true, true);
            if (firstTile != null)
            {
                placedTiles[currentPos] = Instantiate(firstTile, new Vector3(currentPos.x * 10, 0, currentPos.y * 10), Quaternion.identity);
            }
            else
            {
                Debug.LogError("No initial tile could be placed. Check your tile prefabs.");
                return;
            }
        }

        // GameObject currentTile = placedTiles[currentPos];
        //
        // for (int i = 0; i < maxMoves; i++)
        // {
        //     Tile currentTileScript = currentTile.GetComponent<Tile>();
        //
        //     List<Vector2Int> possibleDirections = new List<Vector2Int>();
        //
        //     if (currentTileScript.connectsUp && !visitedPositions.Contains(currentPos + Vector2Int.up)) possibleDirections.Add(Vector2Int.up);
        //     if (currentTileScript.connectsDown && !visitedPositions.Contains(currentPos + Vector2Int.down)) possibleDirections.Add(Vector2Int.down);
        //     if (currentTileScript.connectsLeft && !visitedPositions.Contains(currentPos + Vector2Int.left)) possibleDirections.Add(Vector2Int.left);
        //     if (currentTileScript.connectsRight && !visitedPositions.Contains(currentPos + Vector2Int.right)) possibleDirections.Add(Vector2Int.right);
        //
        //     if (possibleDirections.Count == 0)
        //     {
        //         Debug.LogWarning("No possible directions to walk to from current tile.");
        //         break; // No valid moves
        //     }
        //
        //     // Choose a random direction from the available ones
        //     Vector2Int chosenDirection = possibleDirections[Random.Range(0, possibleDirections.Count)];
        //     currentPos += chosenDirection;
        //
        //     GameObject nextTile = GetRandomCompatibleTile(
        //         chosenDirection == Vector2Int.down,
        //         chosenDirection == Vector2Int.up,
        //         chosenDirection == Vector2Int.right,
        //         chosenDirection == Vector2Int.left
        //     );
        //
        //     if (nextTile != null)
        //     {
        //         placedTiles[currentPos] = Instantiate(nextTile, new Vector3(currentPos.x * 10, 0, currentPos.y * 10), Quaternion.identity);
        //         walkPath.Add(currentPos);
        //         visitedPositions.Add(currentPos); // Mark this position as visited
        //         currentTile = nextTile; // Update the current tile to the newly placed tile
        //     }
        //     else
        //     {
        //         Debug.LogWarning("Could not place a compatible tile.");
        //         break;
        //     }
        // }

        // Ensure the path has no open ends
        CloseOpenEnds();
    }

    void CloseOpenEnds()
    {
        foreach (var tile in placedTiles)
        {
            Vector2Int pos = tile.Key;
            Tile tileScript = tile.Value.GetComponent<Tile>();

            // Close off connections that are not matched by neighboring tiles
            if (tileScript.connectsUp && !placedTiles.ContainsKey(pos + Vector2Int.up))
            {
                tileScript.connectsUp = false;
            }

            if (tileScript.connectsDown && !placedTiles.ContainsKey(pos + Vector2Int.down))
            {
                tileScript.connectsDown = false;
            }

            if (tileScript.connectsLeft && !placedTiles.ContainsKey(pos + Vector2Int.left))
            {
                tileScript.connectsLeft = false;
            }

            if (tileScript.connectsRight && !placedTiles.ContainsKey(pos + Vector2Int.right))
            {
                tileScript.connectsRight = false;
            }

            // Optionally update the tile's appearance here to visually represent the closed ends
        }
    }

    GameObject GetRandomCompatibleTile(bool up, bool down, bool left, bool right)
    {
        List<GameObject> compatibleTiles = new List<GameObject>();

        // Check for each direction and add compatible tiles to the list
        if (up && _sortedTiles.ContainsKey("up"))
        {
            compatibleTiles.AddRange(_sortedTiles["up"]);
        }

        if (down && _sortedTiles.ContainsKey("down"))
        {
            compatibleTiles.AddRange(_sortedTiles["down"]);
        }

        if (left && _sortedTiles.ContainsKey("left"))
        {
            compatibleTiles.AddRange(_sortedTiles["left"]);
        }

        if (right && _sortedTiles.ContainsKey("right"))
        {
            compatibleTiles.AddRange(_sortedTiles["right"]);
        }

        if (compatibleTiles.Count > 0)
        {
            Debug.Log($"Found {compatibleTiles.Count} compatible tiles.");
            return compatibleTiles[Random.Range(0, compatibleTiles.Count)];
        }

        Debug.LogError("No compatible tiles found.");
        return null;
    }
}