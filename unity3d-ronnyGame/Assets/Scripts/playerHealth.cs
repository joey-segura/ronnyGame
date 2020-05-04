using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerHealth : MonoBehaviour
{
    public Slider health;
    
    void Start()
    {
        health.value = 100;
    }
    
    void Update()
    {
        if (Input.GetKey(KeyCode.P))
        {
            health.value += 1;
        }
        if (Input.GetKey(KeyCode.O))
        {
            health.value -= 1;
        }
    }
}
