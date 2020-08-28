using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunCycle : MonoBehaviour
{
    public float xRotation = -0.1f, yRotation = 0, zRotation = 0;

    //public bool isDay = false;
    
    //Make sun travel slower during the day(?)
    void Awake()
    {

    }
        
    void Update ()
    {
        transform.Rotate(xRotation, yRotation, zRotation);
    }
}
