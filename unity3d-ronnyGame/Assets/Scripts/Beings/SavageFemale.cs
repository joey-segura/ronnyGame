using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavageFemale : Enemy
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
                this.health = 12;
                this.damage = 3;
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
    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        this.RecalculateActions();
        GameObject baby = null;
        GameObject joey = battleMasterScript.GetAllyObject();
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            if (allFighters.BeingDatas[i].gameObject != null && allFighters.BeingDatas[i].gameObject.name.Contains("Baby"))
            {
                baby = allFighters.BeingDatas[i].gameObject;
            }
        }
        if (baby == null && joey == null)
        {
            FighterAction skip = new Skip(1, null);
            skip.originator = this.gameObject;
            this.currentAction = skip;
            return skip;
        }
        else
        {
            FighterAction action = new Attack(3, this.damage, null);
            action.targets = baby == null ? new GameObject[] { joey } : new GameObject[] { baby };
            action.originator = this.gameObject;
            this.currentAction = action;
            return action;
        }
    }
    public override string UpdateBeingJsonData()
    {
        return GenericEnemyJsonify();
    }
}
