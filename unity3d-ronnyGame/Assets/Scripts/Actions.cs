using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public abstract class FighterAction
{
    public GameObject originator { get; set; }
    public GameObject[] targets { get; set; }
    public int duration, effectDuration;
    public int virtueValue { get; set; }
    public int targetCount;
    public Animation animation;
    public string name { get; set; }
    public string[] validTargets;
    protected string IMAGEPATH;
    public bool IsActionAOE()
    {
        if (targetCount != 1)
        {
            return true;
        } else
        {
            return false;
        }
    } 
    public GameObject[] GetAOETargets(ListBeingData list)
    {
        List<GameObject> targets = new List<GameObject>();
        Fighter self = originator.GetComponent<Fighter>();
        if (targetCount == 0) // get all enemies
        {
            for (int i = 0; i < list.BeingDatas.Count; i++)
            {
                if (self.TargetRelationToSelf(list.BeingDatas[i].gameObject.tag) == "Foe")
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
        }
        return targets.ToArray();
    }
    public abstract IEnumerator Execute();
    public abstract float GetValue();
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
    public string GetImagePath()
    {
        return this.IMAGEPATH;
    }
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
    public float damage;
    public Attack (int _duration, float _damage, Animation _animation)
    {
        this.name = "Attack";
        this.duration = _duration;
        this.damage = _damage;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Foe" };
        this.virtueValue = Mathf.RoundToInt(this.GetValue());
        this.IMAGEPATH = "UI/UI_attack";
    }
    public override IEnumerator Execute()
    {

        for (int i = 0; i < targets.Length; i++)
        {
            Mathf.RoundToInt(targets[i].GetComponent<Fighter>().AddToHealth(this.damage * -1, this.originator.GetComponent<Fighter>()));
        }
        yield return true;
    }
    public override float GetValue()
    {
        return this.damage;
    }
}
public class ApplyThorns : FighterAction
{
    public float percentValue;
    public ApplyThorns(int _duration, int _effectDuration, float _percentValue, Animation _animation)
    {
        this.name = "Thorns";
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.percentValue = _percentValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Friend" };
        this.virtueValue = Mathf.RoundToInt(_effectDuration + (_effectDuration * _percentValue));
        this.IMAGEPATH = "UI/UI_buff";
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
    public override float GetValue()
    {
        return this.percentValue;
    }
}
public class BolsterDefense : FighterAction
{
    public int buffValue;
    public BolsterDefense(int _duration, int _effectDuration ,int _buffValue, Animation _animation)
    {
        this.name = "Bolster Defense";
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.buffValue = _buffValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Friend" };
        this.virtueValue = _effectDuration * _buffValue;
        this.IMAGEPATH = "UI/UI_buff";
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
    public override float GetValue()
    {
        return this.buffValue;
    }
}
public class BuffAttack : FighterAction
{
    public int buffValue;
    public BuffAttack(int _duration, int _effectDuration ,int _buffValue, Animation _animation)
    {
        this.name = "Buff Attack";
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.buffValue = _buffValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Friend" };
        this.virtueValue = _effectDuration * _buffValue;
        this.IMAGEPATH = "UI/UI_buff";
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
    public override float GetValue()
    {
        return this.buffValue;
    }
}
public class CommandToAttack : FighterAction
{
    public GameObject attackTarget;
    public CommandToAttack(int _duration, Animation _animation)
    {
        this.name = "Command to Attack";
        this.duration = _duration;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Friend", "Foe" };
        this.IMAGEPATH = "UI/UI_Attack";
    }
    public override IEnumerator Execute()
    {
        Ronny ronny = this.originator.GetComponent<Ronny>();
        Fighter fighter = null;
        GameObject targ = null;
        for (int i = 0; i < targets.Length; i++)
        {
            fighter = targets[i].GetComponent<Fighter>();
            targ = targets[i];
        }
        Attack attack = new Attack(3, fighter.damage * fighter.damageMultiplier, null);
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
    }
    public override float GetValue()
    {
        return 0;
    }
}
public class DoubleAttack : FighterAction
{
    public float damage;
    public DoubleAttack(int _duration, float _damage, Animation _animation)
    {
        this.name = "Attack";
        this.duration = _duration;
        this.damage = _damage / 2;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Foe" };
        this.virtueValue = Mathf.RoundToInt(this.GetValue());
        this.IMAGEPATH = "UI/UI_attack";
    }
    public override IEnumerator Execute()
    {

        for (int i = 0; i < targets.Length; i++)
        {
            Mathf.RoundToInt(targets[i].GetComponent<Fighter>().AddToHealth(this.damage * -1, this.originator.GetComponent<Fighter>()));
        }
        yield return true;
    }
    public override float GetValue()
    {
        return this.damage * 2;
    }
}
public class Heal : FighterAction
{
    public int healValue;
    public Heal(int _duration, int _healValue, Animation _animation)
    {
        this.name = "Heal";
        this.duration = _duration;
        this.healValue = _healValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Friend" };
        this.virtueValue = _healValue;
        this.IMAGEPATH = "UI/UI_buff";
    }
    public override IEnumerator Execute()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            Mathf.RoundToInt(targets[i].GetComponent<Fighter>().AddToHealth(this.healValue, this.originator.GetComponent<Fighter>()));
        }
        yield return true;
    }
    public override float GetValue()
    {
        return this.healValue;
    }
}
public class PoisonAttack : FighterAction
{
    public int poisonDamage;
    public PoisonAttack (int _duration, int _effectDuration ,int _poisonDamage, Animation _animation)
    {
        this.name = "Poison Attack";
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.poisonDamage = _poisonDamage;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Foe" };
        this.virtueValue = _effectDuration * _poisonDamage;
        this.IMAGEPATH = "UI/UI_attack";
        
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
    public override float GetValue()
    {
        return this.poisonDamage;
    }
}

public class VulnerableAttack : FighterAction
{
    public int vulnerableValue;
    public VulnerableAttack(int _duration, int _effectDuration ,int _vulnerableValue, Animation _animation)
    {
        this.name = "Vulnerable Attack";
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.vulnerableValue = _vulnerableValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Foe" };
        this.IMAGEPATH = "UI/UI_debuff";
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
    public override float GetValue()
    {
        return this.vulnerableValue;
    }
}
public class WeakAttack : FighterAction
{
    public int weakValue;
    public WeakAttack (int _duration, int _effectDuration ,int _weakValue, Animation _animation)
    {
        this.name = "Weak Attack";
        this.duration = _duration;
        this.effectDuration = _effectDuration;
        this.weakValue = _weakValue;
        this.animation = _animation;
        this.targetCount = 1;
        this.validTargets = new string[] { "Foe" };
        this.IMAGEPATH = "UI/UI_debuff";
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
    public override float GetValue()
    {
        return this.weakValue;
    }
}


