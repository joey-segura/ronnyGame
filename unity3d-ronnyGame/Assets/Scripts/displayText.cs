using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;



public class displayText : MonoBehaviour
{
    public GameObject displayTextPrefab;
    public Vector3 cam;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "ronny")
        {
            ShowpopUpText();
            
            Debug.Log("Hit");
        }
    }
    private void ShowpopUpText()
    {
        {
            Instantiate(displayTextPrefab, new Vector3((float) 0.85,(float) 2.2,(float) 0.6), Quaternion.LookRotation(Vector3.up));
        }
    }
}
