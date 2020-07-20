using System.Collections;
using System.Collections.Generic;
using UnityEngine;


 public abstract class Action
{
    public GameObject originator { get; set; }
    public GameObject target { get; set; }
    public int duration;
    public abstract void Execute();
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
    public float damage;
    public Attack (int duration, float damage)
    {
        this.duration = duration;
        this.damage = damage;
        return;
    }
    public override void Execute()
    {
        Debug.Log(this.target.name + " Got attacked! They took " + this.damage.ToString() + " Damage");
        this.target.GetComponent<Fighter>().AddToHealth(this.damage * -1);
        return;
    }
}
