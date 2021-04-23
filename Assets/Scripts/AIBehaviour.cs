using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIBehaviour : MonoBehaviour
{
    public enum AIAction
    {
        CREATE = 0,
        MOVE = 1
    }

    protected ControllerOpponent controller;

    protected AIAction action;
    protected Cell targetCell;
    protected Shape shape;

    public int MovingRange { get => controller.MovingRange; }
    public List<Shape> Shapes { get => controller.Shapes; }
    public int ID { get => controller.id; }

    protected virtual void Start()
    {
        if (!EventHandler.instance.aiOpponent)
            EventHandler.instance.aiOpponent = this;
        controller = ControllerOpponent.instance;
    }

    public abstract void CalculateAction();

    public abstract void PerformSelectedAction();
}
