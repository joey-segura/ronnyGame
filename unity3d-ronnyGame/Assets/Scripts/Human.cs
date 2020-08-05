using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Fighter
{
    public float virtue;

    public void AddToVirtue(float value)
    {
        this.virtue += value;
        if (this.virtue < 0)
        {
            this.virtue = 0;
        }
        else if (this.virtue > 100)
        {
            this.virtue = 100;
        }
        return;
    }
    public float GetVirtue()
    {
        return virtue;
    }
    //public void Check for new abilities etc
}
