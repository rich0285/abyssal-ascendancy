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
            // i want this only checking on one side of the tile that the random moves uses do i dont place a incomptable tile
            GameObject firstTile = GetRandomCompatibleTile(TileDirectionOptions.Up);
            
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

    GameObject GetRandomCompatibleTile(TileDirectionOptions connection)
    {
        List<GameObject> compatibleTiles = new List<GameObject>();

        // Check for each direction and add compatible tiles to the list
        if (connection == TileDirectionOptions.Up )
        {
            compatibleTiles.AddRange(_sortedTiles["up"]);
        }

        if (connection == TileDirectionOptions.Down)
        {
            compatibleTiles.AddRange(_sortedTiles["down"]);
        }

        if (connection == TileDirectionOptions.Left)
        {
            compatibleTiles.AddRange(_sortedTiles["left"]);
        }

        if (connection == TileDirectionOptions.Right)
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

public enum TileDirectionOptions
{
    Up,
    Down,
    Left,
    Right
}