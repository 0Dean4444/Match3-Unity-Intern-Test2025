using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board
{
    public List<Cell> AllCells { get; private set; }
    private Transform m_root;
    private float spacingX = 1.0f;
    private float spacingY = 1.0f;

    public Board(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;
        AllCells = new List<Cell>();

        CreateRandomLayeredBoard(gameSettings);
    }

    private void CreateRandomLayeredBoard(GameSettings gameSettings)
    {
        int layers = gameSettings.Layers;
        int minRequiredSize = layers * 2 + 2;
        int sizeX = Mathf.Max(gameSettings.BoardSizeX, minRequiredSize);
        int sizeY = Mathf.Max(gameSettings.BoardSizeY, minRequiredSize);

        int[][,] levelLayout = new int[layers][,];
        for (int z = 0; z < layers; z++)
        {
            levelLayout[z] = new int[sizeX, sizeY];

            int shrink = z + 1;

            for (int x = shrink; x < sizeX - shrink; x++)
            {
                for (int y = shrink; y < sizeY - shrink; y++)
                {
                    bool hasSupport = z == 0 || levelLayout[z - 1][x, y] == 1;

                    if (hasSupport)
                    {
                        float chance = (z == 0) ? 0.9f : 0.7f;

                        if (UnityEngine.Random.value <= chance)
                        {
                            levelLayout[z][x, y] = 1;
                        }
                    }
                }
            }
        }
        Vector3 origin = new Vector3(-sizeX * spacingX * 0.5f + (spacingX / 2f), -sizeY * spacingY * 0.5f + (spacingY / 2f), 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);

        for (int z = 0; z < layers; z++)
        {
            int[,] currentLayer = levelLayout[z];
            Vector3 layerOffset = new Vector3(z * (spacingX * 0.5f), z * (spacingY * 0.5f), -z * 0.1f);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    if (currentLayer[x, y] == 1)
                    {
                        GameObject go = GameObject.Instantiate(prefabBG);
                        go.transform.position = origin + new Vector3(x * spacingX, y * spacingY, 0f) + layerOffset;
                        go.transform.SetParent(m_root);

                        Cell cell = go.GetComponent<Cell>();
                        cell.Setup(x, y, z);
                        cell.GetComponent<SpriteRenderer>().enabled = false;

                        AllCells.Add(cell);
                    }
                }
            }
        }
        int remainder = AllCells.Count % 3;
        for (int i = 0; i < remainder; i++)
        {
            Cell c = AllCells[AllCells.Count - 1];
            AllCells.Remove(c);
            GameObject.Destroy(c.gameObject);
        }
    }

    internal void Fill()
    {
        List<NormalItem.eNormalType> typesToSpawn = new List<NormalItem.eNormalType>();
        int tripletCount = AllCells.Count / 3;
        List<NormalItem.eNormalType> allAvailableTypes = Enum.GetValues(typeof(NormalItem.eNormalType))
                                                             .Cast<NormalItem.eNormalType>()
                                                             .ToList();
        allAvailableTypes = allAvailableTypes.OrderBy(x => Guid.NewGuid()).ToList();
        for (int i = 0; i < tripletCount; i++)
        {
            NormalItem.eNormalType selectedType = allAvailableTypes[i % allAvailableTypes.Count];

            typesToSpawn.Add(selectedType);
            typesToSpawn.Add(selectedType);
            typesToSpawn.Add(selectedType);
        }

        typesToSpawn = typesToSpawn.OrderBy(x => Guid.NewGuid()).ToList();

        for (int i = 0; i < AllCells.Count; i++)
        {
            Cell cell = AllCells[i];
            NormalItem item = new NormalItem();
            item.SetType(typesToSpawn[i]);
            item.SetView();
            item.SetViewRoot(m_root);

            SpriteRenderer sp = item.View.GetComponent<SpriteRenderer>();
            if (sp)
            {
                sp.sortingOrder = (cell.BoardZ * 10) + 1;
            }

            cell.Assign(item);
            cell.ApplyItemPosition(true);
        }

        UpdateExposedItems();
    }

    public void UpdateExposedItems()
    {
        foreach (var cell in AllCells)
        {
            if (cell.IsEmpty)
            {
                cell.SetHighlight(false);
                continue;
            }

            bool isCovered = false;
            foreach (var topCell in AllCells)
            {
                if (topCell == cell || topCell.IsEmpty) continue;

                if (topCell.BoardZ > cell.BoardZ)
                {
                    float dx = Mathf.Abs(topCell.transform.position.x - cell.transform.position.x);
                    float dy = Mathf.Abs(topCell.transform.position.y - cell.transform.position.y);

                    if (dx < spacingX * 0.8f && dy < spacingY * 0.8f)
                    {
                        isCovered = true;
                        break;
                    }
                }
            }

            cell.SetHighlight(!isCovered);
        }
    }

    public void Clear()
    {
        foreach (var cell in AllCells)
        {
            cell.Clear();
            if (cell != null && cell.gameObject != null)
            {
                GameObject.Destroy(cell.gameObject);
            }
        }
        AllCells.Clear();
    }
}