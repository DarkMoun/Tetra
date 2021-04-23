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
    private int ownerID = 0;

    private Controller owner;
    private bool ownerIsPlayer;

    public Shape shadowOf = null;

    private List<Vector3> tmpVectors;
    private Cell tmpCell;

    public ShapeType Type { 
        get => type;
        set { 
            type = value;
            coords.Clear();
            coords.AddRange(Shape.typesCoords[type]);
            if (EventHandler.instance.Players.ContainsKey(ownerID))
                EventHandler.instance.Players[ownerID].AllocateCubes(this);
            else
                ControllerPlayer.instance.AllocateCubes(this);
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

    public int OwnerID { 
        get => ownerID;
        set
        {
            ownerID = value;
            if (EventHandler.instance.Players.ContainsKey(ownerID))
            {
                owner = EventHandler.instance.Players[ownerID];
                ownerIsPlayer = owner is ControllerPlayer;
            }

            foreach (Cube c in cubes)
                c.OwnerID = ownerID;
        }
    }

    public Controller Owner { get => owner; }

    public Shape(Cell c, ShapeType t, Color col)
    {
        coords = new List<Vector3>();
        tmpVectors = new List<Vector3>();
        cubes = new List<Cube>();
        Cell = c;
        Type = t;
        Color = col;
    }

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
        if (ownerIsPlayer)
        {
            ControllerPlayer.instance.GenerateShadowShape(this);
            Rotate(rotation);
            if (ControllerPlayer.instance.ShadowShape != null && ControllerPlayer.instance.ShadowShape.shadowOf == this)
                ControllerPlayer.instance.validateButton.interactable = previewShape ? IsValidForCreation() : IsValid();
        }
    }

    public void Move(Vector2 pos)
    {
        Move(owner.GetCell(pos.x, pos.y));
    }

    public void Move(Vector3 pos)
    {
        Move(owner.GetCell(pos.x, pos.z));
    }

    public void Move(Cell c)
    {
        if (c)
            cell = c;
        RefreshCubesPos();
    }

    public void MoveTemporarily(Vector2 pos)
    {
        if (ownerIsPlayer)
        {
            tmpCell = ControllerPlayer.instance.GetCell(pos.x, pos.y);
            //check moving range
            if (!previewShape && tmpCell)
                if (ControllerPlayer.instance.ShadowShape != null && ControllerPlayer.instance.ShadowShape.shadowOf == this
                    && !ControllerPlayer.instance.ShadowShape.previewShape)
                    if (Math.Abs(tmpCell.pos.x - ControllerPlayer.instance.ShadowShape.cell.pos.x)
                        + Math.Abs(tmpCell.pos.z - ControllerPlayer.instance.ShadowShape.cell.pos.z) > ControllerPlayer.instance.MovingRange)
                        return;

            if (tmpCell)
                ControllerPlayer.instance.GenerateShadowShape(this);
            Move(tmpCell);
            if (ControllerPlayer.instance.ShadowShape != null && ControllerPlayer.instance.ShadowShape.shadowOf == this)
                ControllerPlayer.instance.validateButton.interactable = previewShape ? IsValidForCreation() : IsValid();
        }
    }

    public void MoveTemporarily(Vector3 pos)
    {
        if (ownerIsPlayer)
        {
            tmpCell = ControllerPlayer.instance.GetCell(pos.x, pos.z);
            //check moving range
            if (!previewShape && tmpCell)
                if (ControllerPlayer.instance.ShadowShape != null && ControllerPlayer.instance.ShadowShape.shadowOf == this
                    && !ControllerPlayer.instance.ShadowShape.previewShape)
                    if (Math.Abs(tmpCell.pos.x - ControllerPlayer.instance.ShadowShape.cell.pos.x)
                        + Math.Abs(tmpCell.pos.z - ControllerPlayer.instance.ShadowShape.cell.pos.z) > ControllerPlayer.instance.MovingRange)
                        return;

            if (tmpCell)
                ControllerPlayer.instance.GenerateShadowShape(this);
            Move(tmpCell);
            if (ControllerPlayer.instance.ShadowShape != null && ControllerPlayer.instance.ShadowShape.shadowOf == this)
                ControllerPlayer.instance.validateButton.interactable = previewShape ? IsValidForCreation() : IsValid();
        }
    }

    public void MoveTemporarily(Cell c)
    {
        if (ownerIsPlayer)
        {
            //check moving range
            if (!previewShape && c)
                if (ControllerPlayer.instance.ShadowShape != null && ControllerPlayer.instance.ShadowShape.shadowOf == this
                    && !ControllerPlayer.instance.ShadowShape.previewShape)
                    if (Math.Abs(c.pos.x - ControllerPlayer.instance.ShadowShape.cell.pos.x)
                        + Math.Abs(c.pos.z - ControllerPlayer.instance.ShadowShape.cell.pos.z) > ControllerPlayer.instance.MovingRange)
                        return;

            if (c)
                ControllerPlayer.instance.GenerateShadowShape(this);
            if (previewShape)
                RemoveUILayer();
            Move(c);
            if (ControllerPlayer.instance.ShadowShape != null && ControllerPlayer.instance.ShadowShape.shadowOf == this)
                ControllerPlayer.instance.validateButton.interactable = previewShape ? IsValidForCreation() : IsValid();
        }
    }

    public void RefreshCubesPos()
    {
        for (int i = 0; i < coords.Count; i++)
        {
            cubes[i].coords = coords[i];
            if (cubes[i].cell)
            {
                // remove the cube from cell data
                if (!cubes[i].cell.overlappingCubes.Remove(cubes[i]) && cubes[i].cell.cube == cubes[i])
                {
                    if(cubes[i].cell.overlappingCubes.Count > 0)
                    {
                        cubes[i].cell.cube = cubes[i].cell.overlappingCubes[0];
                        cubes[i].cell.overlappingCubes.Remove(cubes[i].cell.cube);
                    }
                    else
                    {
                        cubes[i].cell.cube = null;
                        cubes[i].cell.free = true;
                    }
                }

                cubes[i].cell = null;
            }
            if (previewShape)
                cubes[i].transform.localPosition = (cell.pos + coords[i]) * Cube.cubeScale;
            else
                cubes[i].transform.localPosition = (cell.pos + coords[i]
                    + Vector3.right * 0.5f
                    + Vector3.forward * 0.5f * ((owner.GetMapSize().y + 1) % 2))
                    * Cube.cubeScale * 1.1f;
            tmpCell = owner.GetCell(cell.pos + coords[i]);
            if (tmpCell)
            {
                cubes[i].cell = tmpCell;
                if (ownerIsPlayer && ControllerPlayer.instance.ShadowShape != null && ControllerPlayer.instance.ShadowShape.shadowOf == this)
                    tmpCell.unvalidatedCube = cubes[i];
                else
                {
                    if (!tmpCell.cube)
                        tmpCell.cube = cubes[i];
                    else if (tmpCell.cube && tmpCell.cube.OwnerID != ownerID)
                        tmpCell.overlappingCubes.Add(cubes[i]);
                    tmpCell.free = false;
                }
            }
        }
    }

    public bool IsValid()
    {
        foreach(Cube c in cubes)
        {
            if (!c.cell ||
                (!c.cell.free && c.cell.cube.shape != this && c.cell.cube.shape.shadowOf != this && c.cell.cube.shape.ownerID == ownerID))
                return false;
        }
        return true;
    }

    public bool IsValidForCreation()
    {
        foreach(Cube c in cubes)
        {
            if (!c.cell ||
                (!c.cell.free && c.cell.cube.shape != this && c.cell.cube.shape.shadowOf != this && c.cell.cube.shape.ownerID == ownerID) ||
                (c.cell.OwnerID != 0 && c.cell.OwnerID != ownerID))
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
            owner.UnselectShapes();
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
