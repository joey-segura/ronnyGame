﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect
{
    public string name { get; set; }
    public string description;
    public int duration { get; set; }
    protected int key = -1;
    public Fighter self = null;
    public abstract void Affliction(Fighter fighter); // actually implement effect changes
    public abstract void Cleanse(Fighter fighter); // clears affliction (usually called sometime after affliction)
    public virtual void OnTick(Fighter fighter) //ticks at the start of every turn
    {
        return;
    }
    private int GenerateRandomKey()
    {
        return Random.Range(0, 1025);
    }
    protected int GenerateValidKeyForEffects(Fighter fighter)
    {
        int newKey = GenerateRandomKey();
        int count = 0;
        while (fighter.currentEffects.ContainsKey(newKey))
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
        while(fighter.onHitEffects.ContainsKey(newKey))
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
public class Berserk : Effect
{
    public int toll;
    public Berserk(int _duration, int _toll)
    {
        this.name = "Berserk";
        this.description = $"Takes {_toll} each turn from fighter's health in exchange for increase in damage";
        this.duration = _duration;
        this.toll = _toll;
    }
    public override void Affliction(Fighter fighter)
    {
        this.self = fighter;
        if (key == -1)
        {
            this.key = this.GenerateValidKeyForEffects(fighter);
            fighter.damage += toll;
            OnTick(fighter);
        }
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.damage -= toll;
    }
    public override void OnTick(Fighter fighter)
    {
        fighter.AddToHealth(toll * -1, fighter);
    }
}
public class Block : Effect
{
    public Block()
    {
        this.name = "Block";
        this.description = $"Block all incoming damage for 1 turn";
        this.duration = 1;
    }
    public override void Affliction(Fighter fighter)
    {
        this.self = fighter;
        if (key == -1) // don't add another onHitEffect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForOnHitEffects(fighter);
            fighter.AddToOnHitEffects(this.key, this.BlockDamage);
        }
    }
    private int BlockDamage(int change, Fighter causer)
    {
        return 0;
    }
    public override void Cleanse(Fighter fighter)
    {
        if (!fighter.RemoveOnHitEffect(key))
        {
            Debug.LogWarning($"Effect '{this.name}' not found with key {key}");
        }
    }
}
public class Bolster : Effect
{
    public int additive;
    public Bolster(int _duration, int _additive)
    {
        this.name = "Bolster";
        this.duration = _duration;
        this.additive = _additive;
        this.description = $"Increases defense by {this.additive} for {this.duration}";
    }
    public override void Affliction(Fighter fighter)
    {
        this.self = fighter;
        if (key == -1) // don't add another onHitEffect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForEffects(fighter);
            fighter.defense = fighter.defense + this.additive;
        }
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.defense = fighter.defense - this.additive;
    }
}
public class LifeSteal : Effect
{
    public LifeSteal(int _duration)
    {
        this.name = "LifeSteal";
        this.description = $"Converts all damage dealt by this fighter to heal itself";
        this.duration = _duration;
    }
    public override void Affliction(Fighter fighter)
    {
        this.self = fighter;
        if (key == -1) // don't add another onHitEffect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForOnHitEffects(fighter);
            fighter.AddToOnAttackEffects(this.key, this.Steal);
        }
    }
    private int Steal(int change, Fighter self)
    {
        self.AddToHealth(Mathf.Abs(change), self);
        return 0;
    }
    public override void Cleanse(Fighter fighter)
    {
        if (!fighter.RemoveOnAttackEffect(key))
        {
            Debug.LogWarning($"Effect '{this.name}' not found with key {key}");
        }
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
        this.description = $"Deals {this.poisonDamage} each turn for {this.duration} turns";
        this.causer = _causer;
    }
    public override void Affliction(Fighter fighter)
    {
        this.self = fighter;
        if (key == -1) // don't add another Effect if we already have a key assigned
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
        fighter.AddToHealth(this.poisonDamage * -1, fighter);
    }
}
public class Strengthen : Effect
{
    public int additive;
    public Strengthen(int _duration, int _additive)
    {
        this.name = "Strengthen";
        this.duration = _duration;
        this.additive = _additive;
        this.description = $"Increases fighters damage by {this.additive} for {this.duration}";
    }
    public override void Affliction(Fighter fighter)
    {
        this.self = fighter;
        if (key == -1) // don't add another Effect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForEffects(fighter);
            fighter.damage += this.additive;
        }
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.damage -= this.additive;
    }
}
public class Stun : Effect
{
    public Stun(int _duration)
    {
        this.name = "Stun";
        this.duration = _duration;
        this.description = $"Fighter's turn is skipped for {this.duration} turns";
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
    private int Thorn(int change, Fighter causer)
    {
        if (change > 1) //don't want an endless cycle and to return positive values in case someone got healed
        {
            return change;
        } else
        {
            causer.AddToHealth(Mathf.FloorToInt(change - (percentValue * change)), self);
            if (percentValue > 1)
            {
                return 0;
            } else
            {
                return Mathf.FloorToInt(change - (percentValue * change));
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
    public int additive;
    public Vulnerable(int _duration, int _additive)
    {
        this.name = "Vulnerable";
        this.duration = _duration;
        this.additive = _additive;
        this.description = $"Lower Fighter's defense by {this.additive} for {this.duration}";
    }
    public override void Affliction(Fighter fighter)
    {
        this.self = fighter;
        if (key == -1) // don't add another onHitEffect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForEffects(fighter);
            fighter.defense = fighter.defense - this.additive;
        }
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.defense = fighter.defense + this.additive;
    }
}
public class Weak : Effect
{
    public int value;
    public Weak(int _duration, int _value)
    {
        this.name = "Weak";
        this.duration = _duration;
        this.value = _value;
        this.description = $"Decreased damage by {this.value} for {this.duration} turns";
    }
    public override void Affliction(Fighter fighter)
    {
        this.self = fighter;
        if (key == -1) // don't add another onHitEffect if we already have a key assigned
        {
            this.key = this.GenerateValidKeyForEffects(fighter);
            fighter.damage -= this.value;
        }
    }
    public override void Cleanse(Fighter fighter)
    {
        fighter.damage += this.value;
    }
}


