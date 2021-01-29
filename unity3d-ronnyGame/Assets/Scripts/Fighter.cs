using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class Fighter : Being
{
    public float health, damageMultiplier = 1;
    public int damage, defense = 0;
    public bool isStunned = false, isPoisoned = false, playingWalkingSound = false;
    protected bool isBattle = false;

    private Shader defaultShader;

    public string[] party;
    public FighterAction currentAction = null;
    public Dictionary<int, Effect> currentEffects = new Dictionary<int, Effect>();
    //private List<Effect> currentEffects = new List<Effect>();
    protected List<FighterAction> actionList = new List<FighterAction>();

    public Vector3 battlePosition;
    public Dictionary<int, Func<int, Fighter, int>> onHitEffects = new Dictionary<int, Func<int, Fighter, int>>();
    public Dictionary<int, Func<int, Fighter, int>> onAttackEffects = new Dictionary<int, Func<int, Fighter, int>>();

    public void AddEffect(Fighter fighter, Effect effect)
    {
        effect.Affliction(fighter);
        currentEffects.Add(effect.GetKey(), effect);
        currentEffects.OrderByDescending(x => effect.duration);
    }
    public virtual void AddToHealth(int change, Fighter causer)
    {
        bool heal = Mathf.Sign(change) > 0 ? true : false;
        foreach(Func<int, Fighter, int> a in onHitEffects.Values)
        {
            change = a.Invoke(change, causer);
        }
        Debug.Log($"{this.name} has {onHitEffects.Count} onhit functions");
        if (!heal)
        {
            change = change + this.defense > 0 ? 0 : change + this.defense;
            foreach (Func<int, Fighter, int> a in causer.onAttackEffects.Values)
            {
                a.Invoke(change, causer);
            }
        }
        Debug.Log($"{this.name}'s health just got changed by {change}");
        if (change < 0)
        {
            StartCoroutine(GetHitJiggle());
        }
        this.health += change;
        this.DeathCheck(causer.gameObject);
    }
    public bool AddToOnAttackEffects(int key, Func<int, Fighter, int> method)
    {
        if (onAttackEffects.ContainsKey(key))
        {
            return false;
        }
        else
        {
            onAttackEffects.Add(key, method);
            return true;
        }
    }
    public bool AddToOnHitEffects(int key, Func<int, Fighter, int> method)
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
    public virtual void BattleStart() { }
    public virtual void DeathCheck(GameObject causer)
    {
        if (this.health <= 0)
        {
            
            this.DeathTrigger(false);
            BattleMaster battleMaster = this.transform.parent.GetComponentInParent<BattleMaster>();
            if (causer.tag == "Party")
            {
                battleMaster.AddToVirtue(Mathf.FloorToInt(Mathf.Abs(this.health)));
            }
            battleMaster.RemoveMemberByID(this.ID);
            battleMaster.BattleEndCheck();
            this.DestroyBeing();
            return;
        }
    }
    public IEnumerator MoveToBattleTarget(FighterAction action) {
        float distance = 0;
        Vector3 newPos;
        if (action.targets[0] != null)
        {
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
            }
            else if (action.targets[0] == this.gameObject)
            {
                newPos = battlePosition;
            }
            else
            {
                newPos = Vector3.Lerp(this.transform.position, action.targets[0].GetComponent<Fighter>().battlePosition, .75f);
            }
            while (this.gameObject != null && this.transform.position != newPos)
            {
                this.transform.position = Vector3.MoveTowards(this.transform.position, newPos, 15 * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }
        yield return true;
    }
    public IEnumerator MoveToBattlePosition()
    {
        while (this != null && this.transform.position != battlePosition )
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, this.battlePosition, 15 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        yield return true;
    }
    public FighterAction GetActionByName(string name)
    {
        FighterAction action = null;
        foreach (FighterAction act in actionList)
        {
            if (act.name.Contains(name))
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
   
    public virtual void DeathTrigger(bool shadow)
    {
        //this is here so that it can be instantiated through general terms
    }
    public IEnumerator FlashWhite(float seconds)
    {
        Shader whiteGUI = Shader.Find("GUI/Text Shader");
        SpriteRenderer self = this.GetComponent<SpriteRenderer>();
        self.material.shader = whiteGUI;
        yield return new WaitForSeconds(seconds);
        self.material.shader = defaultShader;
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
            FighterAction skp = new Skip(1, null);
            skp.originator = this.gameObject;
            Debug.Log("stunned!");
            return skp;
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
    public FighterShadow GetShadow()
    {
        return this.transform.GetComponentInChildren<FighterShadow>();
    }
    public bool HasEffect(string name)
    {
        foreach (Effect effect in currentEffects.Values)
        {
            if (effect.name.Contains(name))
            {
                return true;
            }
        }
        return false;
    }
    public virtual void InitializeBattle()
    {
        this.isBattle = true;
        this.battlePosition = this.transform.position;
        this.RemoveAllEffects();
        this.defaultShader = this.GetComponent<SpriteRenderer>().material.shader;
        return; // this is for any enemy script to disable their movement script once the battle starts
    }
    private new void OnGUI()
    {
        if (isBattle)
        {
            Rect rect;
            Vector3 fighterScreenPos = Camera.main.WorldToScreenPoint(battlePosition);
            float width = 150, height = 100;
            if (fighterScreenPos.x > (Screen.width / 2))
            {
                
                rect = new Rect(Screen.width - (Screen.width / 5), (Screen.height - fighterScreenPos.y) - height / 2, width, height);
            }
            else
            {
                rect = new Rect((Screen.width / 5) - width, (Screen.height - fighterScreenPos.y) - height / 2, width, height);
            }
            GUI.Box(rect, $"Name: {name}\n HP: {health}\n Damage: {damage}\n Defense: {defense}\n Action: {(this.currentAction != null ? this.currentAction.name : string.Empty)}");

            int effectIndex = 0;
            foreach (Effect effect in currentEffects.Values)
            {
                Rect effectRect = rect;
                effectRect.width = 25;
                effectRect.height = 25;
                effectRect.y -= 25;
                effectRect.x += 25 * effectIndex;
                effectIndex++;
                if (effect.duration == 1)
                {
                    if (battleMasterScript.isFlashing)
                        GUI.Box(effectRect, effect.name.Substring(0, 1));
                } else
                {
                    GUI.Box(effectRect, effect.name.Substring(0, 1));
                }
                //Debug.Log($"{effectRect.x} and {effectRect.y} and {Input.mousePosition} and {Screen.height - Input.mousePosition.y}");
                Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                if (effectRect.Contains(mousePos))
                {
                    Rect descriptionRect = rect;
                    descriptionRect.width *= 2;
                    descriptionRect.height /= 2;
                    descriptionRect.x = (Screen.width / 2) - (descriptionRect.width / 2);
                    descriptionRect.y = Screen.height / 2;
                    GUI.Box(descriptionRect, $"{effect.name}\n {effect.description}");
                }
            }
        }
        base.OnGUI();
    }
    public IEnumerator PlayWalkingSound (AudioClip sound)
    {
        if (!playingWalkingSound)
        {
            playingWalkingSound = true;
            soundMasterScript.PlaySound(sound, 0);
            yield return new WaitForSeconds(sound.length);
            playingWalkingSound = false;
        }
        yield return true;
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
        foreach (Effect effect in currentEffects.Values)
        {
            effect.Cleanse(this);
        }
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
    public bool RemoveOnAttackEffect(int key)
    {
        if (onAttackEffects.Remove(key))
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
            }
            else
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
} 