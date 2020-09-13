using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool free = true;
    public Vector3 pos;
    [HideInInspector]
    public Cube cube = null;
    [HideInInspector]
    public Cube unvalidatedCube = null;

    private Color initialColor;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in transform)
            if (child.GetComponent<Renderer>())
            {
                initialColor = child.GetComponent<Renderer>().material.color;
                break;
            }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseEnter()
    {
        Controller.instance.mouseOverCell = this;
        Controller.instance.mouseOverCube = null;
        foreach (Transform child in transform)
            if (child.GetComponent<Renderer>())
                child.GetComponent<Renderer>().material.color = Color.yellow;
    }

    private void OnMouseExit()
    {
        foreach (Transform child in transform)
            if (child.GetComponent<Renderer>())
                child.GetComponent<Renderer>().material.color = initialColor;
        if (Controller.instance.mouseOverCell == this)
            Controller.instance.mouseOverCell = null;
    }
}
