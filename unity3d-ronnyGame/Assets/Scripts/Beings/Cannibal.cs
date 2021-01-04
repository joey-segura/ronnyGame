using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannibal : Enemy
{
    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {

            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);
            if (being.jsonData != null && being.jsonData != string.Empty)
            {
                GenericEnemyInject(being.jsonData);
            }
            else
            {
                this.speed = 0;
                this.health = 6;
                this.damage = 2;
                this.party = null;
                this.virtueValue = 2;
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
    public override void InitializeBattle()
    {
        base.InitializeBattle();
        Effect lifeSteal = new LifeSteal(99);
        this.AddEffect(this, lifeSteal);
        return;
    }
    public override void RecalculateActions()
    {
        this.actionList = new List<FighterAction>();
        this.actionList.Add(new DoubleAttack(3, this.damage, null));
        //this.actionList.Add(new LifeSteal(3, this.damage, null));
        base.RecalculateActions();
    }
    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        this.RecalculateActions();
        GameObject joey = battleMasterScript.GetAllyObject();
        if (joey != null)
        {
            FighterAction action = this.actionList.Find(x => x.name == "Double Attack");
            action.targets = new GameObject[] { joey };
            action.originator = this.gameObject;
            this.currentAction = action;
            return action;
        }
        else
        {
            FighterAction skip = new Skip(1, null);
            skip.originator = this.gameObject;
            this.currentAction = skip;
            return skip;
        }
    }
    public override string UpdateBeingJsonData()
    {
        return GenericEnemyJsonify();
    }
}
