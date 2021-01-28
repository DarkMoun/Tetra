using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject cellPrefab;
    public float cellScale = 0.1f;

    public Vector2 size = new Vector2(5, 12);
    private Dictionary<int, Dictionary<int, Cell>> grid;

    public string mapPath;
    private string[][] loadedMap;

    private static string neutralCellCharacter = "n";

    private Cell tmpCell;
    private List<string> tmpStringList1;
    private List<string> tmpStringList2;

    public string mapID = "0";

    // Start is called before the first frame update
    void Start()
    {
        LoadMap("maps/map" + mapID + ".txt");
    }

    private void InitializeGrid(string[][] map)
    {
        if (cellPrefab && map != null && map.Length > 0)
        {
            if (grid == null)
                grid = new Dictionary<int, Dictionary<int, Cell>>();
            grid.Clear();

            // check for max width of the grid
            int maxY = -1;
            for (int i = 0; i < map.Length; i++)
            {
                if (map[i] == null)
                    map[i] = new string[0];
                if (map[i].Length > maxY)
                    maxY = map[i].Length;
            }

            size = new Vector2(map.Length, maxY);

            for (int i = (int)(-size.x / 2); i < (int)(size.x / 2) + size.x % 2; i++)
            {
                for (int j = (int)(-size.y / 2); j < (int)(size.y / 2) + size.y % 2; j++)
                {
                    if (!grid.ContainsKey(i))
                        grid[i] = new Dictionary<int, Cell>();

                    // if loaded number is invalid or 0, skip this cell
                    int loadedNumber = 0;
                    bool neutralCell = map[i + (int)(size.x / 2)][j + (int)(size.y / 2)] == neutralCellCharacter;
                    if(!neutralCell)
                        int.TryParse(map[i + (int)(size.x / 2)][j + (int)(size.y / 2)], out loadedNumber);
                    if (loadedNumber == 0 && !neutralCell)
                        continue;

                    // initialize cell
                    grid[i][j] = GameObject.Instantiate(cellPrefab).GetComponent<Cell>();
                    grid[i][j].pos = new Vector3(i, 0, j);
                    grid[i][j].transform.SetParent(transform);
                    grid[i][j].transform.localScale = Vector3.one * cellScale;
                    grid[i][j].transform.localPosition = grid[i][j].pos * cellScale * 11f;
                    //center grid
                    grid[i][j].transform.localPosition += 5.5f * cellScale * (Vector3.right + Vector3.forward * ((size.y + 1) % 2));
                    if (neutralCell)
                    {
                        grid[i][j].IsNeutral = true;
                        grid[i][j].OwnerID = 0;
                        grid[i][j].IsTarget = false;
                    }
                    else
                    {
                        // set cell owner
                        grid[i][j].OwnerID = loadedNumber < 0 ? -loadedNumber : loadedNumber;
                        grid[i][j].IsTarget = loadedNumber < 0;
                    }
                }
            }
        }
        else
            Debug.LogError("Cell prefab missing");
    }

    private void LoadMap(string path)
    {
        if (File.Exists(path))
        {
            tmpStringList1 = new List<string>(File.ReadAllLines(path));
            if(tmpStringList1.Count > 0)
            {
                loadedMap = new string[tmpStringList1.Count][];
                for(int i = 0; i < loadedMap.Length; i++)
                    loadedMap[i] = tmpStringList1[i].Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
                loadedMap = InvertMatrix<string>(loadedMap);
                InitializeGrid(loadedMap);
            }
        }
        else
            Debug.LogError("Invalid map file path");
    }

    // Update is called once per frame
    void Update()
    {

    }

    private T[][] InvertMatrix<T>(T[][] mat)
    {
        if (mat != null && mat.Length > 0)
        {
            int maxLength = 0;
            for (int i = 0; i < mat.Length; i++)
            {
                if (mat[i] != null && mat[i].Length > maxLength)
                    maxLength = mat[i].Length;
            }

            if (maxLength > 0)
            {
                T[][] tmpMat = new T[maxLength][];
                for (int i = 0; i < mat.Length; i++)
                {
                    for (int j = 0; j < maxLength; j++)
                    {
                        if (tmpMat[j] == null)
                            tmpMat[j] = new T[mat.Length];
                        tmpMat[j][i] = default(T);
                        if (i < mat.Length && j < mat[i].Length)
                            tmpMat[j][i] = mat[i][j];
                    }
                }

                return tmpMat;
            }
        }

        return null;
    }

    public Cell GetCell(int i, int j)
    {
        if(grid != null)
            if (grid.ContainsKey(i) && grid[i].ContainsKey(j))
                return grid[i][j];
        return null;
    }

    public Cell GetAnyCell()
    {
        if(grid != null)
            foreach (Dictionary<int, Cell> d in grid.Values)
                foreach (Cell c in d.Values)
                    return c;
        return null;
    }
}
