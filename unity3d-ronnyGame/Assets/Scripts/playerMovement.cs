using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public float speed = 0f;
    public float angle = 0f;
    public GameObject cameraObject;

    private void Start()
    {
        cameraObject = this.gameObject.transform.GetChild(0).gameObject;
    }
    void Update()
    {
        angle = cameraObject.transform.rotation.eulerAngles.y;
        float x = 0f;
        float z = 0f;
        //Debug.Log(Mathf.Sin(Mathf.Deg2Rad * angle));
        //Debug.Log(Mathf.Cos(Mathf.Deg2Rad * angle));

        if (Input.GetKey(KeyCode.W))
        {
            x += Mathf.Cos(Mathf.Deg2Rad * angle);
            z += Mathf.Sin(Mathf.Deg2Rad * angle);


        } else if (Input.GetKey(KeyCode.S))
        {
            x += Mathf.Cos(Mathf.Deg2Rad * (angle + 180));
            z += Mathf.Sin(Mathf.Deg2Rad * (angle + 180));
        } else
        {
            x += 0;
            z += 0;
        }
        if (Input.GetKey(KeyCode.D))
        {
            x += Mathf.Cos(Mathf.Deg2Rad * (angle + 90));
            z += Mathf.Sin(Mathf.Deg2Rad * (angle + 90));
        }
        else if (Input.GetKey(KeyCode.A))
        {
            x += Mathf.Cos(Mathf.Deg2Rad * (angle - 90));
            z += Mathf.Sin(Mathf.Deg2Rad * (angle - 90));

        }
        else
        {
            x += 0;
            z += 0;
        }
        Debug.Log("z");
        Debug.Log(x + " " + z);
        this.transform.position += new Vector3(z * speed * Time.deltaTime, 0, x * speed * Time.deltaTime);


        //this.transform.position = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle) * speed * Time.deltaTime, this.transform.position.y, Mathf.Sin(Mathf.Deg2Rad * angle) * speed * Time.deltaTime);
    }

}
