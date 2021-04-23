using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public static float cubeScale;

    public Shape shape = null;
    public Cell cell = null;
    public Vector3 coords;

    private new Renderer renderer;
    private Color initialColor;

    private int ownerID = 0;

    [SerializeField]
    private SelectedBorder border = null;

    public int OwnerID { get => ownerID; set => ownerID = value; }

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
        if (renderer)
            initialColor = renderer.material.color;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnMouseEnter()
    {
        ControllerPlayer.instance.mouseOverCube = this;
        ControllerPlayer.instance.mouseOverCell = null;
        if (renderer && shape.shadowOf == null)
        {
            if (border.renderers.Count > 0)
                renderer.material.color = border.renderers[0].material.color;
            else
                renderer.material.color = Color.black;
        }
    }

    private void OnMouseExit()
    {
        if (renderer && shape.shadowOf == null)
            renderer.material.color = initialColor;
        if (ControllerPlayer.instance.mouseOverCube == this)
            ControllerPlayer.instance.mouseOverCube = null;
    }

    public void ChangeBorderColor(Color c)
    {
        if (border)
            border.ChangeColor(c);
    }

    public void SetSelected()
    {
        if(border)
            border.gameObject.SetActive(true);
    }

    public void SetUnselected()
    {
        if(border)
            border.gameObject.SetActive(false);
    }
}
