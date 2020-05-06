using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class displayText : MonoBehaviour
{
    public GameObject displayTextPrefab;

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
            Instantiate(displayTextPrefab, transform.position, Quaternion.identity, transform);
        }
    }
}
