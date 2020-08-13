using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public abstract class Action
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
public class Attack : Action
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
            Mathf.RoundToInt(targets[i].GetComponent<Fighter>().AddToHealth(this.damage * -1));
        }
        yield return true;
    }
    public override float GetValue()
    {
        return this.damage;
    }
}
public class BolsterDefense : Action
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
            targets[i].GetComponent<Fighter>().AddEffect(bolster);
        }
        yield return true;
    }
    public override float GetValue()
    {
        return this.buffValue;
    }
}
public class BuffAttack : Action
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
            targets[i].GetComponent<Fighter>().AddEffect(strengthen);
        }
        yield return true;
    }
    public override float GetValue()
    {
        return this.buffValue;
    }
}
public class Heal : Action
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
            Mathf.RoundToInt(targets[i].GetComponent<Fighter>().AddToHealth(this.healValue));
        }
        yield return null;
    }
    public override float GetValue()
    {
        return this.healValue;
    }
}
public class PoisonAttack : Action
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
            targets[i].GetComponent<Fighter>().AddEffect(poison);
        }
        yield return true;
    }
    public override float GetValue()
    {
        return this.poisonDamage;
    }
}
public class VulnerableAttack : Action
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
            targets[i].GetComponent<Fighter>().AddEffect(vulnerable);
        }
        yield return null;
    }
    public override float GetValue()
    {
        return this.vulnerableValue;
    }
}
public class WeakAttack : Action
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
            targets[i].GetComponent<Fighter>().AddEffect(weak);
        }
        yield return null;
    }
    public override float GetValue()
    {
        return this.weakValue;
    }
}
public class CommandToAttack : Action
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

