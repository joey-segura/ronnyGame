using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TikiMan : Enemy
{
    public int charge = 0;
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
                this.health = 16;
                this.damage = 6;
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
    public int ChargeUp()
    {
        return this.charge++;
    }

    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        this.RecalculateActions();
        GameObject joey = battleMasterScript.GetAllyObject();
        if (joey != null)
        {
            ChargedStunAttack action = new ChargedStunAttack(3, 3, this.damage, 1, 1, null);
            action.onExecute = (() => ChargeUp());
            action.charge = this.charge;
            action.targets = new GameObject[] { this.charge == 1 ? joey : this.gameObject };
            if (charge >= 1)
                charge = 0;
            action.originator = this.gameObject;
            this.currentAction = action;
            return action;
        } else
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
