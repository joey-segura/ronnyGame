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
    protected List<Action> actionList = new List<Action>();
    public virtual void Awake()
    {
        Invoke("InititializeBaseActions", 2);
    }
    private void InititializeBaseActions()
    {
        actionList.Add(new Attack(3, this.damage));
    }
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
        Debug.Log(change);
        this.health += change;
        Debug.LogError("DeathCheck");
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
    public virtual Action ChooseAction(GameObject target)
    {
        Action action = this.actionList[0]; //random action for enemy and friendly units
        action.originator = this.gameObject;
        action.target = this.gameObject;

        return action;
    }
    public virtual GameObject ChooseTarget(ListBeingData allFighters) //chooses a target at random!
    {
        int targetIndex = Random.Range(0, allFighters.BeingDatas.Count);
        Debug.Log(allFighters.BeingDatas[targetIndex].gameObject.name);
        return allFighters.BeingDatas[targetIndex].gameObject;
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
