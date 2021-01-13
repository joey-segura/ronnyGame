using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraRotation : MonoBehaviour
{
    public float speed;
    private Vector3 lastMousePos;

void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            speed = 50f;
        } else if (Input.GetKey(KeyCode.E))
        {
            speed = -50f;
        } else if (Input.GetMouseButton(1))
        {
            Vector3 mousePos = Input.mousePosition;
            if (lastMousePos != null && lastMousePos != mousePos)
            {
                if (mousePos.x > lastMousePos.x)
                {
                    speed = (mousePos.x - lastMousePos.x) * 25;
                } else if (mousePos.x < lastMousePos.x)
                {
                    speed = (mousePos.x - lastMousePos.x) * 25;
                } 
                lastMousePos = mousePos;
            } else
            {
                lastMousePos = mousePos;
                speed = 0f;
            }
        } else
        {
            lastMousePos = Input.mousePosition;
            speed = 0f;
        }
        transform.Rotate(0, speed * Time.deltaTime, 0);
    }
}
