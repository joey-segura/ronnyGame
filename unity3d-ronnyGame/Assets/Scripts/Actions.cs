using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


 public abstract class Action
{
    public GameObject originator { get; set; }
    public GameObject target { get; set; }
    public int duration;
    public Animation animation;
    public string[] validTargets;
    public abstract void Execute();
    public abstract float GetDamage();
    public bool IsValidAction(string targetTag)
    {
        if (validTargets.Contains(targetTag))
        {
            return true;
        } else
        {
            return false;
        }
    }
}
public class Attack : Action
{
    public float damage;
    public Attack (int _duration, float _damage, Animation _animation)
    {
        this.duration = _duration;
        this.damage = _damage;
        this.animation = _animation;
        this.validTargets = new string[] { "Foe" };
    }
    public override void Execute()
    {
        Debug.Log($"{this.target.name} Got attacked! They took {this.damage.ToString()} Damage");
        this.target.GetComponent<Fighter>().AddToHealth(this.damage * -1);
        return;
    }
    public override float GetDamage()
    {
        return this.damage;
    }
}
public class PoisonAttack : Action
{
    public int poisonDamage;
    public PoisonAttack (int _duration, int _poisonDamage, Animation _animation)
    {
        this.duration = _duration;
        this.poisonDamage = _poisonDamage;
        this.animation = _animation;
        this.validTargets = new string[] { "Foe" };
    }
    public override void Execute()
    {
        Effect Poison = new Poison(this.duration, this.poisonDamage);
        this.target.GetComponent<Fighter>().AddEffect(Poison);
    }
    public override float GetDamage()
    {
        return this.poisonDamage;
    }
}
