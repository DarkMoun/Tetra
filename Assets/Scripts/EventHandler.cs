using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventHandler : MonoBehaviour
{
    public static EventHandler instance;

    private Dictionary<int, Controller> players;

    public AIBehaviour aiOpponent;

    private List<Shape> tmpShapes;

    private bool aiFirstActionCalculated = false;

    public Dictionary<int, Controller> Players { get => players; }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (!instance)
                instance = this;

            players = new Dictionary<int, Controller>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        tmpShapes = new List<Shape>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnValidateAction()
    {
        if (aiOpponent)
        {
            if (!aiFirstActionCalculated)
            {
                aiOpponent.CalculateAction();
                aiFirstActionCalculated = true;
            }

            aiOpponent.PerformSelectedAction();
            aiOpponent.CalculateAction();

            CheckCollisions();
        }
    }

    public void CheckCollisions()
    {
        tmpShapes.Clear();
        foreach(Controller controller in players.Values)
        {
            foreach (Shape shape in controller.Shapes)
            {
                if (!tmpShapes.Contains(shape) && shape.Cell != controller.PreviewCell)
                {
                    foreach (Cube cube in shape.cubes)
                    {
                        if (cube.cell.overlappingCubes.Count > 0)
                        {
                            tmpShapes.Add(shape);
                            foreach (Cube c in cube.cell.overlappingCubes)
                                if (!tmpShapes.Contains(c.shape))
                                    tmpShapes.Add(c.shape);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < tmpShapes.Count; i++)
            tmpShapes[i].Owner.DeleteShape(tmpShapes[i]);
    }
}
