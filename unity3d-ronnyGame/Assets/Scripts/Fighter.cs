using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]

public class Fighter : Being
{
    public float health, damage, damageMultiplier = 1, defenseMultiplier = 1;
    public bool isStunned = false, isPoisoned = false;
    public string[] party;
    private List<Effect> currentEffects = new List<Effect>();
    protected List<Action> actionList = new List<Action>();
    //public Canvas canvas;
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
        this.DeathCheck();
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
    private IEnumerator DrawIntentions(Action action)
    {
        yield return new WaitForEndOfFrame();
        this.ToggleCanvas();
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
        direction.gameObject.SetActive(false);
        Texture2D image = Resources.Load(action.GetImagePath()) as Texture2D;
        Debug.Log(image.name);
        Sprite sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0, 0));
        sprite.name = action.GetImagePath();
        intention.sprite = sprite;
        
        yield return null;
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
        GameObject target = null;
        
        bool validTarget = false;
        while (!validTarget)
        {
            int targetIndex = Random.Range(0, allFighters.BeingDatas.Count);
            target = allFighters.BeingDatas[targetIndex].gameObject;
            string relation = this.TargetRelationToSelf(target.tag);
            if (this.ValidTarget(relation))
            {
                Debug.Log($"found valid target {this.gameObject.name} found {relation}");
                validTarget = true;
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
    public virtual void InitializeBattle()
    {
        return; // this is for any enemy script to disable their movement script once the battle starts
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
    public Action GetIntention(ListBeingData allFighters)
    {
        this.ApplyEffects();
        this.RecalculateActions();
        GameObject target = this.ChooseTarget(allFighters);
        Action action = this.ChooseAction(target);
        this.StartCoroutine("DrawIntentions", action);
        Debug.DrawLine(action.originator.transform.position, action.target.transform.position, Color.red, 10);
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
    private string TargetRelationToSelf(string targetTag)
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
