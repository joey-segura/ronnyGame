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
public class Attack : Action
{
    public float damage;
    public Attack (int _duration, float _damage)
    {
        this.duration = _duration;
        this.damage = _damage;
        return;
    }
    public override void Execute()
    {
        Debug.Log(this.target.name + " Got attacked! They took " + this.damage.ToString() + " Damage");
        this.target.GetComponent<Fighter>().AddToHealth(this.damage * -1);
        return;
    }
}
public class Poision : Action
{
    public int poisonDamage;
    public Poision (int _duration, int _poisonDamage)
    {
        this.duration = _duration;
        this.poisonDamage = _poisonDamage;
    }
    public override void Execute()
    {
        Effect Poison = new Poison(this.duration, this.poisonDamage);
        this.target.GetComponent<Fighter>().AddEffect(Poison);
    }
}
