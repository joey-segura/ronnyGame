using System.Collections;
using System.Collections.Generic;
using UnityEngine;


 public class Action
{
    public GameObject originator { get; set; }
    public GameObject target { get; set; }
    public delegate void Affect();
    public Affect method;
    public int duration;
    public float value1, value2;
}
/*
public class Effect
{
    public string name;
    public bool isActive;
    public float value1;
    public float value2;
    public int duration;
}
*/
public class Attack : Action
{   
    public Attack (int duration, float damage)
    {
        this.duration = duration;
        this.value1 = damage;
        this.method = this.Function;
        return;
    }
    public void Function()
    {
        Debug.Log(this.target.name + " Got attacked!");
        this.target.GetComponent<Fighter>().AddToHealth(this.value1 * -1);
        return;
    }
}
