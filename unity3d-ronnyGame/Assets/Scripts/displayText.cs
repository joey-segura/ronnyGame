using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;



public class displayText : MonoBehaviour
{
    public GameObject displayTextPrefab;

    private void Start()
    {
        displayTextPrefab = GameObject.Find("textBox");
    }

    void OnTriggerEnter(Collider ronny)
    {
        displayTextPrefab.GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }

    private void OnTriggerExit(Collider ronny)
    {
        displayTextPrefab.GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }
}
