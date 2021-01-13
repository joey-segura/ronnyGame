using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavageJson
{
    public int damage;
    public float speed, health, virtueValue;
    public string[] party;
}
public class Savage : Enemy
{
    public float speed;

    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {

            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);
            if (being.jsonData != null && being.jsonData != string.Empty)
            {
                SavageJson savage = JsonUtility.FromJson<SavageJson>(being.jsonData);
                this.speed = savage.speed;
                this.health = savage.health;
                this.damage = savage.damage;
                this.party = savage.party;
                this.virtueValue = savage.virtueValue;
            }
            else
            {
                this.speed = 0;
                this.health = 1;
                this.damage = 2;
                this.party = null;
                this.virtueValue = 1;
            }
            this.ID = being.objectID;
            this.beingData = jsonData;
        }
        return;
    }
    public override void Interact()
    {
        base.Interact();
    }
    public override void RecalculateActions()
    {
        this.actionList = new List<FighterAction>();
        //this.actionList.Add(new AttackAndBuff(3, this.damage * this.damageMultiplier, 2, 2, null));
        this.actionList.Add(new Attack(3, Mathf.FloorToInt(this.damage * this.damageMultiplier), null));
        /*
        this.actionList.Add(new WeakAttack(3, 3, 2, null));
        this.actionList.Add(new BuffAttack(3, 3, 2, null));
        this.actionList.Add(new BolsterDefense(3, 3, 2, null));
        this.actionList.Add(new VulnerableAttack(3, 3, 2, null));
        */
        base.RecalculateActions();
    }
    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        this.RecalculateActions();
        GameObject baby = null;
        GameObject joey = null;
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            if (allFighters.BeingDatas[i].gameObject != null && allFighters.BeingDatas[i].gameObject.name.Contains("Baby"))
            {
                baby = allFighters.BeingDatas[i].gameObject;
            }
            if (allFighters.BeingDatas[i].gameObject != null && allFighters.BeingDatas[i].gameObject.name.Contains("Joey"))
            {
                joey = allFighters.BeingDatas[i].gameObject;
            }
        }
        if (baby != null)
        {
            FighterAction action = this.actionList.Find(x => x.name == "Attack");
            action.targets = new GameObject[] { baby };
            action.originator = this.gameObject;
            this.currentAction = action;
            return action;
        } else
        {
            FighterAction action = this.actionList.Find(x => x.name == "Attack");
            action.targets = new GameObject[] { joey };
            action.originator = this.gameObject;
            this.currentAction = action;
            return action;
        }
    }
    public override string UpdateBeingJsonData()
    {
        SavageJson savage = new SavageJson();
        savage.speed = this.speed;
        savage.health = this.health;
        savage.damage = this.damage;
        savage.party = this.party;
        savage.virtueValue = this.virtueValue;
        return JsonUtility.ToJson(savage);
    }
}
