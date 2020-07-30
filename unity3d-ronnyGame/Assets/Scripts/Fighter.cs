using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Fighter : Being
{
    public float health, damage, damageMultiplier = 1, defenseMultiplier = 1;
    public bool isStunned = false, isPoisoned = false;
    public string[] party;
    private List<Effect> currentEffects = new List<Effect>();
    protected List<Action> actionList = new List<Action>();
    public virtual void Awake()
    {
        Invoke("InititializeBaseActions", 1);
    }
    private void InititializeBaseActions()
    {
        actionList.Add(new Attack(3, this.damage, null));
    }
    public void AddEffect(Effect effect)
    {
        currentEffects.Add(effect);
        currentEffects.Sort(CompareEffectsByDuration);
    }
    public void AddToHealth(float change)
    {
        this.health += change;
        Debug.LogError("DeathCheck");
    }
    public void ApplyEffects()
    {
        for(int i = 0; i < currentEffects.Count; i++)
        {
            currentEffects[i].Affliction(this);
            currentEffects[i].duration -= 1;
            if (currentEffects[i].duration <= 0)
            {
                currentEffects[i].Cleanse(this);
                currentEffects.Remove(currentEffects[i]);
            }
        }
    }
    public Action DoAITurn(ListBeingData allFighters)
    {
        this.ApplyEffects();
        this.RecalculateActions();
        GameObject target = this.ChooseTarget(allFighters);
        Action action = this.ChooseAction(target);
        //fighter.animation(action) <- animation handled in BattleMaster.cs
        return action;
    }
    public virtual Action ChooseAction(GameObject target)
    {
        Action action = this.actionList[0]; //random action for enemy and friendly units
        action.originator = this.gameObject;
        action.target = target;

        return action;
    }
    public virtual GameObject ChooseTarget(ListBeingData allFighters) //chooses a target at random!
    {
        int targetIndex = Random.Range(0, allFighters.BeingDatas.Count);
        return allFighters.BeingDatas[targetIndex].gameObject;
    }
    private static int CompareEffectsByDuration(Effect x, Effect y)
    {
        if (x.duration > y.duration)
        {
            return 1;
        } else if (x.duration < y.duration)
        {
            return -1;
        } else
        {
            return 0;
        }
    }
    public bool isDead()
    {
        if (this.health <= 0)
        {
            return true;
        } else
        {
            return false;
        }
    }
    public virtual void RecalculateActions()
    {
        //This function should be instantiated by each individual fighter with their unique actions
        //Recalculating actions is important to apply certain affects
        //(E.G) if you apply a damage buff we need to apply that affect to the persons attack damage
        return;
    }
    public void RemoveAllEffects()//strong cleanse (used after a battle is concluded?)
    {
        this.currentEffects = new List<Effect>();
    }
    public void RemoveAllEffectsOfName(string name)//might be used by actions to cleanse debuffs etc
    {
        for (int i = 0; i < currentEffects.Count; i++)
        {
            if (currentEffects[i].name == name)
            {
                currentEffects.Remove(currentEffects[i]);
            }
        }
    }
    public void SetHealth(float health)
    {
        this.health = health;
    }
    public string[] GetParty()
    {
        if (party != null)
        {
            return party;
        } else
        {
            string[] party = { this.gameObject.name };
            return party;
        }
        
    }
} 
