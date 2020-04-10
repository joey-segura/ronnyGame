using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraRotation : MonoBehaviour
{
    public float speed = 100f;

void Update()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0, -speed * Time.deltaTime, 0);
        }
        else
        {
            transform.Rotate(0, 0, 0);
        }
        
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0, speed * Time.deltaTime, 0);
        }
        else
        {
            transform.Rotate(0, 0, 0);
        }
    }
}
