using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class ProceduralTilemapGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public string tilesFolderPath = "Assets/Tiles/";
    private Dictionary<string, List<TileBase>> _categorizedTiles;

    public int width = 100;
    public int height = 100;
    public int seed;

    private void Start()
    {
        LoadAndCategorizeTiles();
        GenerateTilemap();
    }

    void LoadAndCategorizeTiles()
    {
        _categorizedTiles = new Dictionary<string, List<TileBase>>();

        // Load all tile assets from the specified folder
        string[] tilePaths = AssetDatabase.FindAssets("t:TileBase", new[] { tilesFolderPath });

        if (tilePaths is null)
        {
            throw new Exception("Could not load the tilePaths");
        }

        foreach (string tilePath in tilePaths)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(tilePath);
            var tile = AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);

            if (tile != null)
            {
                // Extract category from tile name
                var tileName = tile.name;
                var category = tileName.Split('_')?[1]; // Assuming naming convention is "Tile_Category"

                if (category is null)
                {
                    throw new Exception("wtf");
                }

                // Add tile to appropriate category list
                if (!_categorizedTiles.ContainsKey(category))
                {
                    _categorizedTiles[category] = new List<TileBase>();
                }

                if (_categorizedTiles[category] is null)
                {
                    throw new Exception("wtf");

                }

                _categorizedTiles[category].Add(tile);
            }
        }
    }

    void GenerateTilemap()
    {
        Random.InitState(seed);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // For this example, we'll randomly select a category and then randomly select a tile from that category
                var selectedCategory = SelectCategoryForTile(x, y);
                TileBase selectedTile = SelectTileFromCategory(selectedCategory);

                // Assign the tile to the tilemap
                if (tilemap is null)
                {
                    throw new Exception("tilemap is null");
                }
                tilemap.SetTile(new Vector3Int(x, y, 0), selectedTile);
            }
        }
    }

    string SelectCategoryForTile(int x, int y)
    {
        // Example logic to choose category (you can modify this logic)
        string[] categories = new string[] { "Open_AllSides", "Open_RightSide", "Open_LeftSide" }; // Add more as needed
        return categories[Random.Range(0, categories.Length)];
    }

    TileBase SelectTileFromCategory(string category)
    {
        if (category is null)
        {
            throw new Exception("Category is null");
        }

        if (_categorizedTiles is null)
        {
            throw new Exception("_categorizedTiles is null");
        }
        
        if (_categorizedTiles[category] is null)
        {
            throw new Exception("_categorizedTiles[category] is null");
        }
        
        if (_categorizedTiles.ContainsKey(category) && _categorizedTiles[category].Count > 0)
        {
            var tilesInCategory = _categorizedTiles[category];
            return tilesInCategory[Random.Range(0, tilesInCategory.Count)];
        }

        return null; // Fallback if no tile found
    }
}