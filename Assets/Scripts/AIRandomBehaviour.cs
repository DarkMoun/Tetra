using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIRandomBehaviour : AIBehaviour
{
    private System.Random random;

    private List<Cell> tmpCells;
    private Cell tmpCell;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        tmpCells = new List<Cell>();
        random = new System.Random();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void CalculateAction()
    {
        if(controller.previewShape == null)
        {
            // create preview shape
            controller.previewShape = controller.GenerateRandomShape(controller.PreviewCell);
            foreach (Cube c in controller.previewShape.cubes)
                c.gameObject.layer = 8;
            controller.previewShape.previewShape = false;
            controller.previewShape.Move(controller.previewShape.Cell);
            Shapes.Remove(controller.previewShape);
        }

        if(Shapes.Count < 2)
        {
            action = AIAction.CREATE;

            shape = controller.previewShape;
        }
        else
        {
            action = AIAction.MOVE;

            // get random shape
            shape = Shapes[random.Next(Shapes.Count)];
        }

        // calculate destination cell
        targetCell = GetRandomValidCell(shape, action);
    }

    private Cell GetRandomValidCell(Shape s, AIAction a)
    {
        if (s == null)
            return null;

        tmpCells.Clear();


        if (a == AIAction.CREATE)
        {
            tmpCell = s.Cell;
            foreach (Cell c in Map.instance.Cells)
            {
                if (c.OwnerID == ID || c.OwnerID == 0)
                {
                    s.Cell = c;
                    if (s.IsValidForCreation())
                        tmpCells.Add(c);
                }
            }
            s.Cell = tmpCell;
        }
        else if (a == AIAction.MOVE)
        {
            tmpCell = s.Cell;
            foreach (Cell c in Map.instance.Cells)
            {
                if (Math.Abs(tmpCell.pos.x - c.pos.x) + Math.Abs(tmpCell.pos.z - c.pos.z) <= MovingRange && c != tmpCell)
                {
                    s.Cell = c;
                    if (s.IsValid())
                        tmpCells.Add(c);
                }
            }
            s.Cell = tmpCell;
        }

        if (tmpCells.Count > 0)
            return tmpCells[random.Next(tmpCells.Count)];

        return null;
    }

    public override void PerformSelectedAction()
    {
        shape.Cell = targetCell;

        if(action == AIAction.CREATE)
        {
            shape.RemoveUILayer();
            if(!Shapes.Contains(shape))
                Shapes.Add(shape);
            controller.previewShape = null;
        }
    }
}
