using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerOpponent : Controller
{
    public static ControllerOpponent instance;

    public Cell PreviewCell { get => previewCell; }

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

    }
}
