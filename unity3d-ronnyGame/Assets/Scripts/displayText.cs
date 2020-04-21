using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class displayText : MonoBehaviour
{

    private void Start()
    {
        GameObject.Find("ritter");
        //ritter.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //ritter.SetActive(true);
        }
    }

    void Update()
    {
        
    }
}
