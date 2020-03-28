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
            if (lastMousePos != null)
            {
                if (mousePos.x > lastMousePos.x || mousePos.x > Screen.width - (Screen.width / 6))
                {
                    speed = 50f;
                } else if (mousePos.x < lastMousePos.x || mousePos.x < Screen.width / 6)
                {
                    speed = -50f;
                } else
                {
                    if (Mathf.Abs(speed) < 2)
                    {
                        speed = 0f;
                    } else
                    {
                        speed = speed / 1.009f;
                    }
                }
                lastMousePos = mousePos;
            } else
            {
                lastMousePos = mousePos;
            }
        } else
        {
            if (Mathf.Abs(speed) < 2)
            {
                speed = 0f;
            }
            else
            {
                speed = speed / 1.009f;
            }
        }
        transform.Rotate(0, speed * Time.deltaTime, 0);
    }
}
