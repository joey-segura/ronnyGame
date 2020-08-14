using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect
{
    public string name { get; set; }
    public int duration { get; set; }
    protected int key = -1;
    protected Fighter self = null;
    public abstract void Affliction(Fighter fighter);
    public abstract void Cleanse(Fighter fighter);
    public virtual void OnTick(Fighter fighter) 
    {
        return;
    }
    public int GenerateRandomKey()
    {
        return Random.Range(0, 1025);
    }
    protected int GenerateValidKeyForEffects(Fighter fighter)
    {
        int newKey = GenerateRandomKey();
        int count = 0;
        while (fighter.GetCurrentEffects().ContainsKey(newKey))
        {
            newKey = GenerateRandomKey();
            count++;
            if (count > 1000)
            {
                Debug.LogError("1000 attempts to distinguish a new Effect Key caused break");
                return -1;
            }
        }
        return newKey;
    }
    protected int GenerateValidKeyForOnHitEffects(Fighter fighter)
    {
        int newKey = GenerateRandomKey();
        int count = 0;
        while(fighter.GetOnHitEffects().ContainsKey(newKey))
        {
            newKey = GenerateRandomKey();
            count++;
            if (count > 1000)
            {
                Debug.LogError("1000 attempts to distinguish a new onHitEffect Key caused break");
                return -1;
            }
        }
        return newKey;
    }
    public int GetKey()
    {
        return key;
    }
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
        this.self = fighter;
        if (key == -1) // don't add another onHitEffect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForEffects(fighter);
            fighter.defenseMultiplier = fighter.defenseMultiplier * this.multiplier;
        }
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.defenseMultiplier = fighter.defenseMultiplier / this.multiplier;
    }
}
public class Poison : Effect
{
    private GameObject causer;
    public int poisonDamage;
    public Poison(int _duration, int _poisonDamage, GameObject _causer)
    {
        this.name = "Poison";
        this.duration = _duration;
        this.poisonDamage = _poisonDamage;
        this.causer = _causer;
    }
    public override void Affliction(Fighter fighter)
    {
        this.self = fighter;
        if (key == -1) // don't add another onHitEffect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForEffects(fighter);
            fighter.isPoisoned = true;
        }
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.isPoisoned = false;
    }
    public override void OnTick(Fighter fighter)
    {
        int virtue = Mathf.RoundToInt(Mathf.Abs(fighter.AddToHealth(this.poisonDamage * -1, fighter)) / 5);
        if (causer.tag == "Party")
        {
            causer.GetComponentInParent<BattleMaster>().AddToVirtue(virtue);
        }
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
        this.self = fighter;
        if (key == -1) // don't add another onHitEffect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForEffects(fighter);
            fighter.damageMultiplier = fighter.damageMultiplier * this.multiplier;
        }
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
        this.self = fighter;
        if (key == -1) // don't add another onHitEffect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForEffects(fighter);
            fighter.isStunned = true;
        }
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.isStunned = false;
    }
}
public class Thorns : Effect
{
    private float percentValue; // between 0 and 1 -- could be more than 1 but /shrug
    public Thorns(int _duration, float _percentValue)
    {
        this.name = "Thorns";
        this.duration = _duration;
        this.percentValue = _percentValue;
    }
    public override void Affliction(Fighter fighter)
    {
        this.self = fighter;
        if (key == -1) // don't add another onHitEffect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForOnHitEffects(fighter);
            fighter.AddToOnHitEffects(this.key, this.Thorn);
        }
    }
    private float Thorn(float change, Fighter causer)
    {
        if (change > -.5f) //don't want an endless cycle and to return positive values in case someone got healed
        {
            return change;
        } else
        {
            causer.AddToHealth(change - (percentValue * change), self);
            if (percentValue > 1)
            {
                return 0;
            } else
            {
                return change - (percentValue * change);
            }
        }
    }
    public override void Cleanse(Fighter fighter)
    {
        if (!fighter.RemoveOnHitEffect(key))
        {
            Debug.LogWarning($"Effect '{this.name}' not found with key {key}");
        }
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
        this.self = fighter;
        if (key == -1) // don't add another onHitEffect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForEffects(fighter);
            fighter.defenseMultiplier = fighter.defenseMultiplier / this.multiplier;
        }
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
        this.self = fighter;
        if (key == -1) // don't add another onHitEffect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForEffects(fighter);
            fighter.damageMultiplier = fighter.damageMultiplier / this.multiplier;
        }
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.damageMultiplier = fighter.damageMultiplier * this.multiplier;
    }
}


