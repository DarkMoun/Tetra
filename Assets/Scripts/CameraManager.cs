using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Cell cellTarget;
    private Vector3 posTarget = Vector3.zero;

    public float minZoom = 5;
    public float maxZoom = 20;

    // Start is called before the first frame update
    void Start()
    {
        SetCameraHeight();

        cellTarget = Controller.instance.GetCell(0, 0);
        if (!cellTarget)
            cellTarget = Controller.instance.GetAnyCell();
        if ((!cellTarget))
            posTarget = Vector3.zero;
        SetCameraTarget(cellTarget);
    }

    private void SetCameraHeight()
    {
        float w = Camera.main.pixelWidth;
        float h = Camera.main.pixelHeight;
        Vector2 size = Controller.instance.GetMapSize();

        if (size.x / w > size.y / h)
            //adjust to x
            Camera.main.transform.position += Vector3.up * (size.x / w * h * 6 / 5.427332f - Camera.main.transform.position.y);
        else
            //adjust to y
            Camera.main.transform.position += Vector3.up * (size.y / w * h * 6 / 2.261388f - Camera.main.transform.position.y);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            transform.LookAt(posTarget);
            transform.RotateAround(posTarget, Vector3.up, Input.GetAxis("Mouse X") * 5);
            transform.RotateAround(posTarget, Vector3.right, Input.GetAxis("Mouse Y") * 5);
        }

        float cameraDist = (transform.position - posTarget).magnitude;
        float scrollValue = Input.mouseScrollDelta.y;
        if ((scrollValue > 0 && cameraDist > minZoom) || (scrollValue < 0 && cameraDist < maxZoom))
            transform.position += transform.forward * scrollValue;
    }

    private void SetCameraTarget(Cell c)
    {
        if (c)
        {
            Camera.main.transform.position += c.transform.position - posTarget;
            posTarget = c.transform.position;
        }
    }
}
