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
    protected FighterAction currentAction = null;
    private Dictionary<int, Effect> currentEffects = new Dictionary<int, Effect>();
    //private List<Effect> currentEffects = new List<Effect>();
    protected List<FighterAction> actionList = new List<FighterAction>();

    public GameObject[] direction;
    protected Vector3 battlePosition;
    private Dictionary<int, Func<float, Fighter, float>> onHitEffects = new Dictionary<int, Func<float, Fighter, float>>();

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
        if (causer.gameObject.tag == "Party") // ugly but sensical solution, every fighther on health change should check if the causer was a party member (this accounts for all buff values etc)
        {
            battleMasterScript.AddToVirtue(change);
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
    private static int CompareEffectsByDuration(Effect x, Effect y) // might need it later
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
        if (action.targets == null)
        {
            yield break;
        }
        string relation = TargetRelationToSelf(action.targets[0].gameObject);
        if (!action.IsValidAction(relation) && action.targetCount == 1 && this.canvas.gameObject.activeInHierarchy)
        {
            this.ToggleCanvas();
        }
        else if (!this.canvas.gameObject.activeInHierarchy && action.IsValidAction(relation))
        {
            this.ToggleCanvas();
        }
        GameObject panel = canvas.transform.GetChild(0).gameObject;
        Image intention = panel.transform.GetComponentInChildren<Image>();
        Texture2D image = Resources.Load(action.GetImagePath()) as Texture2D;
        Sprite sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0, 0));
        sprite.name = action.GetImagePath();
        intention.sprite = sprite;
        
        for (int i = 0; i < direction.Length; i++)
        {
            Destroy(direction[i]);
        }
        direction = new GameObject[action.targets.Length];
        for (int i = 0; i < action.targets.Length; i++)
        {
            string directionPath = "Prefabs/UI/Direction";
            VisualEffectMaster visual = action.GetVisualEffectMaster();
            direction[i] = visual.InstantiateVisualSprite(Resources.Load(directionPath), action.targets[i].transform.position, action.targets[i].transform.rotation, panel.transform);

            direction[i].transform.position = action.originator.transform.position;
            direction[i].transform.rotation = Quaternion.Euler(90, 0, 0);
            RectTransform rect = direction[i].GetComponent<RectTransform>();
            Vector3 self = rect.position;
            Vector3 target = new Vector3(action.targets[i].transform.position.x, self.y, action.targets[i].transform.position.z);
            

            float angle = Vector3.SignedAngle(target - self, transform.forward, Vector3.up);

            direction[i].transform.rotation = Quaternion.Euler(new Vector3(direction[i].transform.rotation.eulerAngles.x, direction[i].transform.rotation.eulerAngles.y, angle));
            direction[i].transform.position = Vector3.MoveTowards(self, target, Vector3.Distance(self, target) / 2);
            direction[i].transform.position += new Vector3(0, -.35f, 0);
            //rect.sizeDelta = new Vector2(rect.sizeDelta.x, Vector3.Distance(self, action.targets[i].transform.position));
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, (Vector3.Distance(target, self) / action.originator.transform.localScale.x));
            /*Color tempColor = direction.color; //Might be necessary?
            tempColor.a = .1f;
            direction.color = tempColor;*/
        }
        Debug.Log($"{action.originator.name} is doing the action {action.name} to {action.targets[0].name}");

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
            Rect rect;
            if (mouse.x > (Screen.width / 2))
            {
                rect = new Rect(mouse.x - 200, (Screen.height - mouse.y + 10), 200, 100);
            } else
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
            }
            GUI.Box(rect, $"Action name: {currentAction.name}\n Targets name: {names}\n Value: {currentAction.GetValue()}");
        }
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
        }
        else
        {
            string[] party = { this.gameObject.name };
            return party;
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
        StartCoroutine("DrawIntentions", action);
    }
    public void SetHealth(float health)
    {
        this.health = health;
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
