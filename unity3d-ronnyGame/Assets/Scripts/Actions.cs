﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class FighterAction
{
    public GameObject originator { get; set; }
    public GameObject[] targets { get; set; }
    public int duration, effectDuration, targetCount, skillLevel = 1, levelCap;
    public Animation animation;
    public string name { get; set; }
    public string description;
    public string[] validTargets;

    public abstract FighterAction Clone();
    public GameObject[] GetAOETargets(ListBeingData list)
    {
        List<GameObject> targets = new List<GameObject>();
        Fighter self = originator.GetComponent<Fighter>();
        if (targetCount == 0) // get all enemies
        {
            for (int i = 0; i < list.BeingDatas.Count; i++)
            {
                if (self.TargetRelationToSelf(list.BeingDatas[i].gameObject) == "Foe")
                {
                    targets.Add(list.BeingDatas[i].gameObject);
                }
            }
        } else if (targetCount == -1)
        {
            for (int i = 0; i < list.BeingDatas.Count; i++)
            {
                if (originator != list.BeingDatas[i].gameObject)
                {
                    targets.Add(list.BeingDatas[i].gameObject);
                }
            }
        } else if (targetCount == -2) // get all allies
        {
            for (int i = 0; i < list.BeingDatas.Count; i++)
            {
                if (originator.tag == list.BeingDatas[i].gameObject.tag 
                    || (originator.tag == "Party" && list.BeingDatas[i].gameObject.tag == "Player") 
                    || (originator.tag == "Player" && list.BeingDatas[i].gameObject.tag == "Party")) // have to handle all cases :(
                {
                    targets.Add(list.BeingDatas[i].gameObject);
                }
            }
        }
        return targets.ToArray();
    }
    public abstract IEnumerator Execute();
    public virtual int GetCost()
    {
        return 0;
    }
    public virtual float GetEffectValue()
    {
        return 0;
    }
    public VisualEffectMaster GetVisualEffectMaster()
    {
        if (this.originator)
        {
            return this.originator.GetComponentInParent<VisualEffectMaster>();
        }
        {
            Debug.LogError($"ERROR - {this.name} could not locate its originator to then return the visual effect master");
            return null;
        }
    }
    public bool IsActionAOE()
    {
        if (targetCount != 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
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
    public virtual void LevelUpdate(){}
    public virtual void ReevaluateActionValues(Fighter self){}
}
public class CoroutineWithData
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public bool finished = false;
    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            if (target.Current is UnityEngine.WaitUntil)
            {
                result = null; 
            } else if (target.Current is UnityEngine.WaitForEndOfFrame)
            {
                result = null;
            } else if (target.Current is bool)
            {
                if ((bool)target.Current)
                {
                    finished = true;
                    yield return finished;
                }
            }
            yield return result;
        }
    }
}
public class Attack : FighterAction
{
    public int damage;
    public Attack (int _duration, int _damage, Animation _animation)
    {
        this.name = "Attack";
        this.description = $"Attack for {_damage}";
        this.duration = _duration;
        this.damage = _damage;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Foe" };
    }
    public override FighterAction Clone()
    {
        return new Attack(this.duration, this.damage, this.animation);
    }
    public override IEnumerator Execute()
    {

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].GetComponent<Fighter>().AddToHealth(this.damage * -1, this.originator.GetComponent<Fighter>());
        }
        yield return true;
    }
    public override void ReevaluateActionValues(Fighter self)
    {
        this.damage = Mathf.FloorToInt(self.damage * self.damageMultiplier);
    }
}
public class AttackAndBuff : FighterAction
{
    public int buffValue;
    public int damage;
    public AttackAndBuff(int _duration, int _damage, int _effectDuration, int _buffValue, Animation _animation)
    {
        this.name = "Attack and Buff";
        this.description = $"Attack for {_damage} and buff target for {_buffValue}";
        this.duration = _duration;
        this.damage = _damage;
        this.effectDuration = _effectDuration;
        this.buffValue = _buffValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Foe" };
    }
    public override FighterAction Clone()
    {
        return new AttackAndBuff(this.duration, this.damage, this.effectDuration, this.buffValue, this.animation);
    }
    public override IEnumerator Execute()
    {
        Effect strengthen = new Strengthen(this.effectDuration, this.buffValue);
        for (int i = 0; i < targets.Length; i++)
        {
            Fighter fighter = targets[i].GetComponent<Fighter>();
            fighter.AddToHealth(this.damage * -1, this.originator.GetComponent<Fighter>());
            fighter.AddEffect(fighter, strengthen);
        }
        yield return true;
    }
    public override float GetEffectValue()
    {
        return this.buffValue;
    }
    public override void ReevaluateActionValues(Fighter self)
    {
        this.damage = Mathf.FloorToInt(self.damage * self.damageMultiplier);
    }
}
public class ApplyThorns : FighterAction
{
    public float percentValue;
    public ApplyThorns(int _duration, int _effectDuration, float _percentValue, Animation _animation)
    {
        this.name = "Thorns";
        this.description = $"Reflect incoming damage by {_percentValue}";
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.percentValue = _percentValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Friend", "Self" };
    }
    public override FighterAction Clone()
    {
        return new ApplyThorns(this.duration, this.effectDuration, this.percentValue, this.animation);
    }
    public override IEnumerator Execute()
    {
        Effect thorns = new Thorns(this.effectDuration, this.percentValue);
        for (int i = 0; i < targets.Length; i++)
        {
            Fighter fighter = targets[i].GetComponent<Fighter>();
            fighter.AddEffect(fighter, thorns);
        }
        yield return true;
    }
}
public class Berserker : FighterAction
{
    public int tollValue;
    public Berserker(int _duration, int _effectDuration, int _tollValue, Animation _animation)
    {
        this.name = "Berserker";
        this.description = $"Gain +{_tollValue} damage by paying {_tollValue} per turn for {_effectDuration} turns";
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.tollValue = _tollValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Self" };
    }
    public override FighterAction Clone()
    {
        return new Berserker(this.duration, this.effectDuration, this.tollValue, this.animation);
    }
    public override IEnumerator Execute()
    {
        Effect bers = new Berserk(this.effectDuration, this.tollValue);
        for (int i = 0; i < targets.Length; i++)
        {
            Fighter fighter = targets[i].GetComponent<Fighter>();
            fighter.AddEffect(fighter, bers);
        }
        yield return true;
    }
    public override float GetEffectValue()
    {
        return this.tollValue;
    }
}
public class BlockAll : FighterAction
{
    public BlockAll(int _duration, Animation _animation)
    {
        this.name = "Block";
        this.description = $"Cause target to become invulnerable!";
        this.duration = _duration;
        this.effectDuration = 1;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Friend", "Self" };
    }
    public override FighterAction Clone()
    {
        return new BlockAll(this.duration, this.animation);
    }
    public override IEnumerator Execute()
    {
        Effect block = new Block();
        for (int i = 0; i < targets.Length; i++)
        {
            Fighter fighter = targets[i].GetComponent<Fighter>();
            fighter.AddEffect(fighter, block);
        }
        yield return true;
    }
}
public class BolsterDefense : FighterAction
{
    public int buffValue, baseValue, baseDuration, cost;
    public BolsterDefense(int _duration, int _effectDuration , int _buffValue, Animation _animation)
    {
        this.name = "Bolster Defense";
        this.description = $"Increase defense by {_buffValue} for {_effectDuration} turns";
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.baseDuration = _effectDuration;
        this.buffValue = _buffValue;
        this.baseValue = _buffValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.levelCap = 3;
        this.validTargets = new string[] { "Friend" , "Foe"};
    }
    public override FighterAction Clone()
    {
        return new BolsterDefense(this.duration, this.effectDuration, this.buffValue, this.animation);
    }
    public override IEnumerator Execute()
    {
        Effect bolster = new Bolster(this.effectDuration, this.buffValue);
        for (int i = 0; i < targets.Length; i++)
        {
            Fighter fighter = targets[i].GetComponent<Fighter>();
            fighter.AddEffect(fighter, bolster);
        }
        yield return true;
    }
    public override int GetCost()
    {
        return cost;
    }
    public override float GetEffectValue()
    {
        return this.buffValue;
    }
    public override void LevelUpdate()
    {
        switch (skillLevel)
        {
            case 1:
                name = "Bolster Defense";
                cost = 6;
                buffValue = baseValue;
                break;
            case 2:
                name = "Bolster Defense+";
                cost = 8;
                buffValue = baseValue + 1;
                effectDuration = baseDuration;
                break;
            case 3:
                name = "Bolster Defense++";
                cost = 10;
                buffValue = baseValue + 2;
                effectDuration = baseDuration + 1;
                break;
        }
        description = $"Increase defense by {buffValue} for {effectDuration} turns";
    }
}
public class BuffAttack : FighterAction
{
    public int buffValue, baseValue, baseDuration, cost;
    public BuffAttack(int _duration, int _effectDuration , int _buffValue, Animation _animation)
    {
        this.name = "Buff Attack";
        this.description = $"Buff Attack for {_buffValue} for {_effectDuration} turns";
        this.cost = 6;
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.baseDuration = _effectDuration;
        this.buffValue = _buffValue;
        this.baseValue = _buffValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.levelCap = 3;
        this.validTargets = new string[] { "Foe", "Friend" };
    }
    public override FighterAction Clone()
    {
        return new BuffAttack(this.duration, this.effectDuration, this.buffValue, this.animation);
    }
    public override IEnumerator Execute()
    {
        Effect strengthen = new Strengthen(this.effectDuration, this.buffValue);
        for (int i = 0; i < targets.Length; i++)
        {
            Fighter fighter = targets[i].GetComponent<Fighter>();
            fighter.AddEffect(fighter, strengthen);
        }
        yield return true;
    }
    public override float GetEffectValue()
    {
        return this.buffValue;
    }
    public override int GetCost()
    {
        return cost;
    }
    public override void LevelUpdate()
    {
        switch (skillLevel)
        {
            case 1:
                name = "Buff Attack";
                buffValue = baseValue;
                cost = 6;
                break;
            case 2:
                name = "Buff Attack+";
                buffValue = baseValue + 1;
                cost = 8;
                effectDuration = baseDuration;
                break;
            case 3:
                name = "Buff Attack++";
                buffValue = baseValue + 2;
                cost = 10;
                effectDuration = baseDuration + 1;
                break;
        }
        this.description = $"Buff attack by {buffValue} for {effectDuration} turns";
    }
}
public class Cleave : FighterAction
{
    public int damage;
    public Cleave(int _duration, int _damage, Animation _animation)
    {
        this.name = "Cleave";
        this.description = $"Attack all targets for {_damage}";
        this.duration = _duration;
        this.damage = _damage;
        this.animation = _animation;
        this.targetCount = 0; // all enemies
        this.validTargets = new string[] { "Foe" };
    }
    public override FighterAction Clone()
    {
        return new Cleave(this.duration, this.damage, this.animation);
    }
    public override IEnumerator Execute()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].GetComponent<Fighter>().AddToHealth(this.damage * -1, this.originator.GetComponent<Fighter>());
        }
        yield return true;
    }
    public override void ReevaluateActionValues(Fighter self)
    {
        this.damage = Mathf.FloorToInt(self.damage * self.damageMultiplier);
    }
}
public class CommandToAttack : FighterAction
{
    public GameObject attackTarget;
    public CommandToAttack(int _duration, Animation _animation)
    {
        this.name = "Command to Attack";
        this.description = $"Command a target to attack following selected unit";
        this.duration = _duration;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Friend", "Foe" };
    }
    public override FighterAction Clone()
    {
        return new CommandToAttack(this.duration, this.animation);
    }
    public override IEnumerator Execute()
    {
        if (!this.originator.name.Contains("Shadow"))
        {
            Ronny ronny = this.originator.GetComponent<Ronny>();
            Fighter fighter = null;
            GameObject targ = null;
            for (int i = 0; i < targets.Length; i++)
            {
                fighter = targets[i].GetComponent<Fighter>();
                targ = targets[i];
            }
            Attack attack = new Attack(3, Mathf.FloorToInt(fighter.damage * fighter.damageMultiplier), null);
            GameObject selectedTarget = null;
            Debug.Log("Please select a fighter!");
            while (selectedTarget == null)
            {
                selectedTarget = ronny.ReturnChoosenGameObject();
                yield return new WaitForEndOfFrame();
            }
            attack.targets = new GameObject[] { selectedTarget };
            attack.originator = targ;
            fighter.SetAction(attack);
            yield return true;
        } else
        {
            yield return true;
        }
    }
    public override int GetCost()
    {
        return 10;
    }
}
public class CommandToBlock : FighterAction
{
    public CommandToBlock(int _duration, Animation _animation)
    {
        this.name = "Command to Block";
        this.description = $"Command target to block on their turn";
        this.duration = _duration;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Friend", "Foe" };
    }
    public override FighterAction Clone()
    {
        return new CommandToBlock(this.duration, this.animation);
    }
    public override IEnumerator Execute()
    {
        Fighter fighter = null;
        for (int i = 0; i < targets.Length; i++)
        {
            fighter = targets[i].GetComponent<Fighter>();
        }
        BlockAll block = new BlockAll(3, null);
        block.targets = this.targets;
        block.originator = fighter.gameObject;
        fighter.SetAction(block);
        yield return true;
    }
    public override int GetCost()
    {
        return 10;
    }
}
public class DoubleAttack : FighterAction
{
    public int damage;
    public DoubleAttack(int _duration, int _damage, Animation _animation)
    {
        this.name = "Attack";
        this.description = $"Attack twice for {_damage}";
        this.duration = _duration;
        this.damage = _damage;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Foe" };
    }
    public override FighterAction Clone()
    {
        return new DoubleAttack(this.duration, this.damage, this.animation);
    }
    public override IEnumerator Execute()
    {

        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].GetComponent<Fighter>().AddToHealth(this.damage * -1, this.originator.GetComponent<Fighter>());
            targets[i].GetComponent<Fighter>().AddToHealth(this.damage * -1, this.originator.GetComponent<Fighter>());
        }
        yield return true;
    }
    public override void ReevaluateActionValues(Fighter self)
    {
        this.damage = Mathf.FloorToInt(self.damage * self.damageMultiplier);
    }
}
public class LifeSteal : FighterAction
{
    public int damage;
    public LifeSteal(int _duration, int _damage, Animation _animation)
    {
        this.name = "Life Steal";
        this.description = $"Attack and heal for _damage";
        this.duration = _duration;
        this.damage = _damage;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Foe" };
    }
    public override FighterAction Clone()
    {
        return new LifeSteal(this.duration, this.damage, this.animation);
    }
    public override IEnumerator Execute()
    {

        for (int i = 0; i < targets.Length; i++)
        {
            Fighter self = this.originator.GetComponent<Fighter>();
            targets[i].GetComponent<Fighter>().AddToHealth(this.damage * -1, self);
            self.AddToHealth(this.damage, self);
        }
        yield return true;
    }
    public override void ReevaluateActionValues(Fighter self)
    {
        this.damage = Mathf.FloorToInt(self.damage * self.damageMultiplier);
    }
}
public class Skip : FighterAction
{
    public Skip(int _duration, Animation _animation)
    {
        this.name = "Skip";
        this.description = "Skip your turn";
        this.duration = _duration;
        this.animation = _animation;
        this.targets = null;
        this.targetCount = 1;
        this.validTargets = new string[] {  };
    }
    public override FighterAction Clone()
    {
        return new Skip(this.duration, this.animation);
    }
    public override IEnumerator Execute()
    {
        Debug.Log("Skipped");
        yield return true;
    }
}
public class StunAttack : FighterAction
{
    public int damage;
    public StunAttack(int _duration, int _damage, int _effectDuration, Animation _animation)
    {
        this.name = "Stun Attack";
        this.description = "Stun enemy and attack them for {}";
        this.duration = _duration;
        this.animation = _animation;
        this.damage = _damage;
        this.effectDuration = _effectDuration;
        this.targetCount = 1;
        this.validTargets = new string[] { "Foe" };
    }
    public override FighterAction Clone()
    {
        return new StunAttack(this.duration, this.damage, this.effectDuration, this.animation);
    }
    public override IEnumerator Execute()
    {
        Effect stun = new Stun(effectDuration);
        for (int i = 0; i < targets.Length; i++)
        {
            Fighter targ = targets[i].GetComponent<Fighter>();
            targ.AddToHealth(this.damage * -1, this.originator.GetComponent<Fighter>());
            targ.AddEffect(targ, stun);
        }
        yield return true;
    }
}
public class Heal : FighterAction
{
    public int healValue;
    public string prefabPath = "Prefabs/Effects/Heal";
    public Heal(int _duration, int _healValue, Animation _animation)
    {
        this.name = "Heal";
        this.description = $"Heal target for {_healValue}";
        this.duration = _duration;
        this.healValue = _healValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Friend", "Self", "Foe"};
    }
    public override FighterAction Clone()
    {
        return new Heal(this.duration, this.healValue, this.animation);
    }
    public override IEnumerator Execute()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i].GetComponent<Fighter>().AddToHealth(this.healValue, this.originator.GetComponent<Fighter>());
            Vector3 pos = new Vector3(targets[i].transform.position.x, targets[i].transform.position.y - .05f, targets[i].transform.position.z);
            if (!this.originator.name.Contains("Shadow"))
            {
                GetVisualEffectMaster().InstantiateVisualSprite(Resources.Load(prefabPath), pos, targets[i].transform.rotation, targets[i].transform, this.duration);
            }
        }
        yield return true;
    }
    public override int GetCost()
    {
        return 7;
    }
}
public class Mark : FighterAction
{
    public GameObject attackTarget;
    public int cost;
    public Mark(int _duration, Animation _animation)
    {
        this.name = "Mark";
        this.description = $"Mark a target for Joey to attack";
        this.duration = _duration;
        this.animation = _animation;
        this.targetCount = 1;
        this.cost = 10;
        this.levelCap = 2;
        this.validTargets = new string[] { "Foe", "Friend" };
    }
    public override FighterAction Clone()
    {
        return new Mark(this.duration, this.animation);
    }
    public override IEnumerator Execute()
    {
        Ronny ronny = null;
        GameObject joey = null;
        if (!this.originator.name.Contains("Shadow"))
        {
            ronny = this.originator.GetComponent<Ronny>();
            joey = ronny.battleMasterScript.GetAllyObject();
        } else
        {
            ronny = this.originator.transform.GetComponentInParent<Ronny>();
            joey = ronny.battleMasterScript.GetShadow(ronny.battleMasterScript.GetAllyObject());
        }
        Fighter fighter = joey.GetComponent<Fighter>();
        Attack attack = new Attack(3, Mathf.FloorToInt(fighter.damage * fighter.damageMultiplier), null);
        attack.originator = joey;
        attack.targets = this.targets;
        fighter.SetAction(attack);
        yield return true;
    }
    public override int GetCost()
    {
        return cost;
    }
    public override void LevelUpdate()
    {
        switch (skillLevel)
        {
            case 1:
                name = "Mark";
                targetCount = 1;
                cost = 10;
                this.description = $"Mark a target for Joey to attack";
                this.targets = null;
                break;
            case 2:
                name = "Mark+";
                targetCount = 0;
                Ronny ronny = this.originator.name.Contains("Shadow") ? ronny = this.originator.GetComponentInParent<Ronny>() : ronny = this.originator.GetComponent<Ronny>();
                cost = 10 + 5 * (this.GetAOETargets(ronny.battleMasterScript.allFighters).Length - 1);
                this.description = $"Mark all targets for Joey to attack";
                break;
        }
    }
}
public class PoisonAttack : FighterAction
{
    public int poisonDamage;
    public PoisonAttack (int _duration, int _effectDuration ,int _poisonDamage, Animation _animation)
    {
        this.name = "Poison Attack";
        this.description = $"Deal {_poisonDamage} each turn for {_effectDuration} turns";
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.poisonDamage = _poisonDamage;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Foe" };
    }
    public override FighterAction Clone()
    {
        return new PoisonAttack(this.duration, this.effectDuration, this.poisonDamage, this.animation);
    }
    public override IEnumerator Execute()
    {
        Effect poison = new Poison(this.effectDuration, this.poisonDamage, this.originator);
        for (int i = 0; i < targets.Length; i++)
        {
            Fighter fighter = targets[i].GetComponent<Fighter>();
            fighter.AddEffect(fighter, poison);
        }
        yield return true;
    }
}
public class Taunt : FighterAction
{
    public GameObject attackTarget;
    public int cost;
    public Taunt(int _duration, Animation _animation)
    {
        this.name = "Taunt";
        this.description = $"Causes target to attack you";
        this.duration = _duration;
        this.animation = _animation;
        this.targetCount = 1;
        this.cost = 10;
        this.levelCap = 2;
        this.validTargets = new string[] { "Foe", "Friend" };
    }
    public override FighterAction Clone()
    {
        return new Taunt(this.duration, this.animation);
    }
    public override IEnumerator Execute()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            Fighter fighter = targets[i].GetComponent<Fighter>();
            Attack attack = new Attack(3, Mathf.FloorToInt(fighter.damage * fighter.damageMultiplier), null);
            attack.originator = targets[i];
            attack.targets = new GameObject[] { originator };
            fighter.SetAction(attack);
        }

        yield return true;
    }
    public override int GetCost()
    {
        return cost;
    }
    public override void LevelUpdate()
    {
        switch (skillLevel)
        {
            case 1:
                name = "Taunt";
                targetCount = 1;
                cost = 10;
                this.description = $"Causes a foe to attack you";
                this.targets = null;
                break;
            case 2:
                name = "Taunt+";
                targetCount = 0;
                Ronny ronny = this.originator.name.Contains("Shadow") ? ronny = this.originator.GetComponentInParent<Ronny>() : ronny = this.originator.GetComponent<Ronny>();
                cost = 10 + 5 * (this.GetAOETargets(ronny.battleMasterScript.allFighters).Length - 1);
                this.description = $"Causes all foes to attack you";
                break;
        }
    }
}
public class TauntAll :FighterAction
{
    public GameObject attackTarget;
    public TauntAll(int _duration, Animation _animation)
    {
        this.name = "Taunt_All";
        this.description = $"Causes all enemies to attack you";
        this.duration = _duration;
        this.animation = _animation;
        this.targetCount = 0;
        this.validTargets = new string[] { "Foe" };
    }
    public override FighterAction Clone()
    {
        return new TauntAll(this.duration, this.animation);
    }
    public override IEnumerator Execute()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            Fighter fighter = targets[i].GetComponent<Fighter>();
            Attack attack = new Attack(3, Mathf.FloorToInt(fighter.damage * fighter.damageMultiplier), null);
            attack.originator = targets[i];
            attack.targets = new GameObject[] { originator };
            fighter.SetAction(attack);
        }
        
        yield return true;
    }
    public override int GetCost()
    {
        return 10;
    }
}
public class VulnerableAttack : FighterAction
{
    public int vulnerableValue, baseValue, baseDuration, cost;
    public VulnerableAttack(int _duration, int _effectDuration , int _vulnerableValue, Animation _animation)
    {
        this.name = "Vulnerable Attack";
        this.description = $"Lowers targets defense by {_vulnerableValue} for {_effectDuration} turns";
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.baseDuration = _effectDuration;
        this.vulnerableValue = _vulnerableValue;
        this.baseValue = _vulnerableValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.levelCap = 3;
        this.validTargets = new string[] { "Foe", "Friend" };
    }
    public override FighterAction Clone()
    {
        return new VulnerableAttack(this.duration, this.effectDuration, this.vulnerableValue, this.animation);
    }
    public override IEnumerator Execute()
    {
        Effect vulnerable = new Vulnerable(this.effectDuration, this.vulnerableValue);
        for (int i = 0; i < targets.Length; i++)
        {
            Fighter fighter = targets[i].GetComponent<Fighter>();
            fighter.AddEffect(fighter, vulnerable);
        }
        yield return true;
    }
    public override float GetEffectValue()
    {
        return this.vulnerableValue;
    }
    public override int GetCost()
    {
        return cost;
    }
    public override void LevelUpdate()
    {
        switch (skillLevel)
        {
            case 1:
                name = "Vulnerable Attack";
                cost = 6;
                vulnerableValue = baseValue;
                break;
            case 2:
                name = "Vulnerable Attack+";
                cost = 8;
                vulnerableValue = baseValue + 1;
                effectDuration = baseDuration;
                break;
            case 3:
                name = "Vulnerable Attack++";
                cost = 10;
                vulnerableValue = baseValue + 2;
                effectDuration = baseDuration + 1;
                break;
        }
        description = $"Lowers targets defense by {vulnerableValue} for {effectDuration} turns";
    }
}
public class WeakAttack : FighterAction
{
    public int weakValue, baseValue, baseDuration, cost;
    public WeakAttack (int _duration, int _effectDuration , int _weakValue, Animation _animation)
    {
        this.name = "Weak Attack";
        this.description = $"Minus targets attack by {_weakValue} for {_effectDuration} turns";
        this.cost = 6;
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.weakValue = _weakValue;
        this.baseValue = _weakValue;
        this.baseDuration = _effectDuration;
        this.animation = _animation;
        this.targetCount = 1;
        this.levelCap = 3;
        this.validTargets = new string[] { "Foe", "Friend" };
    }
    public override FighterAction Clone()
    {
        return new WeakAttack(this.duration, this.effectDuration, this.weakValue, this.animation);
    }
    public override IEnumerator Execute()
    {
        Effect weak = new Weak(this.effectDuration, this.weakValue);
        for (int i = 0; i < targets.Length; i++)
        {
            Fighter fighter = targets[i].GetComponent<Fighter>();
            fighter.AddEffect(fighter, weak);
        }
        yield return true;
    }
    public override int GetCost()
    {
        return cost;
    }
    public override float GetEffectValue()
    {
        return this.weakValue;
    }
    public override void LevelUpdate()
    {
        switch(skillLevel)
        {
            case 1:
                name = "Weak Attack";
                cost = 6;
                weakValue = baseValue;
                effectDuration = baseDuration;
                break;
            case 2:
                name = "Weak Attack+";
                cost = 8;
                weakValue = baseValue + 1;
                effectDuration = baseDuration;
                break;
            case 3:
                name = "Weak Attack++";
                cost = 10;
                weakValue = baseValue + 2;
                effectDuration = baseDuration + 1;
                break;
        }
        this.description = $"Minus targets attack by {weakValue} for {effectDuration} turns";
    }
}