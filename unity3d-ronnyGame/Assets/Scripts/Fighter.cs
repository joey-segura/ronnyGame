using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Effect
{
    public string name;
    public bool isActive;
    public float value1;
    public float value2;
    public int duration;
}
public class Fighter : Being
{
    public float health, damage, damageMultiplier = 1, defenseMultiplier = 1;
    public bool isStunned = false, isPoisoned = false;
    public string[] party;
    private List<Effect> currentEffects = new List<Effect>();
    public virtual void Action()
    {
        return;
    }

    public void AddEffect(Effect effect)
    {
        if (effect.name != "Stunned")
        {
            currentEffects.Add(effect);
        } else
        {
            for(int i = 0; i < currentEffects.Count; i++)
            {
                if (currentEffects[i].name == "Stunned")
                {
                    currentEffects[i].duration += effect.duration;
                }
            }
        }
    }
    public void AddToHealth(float change)
    {
        this.health += change;
    }
    private void ApplyEffects()
    {
        foreach(Effect effect in currentEffects)
        {
            switch (effect.name)
            {
                case "Stunned":
                    isStunned = true;
                    //action is skipped this turn
                    break;
                case "Poisoned":
                    isPoisoned = true;
                    //take poison damage (takedamage(effect.value1));
                    break;
                case "Vulnerable":
                    defenseMultiplier = effect.value1;
                    break;
                case "Bolstered":
                    defenseMultiplier = effect.value1;
                    break;
                case "Empowered":
                    damageMultiplier = effect.value1;
                    break;
                case "Weakened":
                    damageMultiplier = effect.value1;
                    break;
                default:
                    return;
            }
            effect.duration -= 1;
        }
    }
    public void BeginTurn(ListBeingData partyMembers, ListBeingData enemyMembers)
    {
        this.ApplyEffects();
        //apply status effects
    }
    public virtual Action ChooseAction(ListBeingData partyMembers, ListBeingData enemyMembers)
    {
        Action action = new Action();
        action.originator = this.gameObject;
        action.target = this.ChooseTarget(partyMembers, enemyMembers);
        action.method = this.Action;

        return action;
    }
    public virtual GameObject ChooseTarget(ListBeingData partyMembers, ListBeingData enemyMembers) //chooses a target at random!
    {
        int targetIndex = Random.Range(0, partyMembers.BeingDatas.Count + enemyMembers.BeingDatas.Count);
        if (targetIndex > partyMembers.BeingDatas.Count)
        {
            targetIndex -= partyMembers.BeingDatas.Count;
            return enemyMembers.BeingDatas[targetIndex].gameObject;
        } else
        {
            return partyMembers.BeingDatas[targetIndex].gameObject;
        }
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
    public void RemoveEffect(Effect effect)//used to call and remove
    {
        for (int i = 0; i < currentEffects.Count; i++)
        {
            if (currentEffects[i] == effect)
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
        return party;
    }
} 
