﻿using System;
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
    protected FighterAction currentAction = null;
    private Dictionary<int, Effect> currentEffects = new Dictionary<int, Effect>();
    //private List<Effect> currentEffects = new List<Effect>();
    protected List<FighterAction> actionList = new List<FighterAction>();

    protected Vector3 battlePosition;
    private Dictionary<int, Func<float,Fighter, float>> onHitEffects = new Dictionary<int, Func<float, Fighter, float>>();

    public virtual void Awake()
    {
        Invoke("InititializeBaseActions", 1);
    }
    private void InititializeBaseActions()
    {
        this.actionList.Add(new Attack(3, this.damage * this.damageMultiplier, null));
        this.actionList.Add(new WeakAttack(3, 3, 2, null));
        this.actionList.Add(new BuffAttack(3, 3, 2, null));
        this.actionList.Add(new BolsterDefense(3, 3, 2, null));
        this.actionList.Add(new VulnerableAttack(3, 3, 2, null));
    }
    public void AddEffect(Fighter fighter, Effect effect)
    {
        effect.Affliction(fighter);
        currentEffects.Add(effect.GetKey(), effect);
        currentEffects.OrderByDescending(x => effect.duration);
    }
    public float AddToHealth(float change, Fighter causer)
    {
        foreach(Func<float, Fighter, float> a in onHitEffects.Values)
        {
            change = a.Invoke(change, causer);
        }
        Debug.Log($"{this.name} has {onHitEffects.Count} onhit functions");
        change = change / this.defenseMultiplier;
        Debug.Log($"{this.name}'s health just got changed by {change}");
        this.health += change;
        this.DeathCheck();
        return change;
    }
    public bool AddToCurrentEffects(int key, Effect effect)
    {
        if (currentEffects.ContainsKey(key))
        {
            return false;
        }
        else
        {
            currentEffects.Add(key, effect);
            return true;
        }
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
    public void TickEffects() // happens every fighters turn
    {
        for (int i = 0; i < currentEffects.Count; i++)
        {
            var effect = currentEffects.ElementAt(i);
            effect.Value.OnTick(this);
            effect.Value.duration -= 1;
            if (effect.Value.duration <= 0)
            {
                effect.Value.Cleanse(this);
                RemoveEffect(effect.Key);
            }
        }
    }
    public IEnumerator BattleActionMove(FighterAction action)
    {
        float distance = 0;
        if (this.transform.position.x > 0)
        {
            distance = -.25f;
        }
        else
        {
            distance = .25f;
        }
        Vector3 newPos = new Vector3(this.transform.position.x + distance, this.transform.position.y, this.transform.position.z);

        while (this.transform.position != newPos)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, newPos, .01f);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(action.duration);
        while (this.transform.position != battlePosition)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, this.battlePosition, .01f);
            yield return new WaitForEndOfFrame();
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
                    relation = this.TargetRelationToSelf(target.tag);
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
    public void DeathCheck()
    {
        if (this.health <= 0)
        {
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
    public IEnumerator DrawIntentions(FighterAction action)
    {
        yield return new WaitForEndOfFrame(); //waiting a frame to make sure data is settled before we do this call (Not a fan)
        Debug.Log($"{action.originator.name} is doing the action {action.name} to {action.targets[0].name}");
        if (!this.canvas.gameObject.activeInHierarchy) this.ToggleCanvas();
        Image intention = null;
        Image direction = null;
        GameObject panel = canvas.transform.GetChild(0).gameObject;

        for (int i = 0; i < panel.transform.childCount; i++)
        {
            switch (panel.transform.GetChild(i).name)
            {
                case "Intention":
                    intention = panel.transform.GetChild(i).GetComponent<Image>();
                    break;
                case "Direction":
                    direction = panel.transform.GetChild(i).GetComponent<Image>();
                    break;
                default:
                    break;
            }
        }
        direction.transform.position = action.originator.transform.position;
        direction.transform.rotation = Quaternion.Euler(90, 0, 0);
        Vector3 self = direction.rectTransform.position;
        Vector3 target = new Vector3(action.targets[0].transform.position.x, direction.rectTransform.position.y, action.targets[0].transform.position.z);

        float angle = Vector3.SignedAngle(target - self, transform.forward, Vector3.up);
        
        direction.transform.rotation = Quaternion.Euler(new Vector3(direction.transform.rotation.eulerAngles.x, direction.transform.rotation.eulerAngles.y, angle));
        direction.transform.position = Vector3.MoveTowards(self, target, Vector3.Distance(self, target) / 2);
        direction.transform.position += new Vector3(0, -.35f, 0);
        direction.rectTransform.sizeDelta = new Vector2(direction.rectTransform.sizeDelta.x, Vector3.Distance(target, self));
        /*Color tempColor = direction.color; //Might be necessary?
        tempColor.a = .1f;
        direction.color = tempColor;*/

        Texture2D image = Resources.Load(action.GetImagePath()) as Texture2D;
        Sprite sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0, 0));
        sprite.name = action.GetImagePath();
        intention.sprite = sprite;

        yield return null;
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
            Rect rect = new Rect(mouse.x + 10, (Screen.height - mouse.y + 10), 200, 100);
            string names = null;
            for (int i = 0; i < currentAction.targetCount; i++)
            {
                names += $"{currentAction.targets[i].name} ";
            }
            GUI.Box(rect, $"Action name: {currentAction.name}\n Targets name: {names}\n Value: {currentAction.GetValue()}");
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
        StartCoroutine("DrawIntentions", action);
    }
    public void SetHealth(float health)
    {
        this.health = health;
    }
    public List<FighterAction> GetActions()
    {
        return this.actionList;
    }
    public FighterAction GetCurrentAction()
    {
        return this.currentAction;
    }
    public FighterAction GetIntention(ListBeingData allFighters)
    {
        if (this.isStunned)
        {
            this.TickEffects();
            return null;
        }

        FighterAction action = this.TurnAction(allFighters);
        this.StartCoroutine("DrawIntentions", action);
        return action;
    }
    public Dictionary<int, Effect> GetCurrentEffects()
    {
        return currentEffects;
    }
    public Dictionary<int, Func<float, Fighter, float>> GetOnHitEffects()
    {
        return onHitEffects;
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
    public string TargetRelationToSelf(string targetTag)
    {
        if (this.gameObject.tag == "Party" && targetTag == "Player")
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
