using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public enum ShapeType
{
    I,
    L,
    L2,
    O,
    S,
    S2,
    T
}

public class Shape
{
    private static Dictionary<ShapeType, List<Vector3>> typesCoords;
    public static Array shapeTypeValues;

    private ShapeType type;
    public List<Vector3> coords;
    public List<Cube> cubes;
    private Color color;
    private Cell cell;
    public bool previewShape = true;

    public Shape shadowOf = null;

    private List<Vector3> tmpVectors;
    private Cell tmpCell;

    public ShapeType Type { 
        get => type;
        set { 
            type = value;
            coords.Clear();
            coords.AddRange(Shape.typesCoords[type]);
            Controller.instance.AllocateCubes(this);
        } 
    }

    public Color Color
    {
        get => color;
        set
        {
            color = value;
            foreach (Cube cube in cubes)
                cube.GetComponent<Renderer>().material.color = color;
        }
    }

    public Cell Cell {
        get => cell;
        set
        {
            cell = value;
            RefreshCubesPos();
        }
    }

    #region Constructors
    public Shape(Cell c)
    {
        coords = new List<Vector3>();
        tmpVectors = new List<Vector3>();
        cubes = new List<Cube>();
        Cell = c;
    }

    public Shape(Cell c, ShapeType t)
    {
        coords = new List<Vector3>();
        tmpVectors = new List<Vector3>();
        cubes = new List<Cube>();
        Cell = c;
        Type = t;
    }

    public Shape(Cell c, ShapeType t, Color col)
    {
        coords = new List<Vector3>();
        tmpVectors = new List<Vector3>();
        cubes = new List<Cube>();
        Cell = c;
        Type = t;
        Color = col;
    }
    #endregion

    public static void InitializeShapeDictionnary()
    {
        if (typesCoords != null)
            return;
        typesCoords = new Dictionary<ShapeType, List<Vector3>>();
        typesCoords.Add(ShapeType.I, new List<Vector3>());
        typesCoords.Add(ShapeType.L, new List<Vector3>());
        typesCoords.Add(ShapeType.L2, new List<Vector3>());
        typesCoords.Add(ShapeType.O, new List<Vector3>());
        typesCoords.Add(ShapeType.S, new List<Vector3>());
        typesCoords.Add(ShapeType.S2, new List<Vector3>());
        typesCoords.Add(ShapeType.T, new List<Vector3>());
        shapeTypeValues = Enum.GetValues(typeof(ShapeType));

        typesCoords[ShapeType.I].Add(Vector3.zero);
        typesCoords[ShapeType.I].Add(new Vector3(0, 0, 1));
        typesCoords[ShapeType.I].Add(new Vector3(0, 0, -1));
        typesCoords[ShapeType.I].Add(new Vector3(0, 0, -2));
        typesCoords[ShapeType.L].Add(Vector3.zero);
        typesCoords[ShapeType.L].Add(new Vector3(0, 0, 1));
        typesCoords[ShapeType.L].Add(new Vector3(0, 0, -1));
        typesCoords[ShapeType.L].Add(new Vector3(1, 0, -1));
        typesCoords[ShapeType.L2].Add(Vector3.zero);
        typesCoords[ShapeType.L2].Add(new Vector3(0, 0, 1));
        typesCoords[ShapeType.L2].Add(new Vector3(0, 0, -1));
        typesCoords[ShapeType.L2].Add(new Vector3(-1, 0, -1));
        typesCoords[ShapeType.O].Add(Vector3.zero);
        typesCoords[ShapeType.O].Add(new Vector3(0, 0, 1));
        typesCoords[ShapeType.O].Add(new Vector3(-1, 0, 0));
        typesCoords[ShapeType.O].Add(new Vector3(-1, 0, 1));
        typesCoords[ShapeType.S].Add(Vector3.zero);
        typesCoords[ShapeType.S].Add(new Vector3(0, 0, -1));
        typesCoords[ShapeType.S].Add(new Vector3(1, 0, -1));
        typesCoords[ShapeType.S].Add(new Vector3(-1, 0, 0));
        typesCoords[ShapeType.S2].Add(Vector3.zero);
        typesCoords[ShapeType.S2].Add(new Vector3(0, 0, 1));
        typesCoords[ShapeType.S2].Add(new Vector3(1, 0, 1));
        typesCoords[ShapeType.S2].Add(new Vector3(-1, 0, 0));
        typesCoords[ShapeType.T].Add(Vector3.zero);
        typesCoords[ShapeType.T].Add(new Vector3(0, 0, 1));
        typesCoords[ShapeType.T].Add(new Vector3(0, 0, -1));
        typesCoords[ShapeType.T].Add(new Vector3(-1, 0, 0));
    }

    #region Movement
    /// <summary>
    /// 
    /// </summary>
    /// <param name="rotation">The rotation value is an int between 0 and 3 and is then multiplied by 90</param>
    public void Rotate(int rotation)
    {
        rotation = rotation % 4;
        if (rotation != 0)
        {
            tmpVectors.Clear();
            tmpVectors.AddRange(coords);
            coords.Clear();

            if (rotation == 1)
                foreach (Vector3 v in tmpVectors)
                    coords.Add(new Vector3(v.z, v.y, -v.x));
            else if (rotation == 2)
                foreach (Vector3 v in tmpVectors)
                    coords.Add(new Vector3(-v.x, v.y, -v.z));
            else if (rotation == 3)
                foreach (Vector3 v in tmpVectors)
                    coords.Add(new Vector3(-v.z, v.y, v.x));
        }

        RefreshCubesPos();
    }

