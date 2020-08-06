using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect
{
    public string name { get; set; }
    public int duration { get; set; }
    public abstract void Affliction(Fighter fighter);
    public abstract void Cleanse(Fighter fighter);
}
public class Bolster : Effect
{
    public float multiplier;
    public Bolster(int _duration, float _multiplier)
    {
        this.name = "Bolster";
        this.duration = _duration;
        this.multiplier = _multiplier;
    }
    public override void Affliction(Fighter fighter)
    {
        fighter.defenseMultiplier = fighter.defenseMultiplier * this.multiplier;
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.defenseMultiplier = fighter.defenseMultiplier / this.multiplier;
    }
}
public class Poison : Effect
{
    public int poisonDamage;
    public Poison(int _duration, int _poisonDamage)
    {
        this.name = "Poison";
        this.duration = _duration;
        this.poisonDamage = _poisonDamage;
    }
    public override void Affliction(Fighter fighter)
    {
        fighter.isPoisoned = true;
        fighter.AddToHealth(this.poisonDamage * -1);
        Debug.Log($"{fighter.gameObject.name} has been poisoned for {this.poisonDamage}!");
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.isPoisoned = false;
    }
}
public class Strengthen : Effect
{
    public float multiplier;
    public Strengthen(int _duration, float _multiplier)
    {
        this.name = "Strengthen";
        this.duration = _duration;
        this.multiplier = _multiplier;
    }
    public override void Affliction(Fighter fighter)
    {
        fighter.damageMultiplier = fighter.damageMultiplier * this.multiplier;
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.damageMultiplier = fighter.damageMultiplier / this.multiplier;
    }
}
public class Stun : Effect
{
    public Stun(int _duration)
    {
        this.name = "Stun";
        this.duration = _duration;
    }
    public override void Affliction(Fighter fighter)
    {
        fighter.isStunned = true;
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.isStunned = false;
    }
}

public class Vulnerable : Effect
{
    public float multiplier;
    public Vulnerable(int _duration, float _multiplier)
    {
        this.name = "Vulnerable";
        this.duration = _duration;
        this.multiplier = _multiplier;
    }
    public override void Affliction(Fighter fighter)
    {
        fighter.defenseMultiplier = fighter.defenseMultiplier / this.multiplier;
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.defenseMultiplier = fighter.defenseMultiplier * this.multiplier;
    }
}
public class Weak : Effect
{
    public float multiplier;
    public Weak(int _duration, float _multiplier)
    {
        this.name = "Weak";
        this.duration = _duration;
        this.multiplier = _multiplier;
    }
    public override void Affliction(Fighter fighter)
    {
        fighter.damageMultiplier = fighter.damageMultiplier / this.multiplier;
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.damageMultiplier = fighter.damageMultiplier * this.multiplier;
    }
}


