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

    private float minRotation = 0;
    private float maxRotation = 89;

    // Start is called before the first frame update
    void Start()
    {
        SetCameraHeight();

        cellTarget = ControllerPlayer.instance.GetCell(0, 0);
        if (!cellTarget)
            cellTarget = ControllerPlayer.instance.GetAnyCell();
        if ((!cellTarget))
            posTarget = Vector3.zero;
        SetCameraTarget(cellTarget);
    }

    private void SetCameraHeight()
    {
        float w = Camera.main.pixelWidth;
        float h = Camera.main.pixelHeight;
        Vector2 size = ControllerPlayer.instance.GetMapSize();

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
            // Rotate around posTarget
            transform.RotateAround(posTarget, Vector3.up, Input.GetAxis("Mouse X") * 5);
            float yAngle = Vector3.Angle(transform.position - posTarget, new Vector3(transform.position.x, 0, transform.position.z) - posTarget);
            float newAngle = yAngle + Input.GetAxis("Mouse Y") * 5;
            if (newAngle > maxRotation)
                newAngle = maxRotation - yAngle;
            else if (newAngle < minRotation)
                newAngle = minRotation - yAngle;
            else
                newAngle = Input.GetAxis("Mouse Y") * 5;
            transform.RotateAround(posTarget, transform.right, newAngle);
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
