using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TikiManJson
{
    public float speed, health, damage, virtueValue;
    public string[] party;
}
public class TikiMan : Enemy
{
    public float speed;

    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {

            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);
            if (being.jsonData != null && being.jsonData != string.Empty)
            {
                TikiManJson tikiMan = JsonUtility.FromJson<TikiManJson>(being.jsonData);
                this.speed = tikiMan.speed;
                this.health = tikiMan.health;
                this.damage = tikiMan.damage;
                this.party = tikiMan.party;
                this.virtueValue = tikiMan.virtueValue;
            }
            else
            {
                this.speed = 0;
                this.health = 4;
                this.damage = 3;
                this.party = null;
                this.virtueValue = 3;
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
        this.actionList.Add(new Attack(3, this.damage * this.damageMultiplier, null));
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
        FighterAction action;
        switch (battleMasterScript.turnCounter)
        {
            case 0:
                action = new VulnerableAttack(3, 6, 2, null);
                action.targets = new GameObject[] { this.gameObject };
                action.originator = this.gameObject;
                this.currentAction = action;
                return action;
            default:
                return base.TurnAction(allFighters);
        }
    }
    public override string UpdateBeingJsonData()
    {
        TikiManJson tikiMan = new TikiManJson();
        tikiMan.speed = this.speed;
        tikiMan.health = this.health;
        tikiMan.damage = this.damage;
        tikiMan.party = this.party;
        tikiMan.virtueValue = this.virtueValue;
        return JsonUtility.ToJson(tikiMan);
    }
}
