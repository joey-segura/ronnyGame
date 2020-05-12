using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;



public class displayText : MonoBehaviour
{
    public GameObject displayTextPrefab;
    public Text testText;
    public int value = 0;

    private void Start()
    {
        displayTextPrefab = GameObject.Find("textBox");
    }

    private void Update()
    {
        testText.text = value.ToString();

        value++;
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
