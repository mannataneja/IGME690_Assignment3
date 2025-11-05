using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class WaveFunctionCollapse : MonoBehaviour
{
    public int dimensions;
    public Tile[] tileObjects;
    public Tile[] cornerTiles;
    public Tile[] topEdgeTiles;
    public Tile[] bottomEdgeTiles;
    public Tile[] leftEdgeTiles;
    public Tile[] rightEdgeTiles;

    public List<Cell> gridComponents;
    public Cell cellObj;

    public Tile backupTile;

    private int iteration;


    private void Awake()
    {
        gridComponents = new List<Cell>();
        InitializeGrid();
    }

    void InitializeGrid()
    {
        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                Cell newCell = Instantiate(cellObj, new Vector3(x, 0, y), Quaternion.identity);

                newCell.gridX = x;
                newCell.gridY = y;

                bool isCorner =
                    (x == 0 && y == 0) ||
                    (x == dimensions - 1 && y == 0) ||
                    (x == 0 && y == dimensions - 1) ||
                    (x == dimensions - 1 && y == dimensions - 1);

                bool isTopEdge = (y == dimensions - 1 && !isCorner);
                bool isBottomEdge = (y == 0 && !isCorner);
                bool isLeftEdge = (x == 0 && !isCorner);
                bool isRightEdge = (x == dimensions - 1 && !isCorner);

                // Assign rule-based tile options
                if (isCorner)
                {
                    newCell.CreateCell(false,
                        (cornerTiles != null && cornerTiles.Length > 0)
                        ? cornerTiles : tileObjects);
                }
                else if (isTopEdge)
                {
                    newCell.CreateCell(false,
                        (topEdgeTiles != null && topEdgeTiles.Length > 0)
                        ? topEdgeTiles : tileObjects);
                }
                else if (isBottomEdge)
                {
                    newCell.CreateCell(false,
                        (bottomEdgeTiles != null && bottomEdgeTiles.Length > 0)
                        ? bottomEdgeTiles : tileObjects);
                }
                else if (isLeftEdge)
                {
                    newCell.CreateCell(false,
                        (leftEdgeTiles != null && leftEdgeTiles.Length > 0)
                        ? leftEdgeTiles : tileObjects);
                }
                else if (isRightEdge)
                {
                    newCell.CreateCell(false,
                        (rightEdgeTiles != null && rightEdgeTiles.Length > 0)
                        ? rightEdgeTiles : tileObjects);
                }
                else
                {
                    // ✅ Center uses ALL tiles
                    newCell.CreateCell(false, tileObjects);
                }

                gridComponents.Add(newCell);
            }
        }

        StartCoroutine(CheckEntropy());
    }



    IEnumerator CheckEntropy()
    {
        List<Cell> tempGrid = new List<Cell>(gridComponents);
        tempGrid.RemoveAll(c => c.collapsed);
        tempGrid.Sort((a, b) => a.tileOptions.Length - b.tileOptions.Length);
        tempGrid.RemoveAll(a => a.tileOptions.Length != tempGrid[0].tileOptions.Length);

        yield return new WaitForSeconds(0.025f);

        CollapseCell(tempGrid);
    }

    void CollapseCell(List<Cell> tempGrid)
    {
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);

        Cell cellToCollapse = tempGrid[randIndex];

        cellToCollapse.collapsed = true;
        try
        {
            Tile selectedTile = GetWeightedRandomTile(cellToCollapse.tileOptions);
            cellToCollapse.tileOptions = new Tile[] { selectedTile };
        }
        catch
        {
            Tile selectedTile = backupTile;
            cellToCollapse.tileOptions = new Tile[] { selectedTile };
        }

        Tile foundTile = cellToCollapse.tileOptions[0];
        Instantiate(foundTile, cellToCollapse.transform.position, foundTile.transform.rotation);

        UpdateGeneration();
    }

    void UpdateGeneration()
    {
        List<Cell> newGenerationCell = new List<Cell>(gridComponents);

        for(int y = 0; y < dimensions; y++)
        {
            for(int x = 0; x < dimensions; x++)
            {
                var index = x + y * dimensions;

                if (gridComponents[index].collapsed)
                {
                    newGenerationCell[index] = gridComponents[index];
                }
                else
                {
                    // Start from this cell's current domain (corner/edge restrictions preserved)
                    List<Tile> options = new List<Tile>(gridComponents[index].tileOptions);

                    // If somehow empty (e.g., inspector arrays left blank), fall back to all tiles
                    if (options.Count == 0)
                        options.AddRange(tileObjects);

                    // neighbor constraints
                    if (y > 0)
                    {
                        Cell up = gridComponents[x + (y - 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile possibleOptions in up.tileOptions)
                        {
                            var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[validOption].downNeighbours;
                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }

                    if (x < dimensions - 1)
                    {
                        Cell rightNeighbor = gridComponents[x + 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile possibleOptions in rightNeighbor.tileOptions)
                        {
                            var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[validOption].rightNeighbours;
                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }

                    if (y < dimensions - 1)
                    {
                        Cell down = gridComponents[x + (y + 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile possibleOptions in down.tileOptions)
                        {
                            var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[validOption].upNeighbours;
                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }

                    if (x > 0)
                    {
                        Cell leftNeighbor = gridComponents[x - 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile possibleOptions in leftNeighbor.tileOptions)
                        {
                            var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[validOption].leftNeighbours;
                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }

                    // If constraints eliminated everything, keep at least something to avoid deadlock
                    if (options.Count == 0)
                        options.Add(backupTile != null ? backupTile : tileObjects[0]);

                    // Apply reduced domain
                    Tile[] newTileList = new Tile[options.Count];
                    for (int i = 0; i < options.Count; i++)
                        newTileList[i] = options[i];

                    newGenerationCell[index].RecreateCell(newTileList);
                }

            }
        }

        gridComponents = newGenerationCell;
        iteration++;

        if (iteration < dimensions * dimensions)
        {
            StartCoroutine(CheckEntropy());
        }
    }

    void CheckValidity(List<Tile> optionList, List<Tile> validOption)
    {
        for(int x = optionList.Count - 1; x >=0; x--)
        {
            var element = optionList[x];
            if (!validOption.Contains(element))
            {
                optionList.RemoveAt(x);
            }
        }
    }
    Tile GetWeightedRandomTile(Tile[] tiles)
    {
        int totalWeight = 0;
        foreach (Tile tile in tiles) totalWeight += tile.weight;

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (Tile t in tiles)
        {
            cumulative += t.weight;
            if (randomValue < cumulative)
            {
                return t;
            }
        }

        return tiles[0]; // fallback
    }

}
