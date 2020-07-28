using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMaster : Kami
{
    public void Say(Canvas canvas, string[] message, int duration)
    {
        GameObject being = canvas.gameObject;
        //set text in field to message for duration and then set the canvas to be active
        //being.transform.c
        being.SetActive(true);
    }
}