    public void RotateTemporarily(int rotation)
    {
        Controller.instance.GenerateShadowShape(this);
        Rotate(rotation);
        if(Controller.instance.ShadowShape != null && Controller.instance.ShadowShape.shadowOf == this)
            Controller.instance.validateButton.interactable = IsValid();
    }

    public void Move(Vector2 pos)
    {
        Move(Controller.instance.GetCell(pos.x, pos.y));
    }

    public void Move(Vector3 pos)
    {
        Move(Controller.instance.GetCell(pos.x, pos.z));
    }

    public void Move(Cell c)
    {
        if (c)
            cell = c;
        RefreshCubesPos();
    }

    public void MoveTemporarily(Vector2 pos)
    {
        tmpCell = Controller.instance.GetCell(pos.x, pos.y);
        //check moving range
        if (!previewShape && tmpCell)
            if (Controller.instance.ShadowShape != null && Controller.instance.ShadowShape.shadowOf == this
                && !Controller.instance.ShadowShape.previewShape)
                if (Math.Abs(tmpCell.pos.x - Controller.instance.ShadowShape.cell.pos.x)
                    + Math.Abs(tmpCell.pos.z - Controller.instance.ShadowShape.cell.pos.z) > Controller.instance.MovingRange)
                    return;

        if (tmpCell)
            Controller.instance.GenerateShadowShape(this);
        Move(tmpCell);
        if (Controller.instance.ShadowShape != null && Controller.instance.ShadowShape.shadowOf == this)
            Controller.instance.validateButton.interactable = IsValid();
    }

    public void MoveTemporarily(Vector3 pos)
    {
        tmpCell = Controller.instance.GetCell(pos.x, pos.z);
        //check moving range
        if (!previewShape && tmpCell)
            if (Controller.instance.ShadowShape != null && Controller.instance.ShadowShape.shadowOf == this
                && !Controller.instance.ShadowShape.previewShape)
                if (Math.Abs(tmpCell.pos.x - Controller.instance.ShadowShape.cell.pos.x)
                    + Math.Abs(tmpCell.pos.z - Controller.instance.ShadowShape.cell.pos.z) > Controller.instance.MovingRange)
                    return;

        if (tmpCell)
            Controller.instance.GenerateShadowShape(this);
        Move(tmpCell);
        if (Controller.instance.ShadowShape != null && Controller.instance.ShadowShape.shadowOf == this)
            Controller.instance.validateButton.interactable = IsValid();
    }

    public void MoveTemporarily(Cell c)
    {
        //check moving range
        if (!previewShape && c)
            if (Controller.instance.ShadowShape != null && Controller.instance.ShadowShape.shadowOf == this
                && !Controller.instance.ShadowShape.previewShape)
                if (Math.Abs(c.pos.x - Controller.instance.ShadowShape.cell.pos.x)
                    + Math.Abs(c.pos.z - Controller.instance.ShadowShape.cell.pos.z) > Controller.instance.MovingRange)
                    return;

        if (c)
            Controller.instance.GenerateShadowShape(this);
        if (previewShape)
            RemoveUILayer();
        Move(c);
        if (Controller.instance.ShadowShape != null && Controller.instance.ShadowShape.shadowOf == this)
            Controller.instance.validateButton.interactable = IsValid();
    }

    public void RefreshCubesPos()
    {
        for (int i = 0; i < coords.Count; i++)
        {
            cubes[i].coords = coords[i];
            if (cubes[i].cell)
            {
                if (cubes[i].cell.cube == cubes[i])
                    cubes[i].cell.free = true;
                cubes[i].cell = null;
            }
            if (previewShape)
                cubes[i].transform.localPosition = (cell.pos + coords[i]) * Cube.cubeScale;
            else
                cubes[i].transform.localPosition = (cell.pos + coords[i]
                    + Vector3.right * 0.5f
                    + Vector3.forward * 0.5f * ((Controller.instance.GetMapSize().y + 1) % 2))
                    * Cube.cubeScale * 1.1f;
            tmpCell = Controller.instance.GetCell(cell.pos + coords[i]);
            if (tmpCell)
            {
                cubes[i].cell = tmpCell;
                if (Controller.instance.ShadowShape != null && Controller.instance.ShadowShape.shadowOf == this)
                    tmpCell.unvalidatedCube = cubes[i];
                else
                {
                    tmpCell.cube = cubes[i];
                    tmpCell.free = false;
                }
            }
        }
    }

    public bool IsValid()
    {
        foreach(Cube c in cubes)
        {
            if (!c.cell)
                return false;
            else if (!c.cell.free && c.cell.cube.shape != this && c.cell.cube.shape.shadowOf != this)
                return false;
        }
        return true;
    }
    #endregion

    #region Selection
    public void Select()
    {
        if(shadowOf == null)
        {
            Controller.instance.UnselectShapes();
            foreach (Cube c in cubes)
                c.SetSelected();
        }
    }

    public void Unselect()
    {
        foreach (Cube c in cubes)
            c.SetUnselected();
    }
    #endregion

    public void RemoveUILayer()
    {
        foreach (Cube c in cubes)
            c.gameObject.layer = 0;
        previewShape = false;
    }
}
