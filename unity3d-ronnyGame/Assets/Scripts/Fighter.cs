using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class Fighter : Being
{
    public float health, damage, damageMultiplier = 1, defenseMultiplier = 1;
    public bool isStunned = false, isPoisoned = false;
    protected bool isBattle = false;

    public string[] party;
    public FighterAction currentAction = null;
    public Dictionary<int, Effect> currentEffects = new Dictionary<int, Effect>();
    //private List<Effect> currentEffects = new List<Effect>();
    protected List<FighterAction> actionList = new List<FighterAction>();

    public Vector3 battlePosition;
    public Dictionary<int, Func<float, Fighter, float>> onHitEffects = new Dictionary<int, Func<float, Fighter, float>>();

    public void AddEffect(Fighter fighter, Effect effect)
    {
        effect.Affliction(fighter);
        currentEffects.Add(effect.GetKey(), effect);
        currentEffects.OrderByDescending(x => effect.duration);
    }
    public virtual float AddToHealth(float change, Fighter causer)
    {
        foreach(Func<float, Fighter, float> a in onHitEffects.Values)
        {
            change = a.Invoke(change, causer);
        }
        Debug.Log($"{this.name} has {onHitEffects.Count} onhit functions");
        change = change / this.defenseMultiplier;
        Debug.Log($"{this.name}'s health just got changed by {change}");
        if (causer.gameObject.name == "Joey" || causer.gameObject.name == "Ritter") // ugly but sensical solution, every fighther on health change should check if the causer was a party member (this accounts for all buff values etc)
        {
            battleMasterScript.AddToVirtue(change);
        }
        if (change < 0)
        {
            StartCoroutine(GetHitJiggle());
        }
        this.health += change;
        this.DeathCheck();
        return change;
    }
    public bool AddToOnHitEffects(int key, Func<float, Fighter, float> method)
    {
        if (onHitEffects.ContainsKey(key))
        {
            return false;
        } else
        {
            onHitEffects.Add(key, method);
            return true;
        }
    }
    
    public IEnumerator BattleActionMove(FighterAction action)
    {
        float distance = 0;
        Vector3 newPos;
        if (this.transform.position.x > 0)
        {
            distance = -.25f;
        }
        else
        {
            distance = .25f;
        }
        if (action.IsActionAOE())
        {
            newPos = new Vector3(this.transform.position.x + distance, this.transform.position.y, this.transform.position.z);
        } else
        {
            newPos = Vector3.Lerp(this.transform.position, action.targets[0].transform.position, .75f);
        }
        

        while (this.transform.position != newPos)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, newPos, .1f);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(action.duration);
        while (this.transform.position != battlePosition)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, this.battlePosition, .1f);
            yield return new WaitForEndOfFrame();
        } //not moving back slow enough
    }
    public FighterAction GetActionByName(string name)
    {
        FighterAction action = null;
        foreach (FighterAction act in actionList)
        {
            if (act.name == name)
            {
                action = act;
            }
        }
        return action;
    }
    public void TickEffects() // happens every fighters turn
    {
        for (int i = 0; i < currentEffects.Count; i++)
        {

            KeyValuePair<int, Effect> effect = currentEffects.ElementAt(i);
            effect.Value.OnTick(this);
            effect.Value.duration -= 1;
            if (effect.Value.duration <= 0)
            {
                effect.Value.Cleanse(this);
                RemoveEffect(effect.Key);
            }
        }
    }
    public virtual FighterAction TurnAction(ListBeingData allFighters)
    {
        this.RecalculateActions();
        FighterAction action = null;
        string relation = null;
        bool valid = false;
        while (!valid)
        {
            int actionIndex = UnityEngine.Random.Range(0, this.actionList.Count);
            action = this.actionList[actionIndex]; //random action for enemy and friendly units
            action.originator = this.gameObject;
            if (action.IsActionAOE())
            {
                action.targets = action.GetAOETargets(allFighters);
                this.currentAction = action;
                valid = true;
            } else
            {
                GameObject target = null;
                bool validTarget = false;
                int attempts = 0;
                while (!validTarget)
                {
                    target = this.ChooseTarget(allFighters);
                    relation = this.TargetRelationToSelf(target);
                    if (action.IsValidAction(relation))
                    {
                        validTarget = true;
                    }
                    if (attempts >= 10) //If no target could be found with valid actions (E.G., a buffer type unit who only has buff friendlies has no more teammates left)
                    {
                        return null;
                    }
                    else
                    {
                        attempts++;
                    }
                }
                action.targets = new GameObject[] { target };
                this.currentAction = action;
                valid = true;
            }
        }
        return action;
    }
    public virtual GameObject ChooseTarget(ListBeingData allFighters) //chooses a target at random!
    {
        int targetIndex = UnityEngine.Random.Range(0, allFighters.BeingDatas.Count);
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
    public virtual void DeathCheck()
    {
        if (this.health <= 0)
        {
            this.DeathTrigger(false);
            BattleMaster battleMaster = this.transform.parent.GetComponentInParent<BattleMaster>();
            battleMaster.RemoveMemberByID(this.ID);
            battleMaster.BattleEndCheck();
            this.DestroyBeing();
            return;
        }
        else
        {
            return;
        }
    }
    public virtual void DeathTrigger(bool shadow)
    {
        //this is here so that it can be instantiated through general terms
    }
    public IEnumerator FlashWhite(float seconds)
    {
        Shader whiteGUI = Shader.Find("GUI/Text Shader");
        SpriteRenderer self = this.GetComponent<SpriteRenderer>();
        Shader defaultShad = self.material.shader;
        self.material.shader = whiteGUI;
        yield return new WaitForSeconds(seconds);
        self.material.shader = defaultShad;
    }
    public List<FighterAction> GetActions()
    {
        return this.actionList;
    }
    public IEnumerator GetHitJiggle()
    {
        Vector3 pos = this.transform.position;
        float distance = .2f;
        StartCoroutine(FlashWhite(.4f));
        this.transform.position = Vector3.MoveTowards(pos, new Vector3(pos.x + distance, pos.y, pos.z), 1);
        yield return new WaitForSeconds(.05f);
        this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(pos.x - 2 * distance, pos.y, pos.z), 1);
        yield return new WaitForSeconds(.05f);
        this.transform.position = Vector3.MoveTowards(this.transform.position, pos, 1);
    }
    public FighterAction GetIntention(ListBeingData allFighters)
    {
        if (this.isStunned)
        {
            this.TickEffects();
            return null;
        }

        FighterAction action = this.TurnAction(allFighters);
        return action;
    }
    public string[] GetParty()
    {
        if (party != null)
        {
            return party;
        }
        else
        {
            string[] party = { this.gameObject.name };
            return party;
        }
    }
    private FighterShadow GetShadow()
    {
        return this.transform.GetComponentInChildren<FighterShadow>();
    }
    public virtual void InitializeBattle()
    {
        this.isBattle = true;
        this.battlePosition = this.transform.position;
        return; // this is for any enemy script to disable their movement script once the battle starts
    }
    private void OnGUI()
    {
        if (isBattle && currentAction != null && this.isHovering)
        {
            Vector3 mouse = Input.mousePosition;
            Rect rect;
            if (mouse.x > (Screen.width / 2))
            {
                rect = new Rect(mouse.x - 200, (Screen.height - mouse.y + 10), 200, 100);
            }
            else
            {
                rect = new Rect(mouse.x + 10, (Screen.height - mouse.y + 10), 200, 100);
            }

            string names = null;
            if (currentAction.targets != null)
            {
                for (int i = 0; i < currentAction.targetCount; i++)
                {
                    names += $"{currentAction.targets[i].name} ";
                }
                FighterShadow shadow = GetShadow();
                if (!shadow.playing)
                {
                    StartCoroutine(shadow.PlayAnimations());
                }
            }
            GUI.Box(rect, $"Character Name: {name}\n HP: {health}\n Action name: {currentAction.name}\n Targets name: {names}\n Value: {currentAction.GetValue()}");
        }
    }
    public virtual void RecalculateActions()
    {
        //This function should be instantiated by each individual fighter with their unique actions
        //Recalculating actions is important to apply certain affects
        //(E.G) if you apply a damage buff we need to apply that affect to the persons attack damage
        if (currentAction != null)
        {
            foreach (FighterAction action in this.actionList)
            {
                if (action.name == currentAction.name)
                {
                    action.originator = currentAction.originator;
                    action.targets = currentAction.targets;
                    currentAction = action;
                }
            }
        }
        return;
    }
    public void RemoveAllEffects()//strong cleanse (used after a battle is concluded?)
    {
        this.currentEffects = new Dictionary<int, Effect>();
    }
    public void RemoveAllEffectsOfName(string name)//might be used by actions to cleanse debuffs etc
    {
        for (int i = 0; i < currentEffects.Count; i++)
        {
            if (currentEffects[i].name == name)
            {
                currentEffects.Remove(currentEffects[i].GetKey());
            }
        }
    }
    public void SetAction(FighterAction action)
    {
        this.currentAction = action;
    }
    public bool RemoveEffect(int key)
    {
        if (currentEffects.Remove(key))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool RemoveOnHitEffect(int key)
    {
        if (onHitEffects.Remove(key))
        {
            return true;
        } else
        {
            return false;
        }
    }
    public string TargetRelationToSelf(GameObject target)
    {
        string targetTag = target.tag;
        if (this.gameObject == target)
        {
            return "Self";
        } else if (this.gameObject.tag == "Party" && targetTag == "Player")
        {
            return "Friend";
        } else if (this.gameObject.tag == "Player" && targetTag == "Party")
        {
            return "Friend";
        }
        else if (targetTag == this.gameObject.tag)
        {
            return "Friend";
        } else
        {
            return "Foe";
        }
    }
} 