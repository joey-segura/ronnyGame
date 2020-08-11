using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class Fighter : Being
{
    public float health, damage, damageMultiplier = 1, defenseMultiplier = 1;
    public bool isStunned = false, isPoisoned = false;
    protected bool isBattle = false;
    
    public string[] party;
    protected Action currentAction = null;
    private List<Effect> currentEffects = new List<Effect>();
    protected List<Action> actionList = new List<Action>();

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
    public void AddEffect(Effect effect)
    {
        currentEffects.Add(effect);
        currentEffects.Sort(CompareEffectsByDuration);
    }
    public float AddToHealth(float change)
    {
        change = change / this.defenseMultiplier;
        Debug.Log($"{this.name}'s health just got changed by {change}");
        this.health += change;
        this.DeathCheck();
        return change;
    }
    public void ApplyEffects()
    {
        this.damageMultiplier = 1; //these values get reset every turn so that 1 effect doesn't proc twice
        this.defenseMultiplier = 1;
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
    public virtual Action ChooseAction(GameObject target)
    {
        Action action = null;
        string relation = this.TargetRelationToSelf(target.tag);
        bool valid = false;
        while (!valid)
        {
            int actionIndex = Random.Range(0, this.actionList.Count);
            action = this.actionList[actionIndex]; //random action for enemy and friendly units
            if (action.IsValidAction(relation))
            {
                action.originator = this.gameObject;
                action.target = target;
                this.currentAction = action;
                valid = true;
            }
        }
        return action;
    }
    public virtual GameObject ChooseTarget(ListBeingData allFighters) //chooses a target at random!
    {
        GameObject target = null;
        
        bool validTarget = false;
        int attempts = 0;
        while (!validTarget)
        {
            int targetIndex = Random.Range(0, allFighters.BeingDatas.Count);
            target = allFighters.BeingDatas[targetIndex].gameObject;
            string relation = this.TargetRelationToSelf(target.tag);
            if (this.ValidTarget(relation))
            {
                validTarget = true;
            }
            if (attempts >= 10) //If no target could be found with valid actions (E.G., a buffer type unit who only has buff friendlies has no more teammates left)
            {
                return null; 
            } else
            {
                attempts++;
            }
        }
        return target;
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
    public IEnumerator DrawIntentions(Action action)
    {
        yield return new WaitForEndOfFrame(); //waiting a frame to make sure data is settled before we do this call (Not a fan)
        Debug.Log($"{action.originator.name} is doing the action {action.name} to {action.target.name}");
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
        Vector3 target = new Vector3(action.target.transform.position.x, direction.rectTransform.position.y, action.target.transform.position.z);

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
        return; // this is for any enemy script to disable their movement script once the battle starts
    }
    private void OnGUI()
    {
        if (isBattle && currentAction != null && this.isHovering)
        {
            Vector3 mouse = Input.mousePosition;
            Rect rect = new Rect(mouse.x + 10, (Screen.height - mouse.y + 10), 200, 100);
            GUI.Box(rect, $"Action name: {currentAction.name}\n Target name: {currentAction.target.name}\n Value: {currentAction.GetValue()}");
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
    public void SetAction(Action action)
    {
        this.currentAction = action;
        StartCoroutine("DrawIntentions", action);
    }
    public void SetHealth(float health)
    {
        this.health = health;
    }
    public List<Action> GetActions()
    {
        return this.actionList;
    }
    public Action GetCurrentAction()
    {
        return this.currentAction;
    }
    public Action GetIntention(ListBeingData allFighters)
    {
        if (this.isStunned)
        {
            this.ApplyEffects();
            return null;
        }
        this.ApplyEffects();
        this.RecalculateActions();
        GameObject target = this.ChooseTarget(allFighters);
        Action action = this.ChooseAction(target);
        this.StartCoroutine("DrawIntentions", action);
        return action;
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
    private bool ValidTarget(string targetTag)
    {
        bool valid = false;
        for (int i = 0; i < actionList.Count; i++)
        {
            if (actionList[i].IsValidAction(targetTag))
            {
                valid = true;
            }
        }
        return valid;
    }
} 
