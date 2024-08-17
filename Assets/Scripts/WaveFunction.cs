using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunction : MonoBehaviour
{
    public int dimensions;
    public Tile[] tileObjects;
    public List<Cell> gridComponents = new List<Cell>();
    public Cell cellObj;

    int iterations = 0;

    void Awake()
    {
        // gridComponents = new List<Cell>();
        InitializeGrid();
    }

    void InitializeGrid()
    {
        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                Cell newCell = Instantiate(cellObj, new Vector2(x * 10, y * 10), Quaternion.identity);
                newCell!.CreateCell(false, tileObjects);
                gridComponents!.Add(newCell);
            }
        }

        StartCoroutine(CheckEntropy());
    }


    IEnumerator CheckEntropy()
    {
        List<Cell> tempGrid = new List<Cell>(gridComponents!);

        tempGrid.RemoveAll(c => c.collapsed);

        tempGrid.Sort((a, b) => { return a.tileOptions.Length - b.tileOptions.Length; });

        int arrLength = tempGrid[0].tileOptions.Length;
        int stopIndex = default;

        for (int i = 1; i < tempGrid.Count; i++)
        {
            if (tempGrid[i].tileOptions.Length > arrLength)
            {
                stopIndex = i;
                break;
            }
        }

        if (stopIndex > 0)
        {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }

        yield return new WaitForSeconds(.01f);

        CollapseCell(tempGrid);
    }

    void CollapseCell(List<Cell> tempGrid)
    {
        Debug.Log(tempGrid.Count);
        Debug.Log(tempGrid);
        
        
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);

        Cell cellToCollapse = tempGrid[randIndex];

        cellToCollapse.collapsed = true;
        
        if (cellToCollapse.tileOptions.Length == 0)
        {
            Debug.LogError("Error: No tile options available for cell collapse.");
            return; // Or handle the situation appropriately
        }

        Tile selectedTile = cellToCollapse.tileOptions[UnityEngine.Random.Range(0, cellToCollapse.tileOptions.Length)];
        
        cellToCollapse.tileOptions = new Tile[] { selectedTile };

        Tile foundTile = cellToCollapse.tileOptions[0];
        Instantiate(foundTile, cellToCollapse.transform.position, Quaternion.identity);

        UpdateGeneration();
    }

    void UpdateGeneration()
    {
        List<Cell> newGenerationCell = new List<Cell>(gridComponents);

        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                var index = x + y * dimensions;
                Cell currentCell = gridComponents[index];

                if (currentCell.collapsed)
                {
                    Debug.Log("Cell already collapsed at index: " + index);
                    newGenerationCell[index] = currentCell;
                    continue;
                }

                List<Tile> options = new List<Tile>(tileObjects);

                // Checking neighbors and adjusting options accordingly
                if (y > 0) UpdateOptions(options, gridComponents[x + (y - 1) * dimensions], cell => cell.UpNeighbours);
                if (x < dimensions - 1) UpdateOptions(options, gridComponents[x + 1 + y * dimensions], cell => cell.LeftNeighbours);
                if (y < dimensions - 1) UpdateOptions(options, gridComponents[x + (y + 1) * dimensions], cell => cell.DownNeighbours);
                if (x > 0) UpdateOptions(options, gridComponents[x - 1 + y * dimensions], cell => cell.RightNeighbours);

                if (options.Count == 0)
                {
                    Debug.LogError("No valid options found for cell at index: " + index);
                    // Handle the error case
                    continue;
                }

                newGenerationCell[index].RecreateCell(options.ToArray());
            }
        }

        gridComponents = newGenerationCell;
        iterations++;

        if(iterations < dimensions * dimensions)
        {
            StartCoroutine(CheckEntropy());
        }
    }

    void UpdateOptions(List<Tile> options, Cell neighbor, Func<Tile, Tile[]> getValidNeighbors)
    {
        List<Tile> validOptions = new List<Tile>();
        foreach (Tile neighborTile in neighbor.tileOptions)
        {
            var valid = getValidNeighbors(neighborTile);
            validOptions.AddRange(valid);
        }
        CheckValidity(options, validOptions);
    }

    void CheckValidity(List<Tile> optionList, List<Tile> validOption)
    {
        for (int x = optionList.Count - 1; x >= 0; x--)
        {
            var element = optionList[x];
            if (!validOption.Contains(element))
            {
                optionList.RemoveAt(x);
            }
        }
    }
}