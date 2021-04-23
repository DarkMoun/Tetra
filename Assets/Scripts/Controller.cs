using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Controller : MonoBehaviour
{
    public GameObject cubePrefab;

    protected List<Cube> unusedPool;
    protected List<Cube> usedPool;

    protected List<Shape> shapes;
    protected List<Color> shapeColors;
    protected Shape selectedShape = null;

    [SerializeField]
    protected Cell previewCell = null;
    public Shape previewShape = null;

    public int id;
    public Color color;

    [SerializeField]
    protected int movingRange = 1;

    protected Cube tmpCube;
    protected Shape tmpShape;
    protected Cell tmpCell;
    protected System.Random random;
    public int MovingRange { get => movingRange; }
    public List<Shape> Shapes { get => shapes; }
    public Cell PreviewCell { get => previewCell; }

    // Start is called before the first frame update
    protected void Start()
    {
        if(!EventHandler.instance.Players.ContainsKey(id))
            EventHandler.instance.Players.Add(id, this);

        Cube.cubeScale = Map.instance.cellScale * 10;
        InitializePool();
        Shape.InitializeShapeDictionnary();
        shapes = new List<Shape>();
        shapeColors = new List<Color>();
        random = new System.Random();

        shapeColors.Add(Color.red);
        shapeColors.Add(Color.blue);
        shapeColors.Add(Color.green);
        shapeColors.Add(Color.yellow);

        if (previewCell && Map.instance)
        {
            previewCell.transform.localScale = Vector3.one * Map.instance.cellScale;
            previewCell.pos = previewCell.transform.localPosition;
        }
    }

    private void InitializePool()
    {
        unusedPool = new List<Cube>();
        usedPool = new List<Cube>();

        if (Map.instance && cubePrefab)
        {
            int l = (int)(Map.instance.size.x * Map.instance.size.y + 10);
            for (int i = 0; i < l; i++)
            {
                tmpCube = GameObject.Instantiate(cubePrefab).GetComponent<Cube>();
                tmpCube.transform.SetParent(transform);
                tmpCube.transform.localScale = Vector3.one * Cube.cubeScale;
                tmpCube.gameObject.SetActive(false);
                unusedPool.Add(tmpCube);
            }
        }
        else
        {
            if (!Map.instance)
                Debug.LogError("Map unassigned");
            if (!cubePrefab)
                Debug.LogError("Element prefab missing");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Cell GetCell(int i, int j)
    {
        if (Map.instance)
            return Map.instance.GetCell(i, j);
        return null;
    }

    public Cell GetCell(float i, float j)
    {
        return GetCell((int)Math.Floor(i), (int)Math.Floor(j));
    }

    public Cell GetCell(Vector3 v)
    {
        return GetCell(v.x, v.z);
    }

    public Cell GetAnyCell()
    {
        if (Map.instance)
            return Map.instance.GetAnyCell();
        return null;
    }

    public Vector2 GetMapSize()
    {
        if (Map.instance)
            return Map.instance.size;
        else
            return Vector2.one * -1;
    }

    #region Shape Management
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="cell"></param>
    /// <param name="color"></param>
    /// <param name="rotation">The rotation value is an int between 0 and 3 and is then multiplied by 90</param>
    protected Shape GenerateShape(ShapeType type, Cell cell, Color color, int rotation = 0)
    {
        tmpShape = new Shape(cell, type, color);
        tmpShape.OwnerID = id;
        tmpShape.Rotate(rotation);
        shapes.Add(tmpShape);
        return tmpShape;
    }

    public Shape GenerateRandomShape(Cell cell)
    {
        Color randomColor = shapeColors[random.Next(shapeColors.Count)];
        ShapeType randomType = (ShapeType)Shape.shapeTypeValues.GetValue(random.Next(Shape.shapeTypeValues.Length));
        return GenerateShape(randomType, cell, randomColor, random.Next(4));
    }

    public void UnselectShapes()
    {
        foreach (Shape s in shapes)
            s.Unselect();
    }

    public virtual void DeleteShape(Shape shape)
    {
        if(shape != null)
        {
            if(selectedShape == shape)
            {
                UnselectShapes();
                selectedShape = null;
            }
            foreach (Cube cube in shape.cubes)
            {
                usedPool.Remove(cube);
                unusedPool.Add(cube);
                cube.gameObject.SetActive(false);
                cube.gameObject.layer = 5;
                if (cube.cell)
                {
                    if (cube == cube.cell.unvalidatedCube)
                        cube.cell.unvalidatedCube = null;
                    else
                    {
                        if (!cube.cell.overlappingCubes.Remove(cube))
                            cube.cell.cube = null;
                        cube.cell.free = true;
                    }
                    cube.cell = null;
                }
                cube.shape = null;
            }
        }
        shapes.Remove(shape);
    }

    public void AllocateCubes(Shape shape)
    {
        foreach(Cube cube in shape.cubes)
        {
            usedPool.Remove(cube);
            unusedPool.Add(cube);
            cube.gameObject.SetActive(false);
            cube.cell.cube = null;
            cube.cell.free = true;
            cube.cell = null;
            cube.shape = null;
        }
        shape.cubes.Clear();
        foreach(Vector3 v in shape.coords)
        {
            if(unusedPool.Count == 0)
            {
                tmpCube = GameObject.Instantiate(cubePrefab).GetComponent<Cube>();
                tmpCube.transform.SetParent(transform);
                tmpCube.transform.localScale = Vector3.one * Cube.cubeScale;
                tmpCube.gameObject.SetActive(false);
                unusedPool.Add(tmpCube);
                Debug.LogWarning("Had to instantiate in game because cubes weren't enough. Increase cube pool size");
            }
            tmpCube = unusedPool[unusedPool.Count - 1];
            unusedPool.Remove(tmpCube);
            usedPool.Add(tmpCube);
            tmpCube.gameObject.SetActive(true);
            tmpCube.transform.localPosition = shape.Cell.pos + v;
            shape.cubes.Add(tmpCube);
            tmpCube.shape = shape;
        }
    }
    #endregion
}
