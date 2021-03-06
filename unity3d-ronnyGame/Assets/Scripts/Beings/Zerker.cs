using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zerker : Enemy
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
                this.health = 10;
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
    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        GameObject joey = battleMasterScript.GetAllyObject();
        if (!HasEffect("Berserk"))
        {
            FighterAction action = new Berserker(3, 3, 10, 2, null);
            action.targets = new GameObject[] { this.gameObject };
            action.originator = this.gameObject;
            this.currentAction = action;
            return action;
        } else
        {
            if (joey != null)
            {
                FighterAction action = new Attack(3, 3, this.damage, null);
                action.targets = new GameObject[] { joey };
                action.originator = this.gameObject;
                this.currentAction = action;
                return action;
            } else
            {
                FighterAction skip = new Skip(1, null);
                return skip;
            }
        }
    }
    public override string UpdateBeingJsonData()
    {
        return GenericEnemyJsonify();
    }
}
