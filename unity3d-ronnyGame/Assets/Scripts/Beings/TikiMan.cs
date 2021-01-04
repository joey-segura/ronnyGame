using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TikiMan : Enemy
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
    public override void RecalculateActions()
    {
        if (this.actionList.Count == 0)
        {
            this.actionList = new List<FighterAction>();
            this.actionList.Add(new StunAttack(3, this.damage, 1, null));
        } else
        {
            StunAttack action = (StunAttack)this.actionList.Find(x => x.name == "Stun Attack");
            action.damage = this.damage;
        }
        base.RecalculateActions();
    }
    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        this.RecalculateActions();
        GameObject joey = battleMasterScript.GetAllyObject();
        if (joey != null)
        {
            StunAttack action = (StunAttack)this.actionList.Find(x => x.name == "Stun Attack");
            action.targets = new GameObject[] { action.charge == 1 ? joey : this.gameObject };
            action.originator = this.gameObject;
            this.currentAction = action;
            return action;
        } else
        {
            FighterAction skip = this.actionList.Find(x => x.name == "Skip");
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
