using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Controller : MonoBehaviour
{
    public static Controller instance;

    [SerializeField]
    private Map map = null;
    public GameObject cubePrefab;
    public Button validateButton;

    private List<Cube> unusedPool;
    private List<Cube> usedPool;

    private List<Shape> shapes;
    private List<Color> shapeColors;
    private Shape selectedShape = null;
    private Shape shadowShape = null;

    [HideInInspector]
    public Cell mouseOverCell;
    private Cell mouseDownCell = null;

    [HideInInspector]
    public Cube mouseOverCube;
    private Cube mouseDownCube = null;
    private Cube draggedCube = null;

    [SerializeField]
    private Cell previewCell = null;
    private Shape previewShape = null;

    public int playerID = 1;

    [SerializeField]
    private int movingRange = 1;

    public List<Color> playersColors;

    private Cube tmpCube;
    private Shape tmpShape;
    private Cell tmpCell;
    private System.Random random;

    public Shape ShadowShape { get => shadowShape; }
    public int MovingRange { get => movingRange; }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (!instance)
                instance = this;
            else
                enabled = false;

            if (playersColors == null)
            {
                playersColors = new List<Color>();
                playersColors.Add(Color.blue);
                playersColors.Add(Color.red);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Cube.cubeScale = map.cellScale * 10;
        InitializePool();
        Shape.InitializeShapeDictionnary();
        shapes = new List<Shape>();
        shapeColors = new List<Color>();
        random = new System.Random();

        shapeColors.Add(Color.red);
        shapeColors.Add(Color.blue);
        shapeColors.Add(Color.green);
        shapeColors.Add(Color.yellow);

        if (previewCell && map)
        {
            previewCell.transform.localScale = Vector3.one * map.cellScale;
            previewCell.pos = previewCell.transform.localPosition;
        }
    }

    private void InitializePool()
    {
        unusedPool = new List<Cube>();
        usedPool = new List<Cube>();

        if (map && cubePrefab)
        {
            int l = (int)(map.size.x * map.size.y + 20);
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
            if (!map)
                Debug.LogError("Map unassigned");
            if (!cubePrefab)
                Debug.LogError("Element prefab missing");
        }
    }

    // Update is called once per frame
    void Update()
    {
        #region Shape Creation/Selection
        if(previewShape == null)
        {
            previewShape = GenerateRandomShape(previewCell);
            previewShape.Move(previewCell);
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (mouseOverCell)
                mouseDownCell = mouseOverCell;
            else if (mouseOverCube)
                mouseDownCube = mouseOverCube;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (mouseOverCell && mouseOverCell == mouseDownCell)
            {
                mouseDownCell = null;
                //action for a click on mouseOverCell
                if (mouseOverCell.free && (mouseOverCell.OwnerID == playerID || mouseOverCell.OwnerID == 0))
                {
                    selectedShape = previewShape;
                    selectedShape.MoveTemporarily(mouseOverCell);
                    selectedShape.Select();
                    selectedShape.previewShape = false;
                }
                else if (!mouseOverCell.free)
                {
                    selectedShape = mouseOverCell.cube.shape;
                    mouseOverCell.cube.shape.Select();
                }
            }
            else if (mouseOverCube && mouseOverCube == mouseDownCube && mouseOverCube.shape != shadowShape)
            {
                mouseDownCube = null;
                //action for a click on mouseOverCube
                selectedShape = mouseOverCube.shape;
                mouseOverCube.shape.Select();

            }
        }
        #endregion

        #region Drag&Drop
        if (Input.GetMouseButtonDown(0))
            if (mouseOverCube)
                draggedCube = mouseOverCube;
        if (Input.GetMouseButton(0) && draggedCube)
        {
            tmpCell = mouseOverCube ? mouseOverCube.cell : mouseOverCell ? mouseOverCell : null;
            if (tmpCell)
            {
                tmpCell = GetCell(tmpCell.pos - draggedCube.coords);
                if (tmpCell && tmpCell != draggedCube.shape.Cell)
                    draggedCube.shape.MoveTemporarily(tmpCell);
            }
        }
        else if (Input.GetMouseButtonUp(0))
            draggedCube = null;
        #endregion

        if (selectedShape != null)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                selectedShape.MoveTemporarily(selectedShape.Cell.pos + Vector3.forward);
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                selectedShape.MoveTemporarily(selectedShape.Cell.pos + Vector3.left);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                selectedShape.MoveTemporarily(selectedShape.Cell.pos + Vector3.back);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                selectedShape.MoveTemporarily(selectedShape.Cell.pos + Vector3.right);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                selectedShape.RotateTemporarily(1);
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                selectedShape.RotateTemporarily(3);
            }
        }
    }

    public Cell GetCell(int i, int j)
    {
        if (map)
            return map.GetCell(i, j);
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
        if (map)
            return map.GetAnyCell();
        return null;
    }

    public Vector2 GetMapSize()
    {
        if (map)
            return map.size;
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
    private Shape GenerateShape(ShapeType type, Cell cell, Color color, int rotation = 0)
    {
        tmpShape = new Shape(cell, type, color);
        tmpShape.Rotate(rotation);
        shapes.Add(tmpShape);
        return tmpShape;
    }

    private Shape GenerateRandomShape(Cell cell)
    {
        Color randomColor = shapeColors[random.Next(shapeColors.Count)];
        ShapeType randomType = (ShapeType)Shape.shapeTypeValues.GetValue(random.Next(Shape.shapeTypeValues.Length));
        return GenerateShape(randomType, cell, randomColor, random.Next(4));
    }

    public Shape GenerateShadowShape(Shape shape)
    {
        if(shape != null && (shadowShape == null || shape != shadowShape.shadowOf))
        {
            if (shadowShape != null)
                DeleteShape(shadowShape.shadowOf);

            Color c = shape.Color;
            shadowShape = GenerateShape(shape.Type, shape.Cell, new Color(c.r, c.g, c.b, 0.6f));
            shadowShape.previewShape = false;
            shadowShape.previewShape = shape.previewShape;
            if (shadowShape.previewShape)
                previewShape = shadowShape;
            else
                shadowShape.RemoveUILayer();
            shadowShape.coords.Clear();
            shadowShape.coords.AddRange(shape.coords);
            shadowShape.RefreshCubesPos();
            shadowShape.shadowOf = shape;
            return shadowShape;
        }
        return null;
    }

    public void UnselectShapes()
    {
        foreach (Shape s in shapes)
            s.Unselect();
    }

    public void DeleteShape(Shape shape)
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
                        cube.cell.cube = null;
                        cube.cell.free = true;
                    }
                    cube.cell = null;
                }
                cube.shape = null;
            }

            //if the shape has a shadow, set the shadow back to normal
            if(shadowShape != null && shadowShape.shadowOf == shape)
            {
                shadowShape.shadowOf = null;
                Color c = shadowShape.Color;
                shadowShape.Color = new Color(c.r, c.g, c.b, 1f);
                shadowShape = null;
            }
            shapes.Remove(shape);
        }
    }

    public void ReverseToShadow()
    {
        if (shadowShape != null)
        {
            tmpShape = shadowShape;
            DeleteShape(shadowShape.shadowOf);
            selectedShape = tmpShape;
            selectedShape.Select();
        }
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

    public void ValidateAction()
    {
        if(shadowShape != null)
        {
            foreach(Cube cube in shadowShape.shadowOf.cubes)
                if (cube.cell)
                {
                    cube.cell.cube = cube;
                    cube.cell.unvalidatedCube = null;
                    cube.cell.free = false;
                }
            if (shadowShape.previewShape)
            {
                shadowShape.previewShape = false;
                previewShape = null;
            }
            DeleteShape(shadowShape);
            shadowShape = null;
        }
    }
}
