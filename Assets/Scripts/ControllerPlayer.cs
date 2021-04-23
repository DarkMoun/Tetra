using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerPlayer : Controller
{
    public static ControllerPlayer instance;

    public Button validateButton;

    protected Shape shadowShape = null;

    [HideInInspector]
    public Cell mouseOverCell;
    protected Cell mouseDownCell = null;

    [HideInInspector]
    public Cube mouseOverCube;
    protected Cube mouseDownCube = null;
    protected Cube draggedCube = null;

    public Shape ShadowShape { get => shadowShape; }

    protected void Awake()
    {
        if (Application.isPlaying)
        {
            if (!instance)
                instance = this;
            else
                enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        #region Shape Creation/Selection
        if (previewShape == null)
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
                if (mouseOverCell.free && (mouseOverCell.OwnerID == id || mouseOverCell.OwnerID == 0))
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
            else if (mouseOverCube && mouseOverCube.shape.OwnerID == id && mouseOverCube == mouseDownCube && mouseOverCube.shape != shadowShape)
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
            if (mouseOverCube && mouseOverCube.shape.OwnerID == id)
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

    public Shape GenerateShadowShape(Shape shape)
    {
        if (shape != null && (shadowShape == null || shape != shadowShape.shadowOf))
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

    public override void DeleteShape(Shape shape)
    {
        base.DeleteShape(shape);

        //if the shape has a shadow, set the shadow back to normal
        if (shadowShape != null && shadowShape.shadowOf == shape)
        {
            shadowShape.shadowOf = null;
            Color c = shadowShape.Color;
            shadowShape.Color = new Color(c.r, c.g, c.b, 1f);
            shadowShape = null;
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

    public void ValidateAction()
    {
        if (shadowShape != null)
        {
            foreach (Cube cube in shadowShape.shadowOf.cubes)
                if (cube.cell)
                {
                    if (cube.cell.cube && cube.cell.cube.OwnerID != cube.OwnerID)
                        cube.cell.overlappingCubes.Add(cube);
                    else
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

            EventHandler.instance.OnValidateAction();
        }
    }
}
