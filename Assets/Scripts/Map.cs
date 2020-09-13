using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject cellPrefab;
    public float cellScale = 0.1f;

    public Vector2 size = new Vector2(5, 12);
    private Dictionary<int, Dictionary<int, Cell>> grid;

    private Cell tmpCell;

    // Start is called before the first frame update
    void Start()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        if (cellPrefab)
        {
            grid = new Dictionary<int, Dictionary<int, Cell>>();

            size = new Vector2((int)size.x, (int)size.y);
            if (size.y % 2 == 1)
            {
                size = new Vector2(size.x, size.y + 1);
                Debug.LogWarning("Grid height has to be an even number. Grid height set to " + size.y);
            }

            for (int i = (int)(-size.x / 2); i < (int)(size.x / 2) + size.x % 2; i++)
            {
                for (int j = (int)(-size.y / 2); j < (int)(size.y / 2) + size.y % 2; j++)
                {
                    if (!grid.ContainsKey(i))
                        grid[i] = new Dictionary<int, Cell>();
                    grid[i][j] = GameObject.Instantiate(cellPrefab).GetComponent<Cell>();
                    grid[i][j].pos = new Vector3(i, 0, j);
                    grid[i][j].transform.SetParent(transform);
                    grid[i][j].transform.localScale = Vector3.one * cellScale;
                    grid[i][j].transform.localPosition = grid[i][j].pos * cellScale * 11f;
                    //center grid
                    grid[i][j].transform.localPosition += 5.5f * cellScale * (Vector3.right + Vector3.forward * ((size.y + 1) % 2));
                }
            }
        }
        else
            Debug.LogError("Cell prefab missing");
    }

    // Update is called once per frame
    void Update()
    {
        
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
