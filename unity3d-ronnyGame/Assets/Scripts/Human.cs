﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Fighter
{
    public int virtue;

    public void AddToVirtue(int value)
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
        Debug.Log($"Virtue got changed by {value}!");
        return;
    }
    public float GetVirtue()
    {
        return virtue;
    }
    public int GetLevel()
    {
        //return Mathf.Abs(Mathf.Ceil(((i < 55 ? i : Mathf.Abs(i - 100)) - 4) / 10));
        
        if ((virtue >= 0 && virtue <= 4) || (virtue >= 96 && virtue <= 100))
        {
            return 0;
        } else if ((virtue >= 5 && virtue <= 14) || (virtue >= 86 && virtue <= 95))
        {
            return 1;
        } else if ((virtue >= 15 && virtue <= 24) || (virtue >= 76 && virtue <= 85))
        {
            return 2;
        } else if ((virtue >= 25 && virtue <= 34) || (virtue >= 66 && virtue <= 75))
        {
            return 3;
        } else if ((virtue >= 35 && virtue <= 44) || (virtue >= 56 && virtue <= 65))
        {
            return 4;
        } else
        {
            return 5;
        }
    }
    //public void Check for new abilities etc
}
