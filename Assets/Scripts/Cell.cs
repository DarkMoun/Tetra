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

    private int ownerID = 0;
    private bool isTarget = false;
    private bool isNeutral = false;
    public GameObject targetDisplayer;
    public List<Renderer> renderers;

    private Color initialColor;

    public int OwnerID { 
        get => ownerID; 
        set {
            ownerID = value;
            if(!isNeutral && ownerID < 3)
            {
                // change cell color to owner color
                Color c = Controller.instance.playersColors[ownerID - 1];
                initialColor = new Color(c.r, c.g, c.b, 0.3f);
                SetColor(initialColor);
            }
        } 
    }

    public bool IsTarget { 
        get => isTarget;
        set
        {
            isTarget = value;
            if (!isNeutral)
            {
                // change transparancy depending on value
                initialColor = new Color(initialColor.r, initialColor.g, initialColor.b, isTarget ? 0.8f : 0.3f);
                SetColor(initialColor);
            }
        }
    }

    public bool IsNeutral { 
        get => isNeutral;
        set
        {
            isNeutral = value;
            // change color to grey if true
            if (isNeutral)
            {
                initialColor = new Color(Color.grey.r, Color.grey.g, Color.grey.b, 0.3f);
                SetColor(initialColor);
            }
        }
    }

    private void OnMouseEnter()
    {
        Controller.instance.mouseOverCell = this;
        Controller.instance.mouseOverCube = null;
        SetColor(Color.yellow);
    }

    private void OnMouseExit()
    {
        SetColor(initialColor);
        if (Controller.instance.mouseOverCell == this)
            Controller.instance.mouseOverCell = null;
    }

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

    private void SetColor(Color c)
    {
        if (renderers == null)
            renderers = new List<Renderer>();
        foreach (Renderer r in renderers)
            r.material.color = c;
    }
}
