using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TikiManJson
{
    public int damage;
    public float speed, health, virtueValue;
    public string[] party;
    public Vector4[] patrolPoints;
}
public class TikiMan : Enemy
{
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
                this.patrolPoints = tikiMan.patrolPoints == null ? null : (Vector4[])tikiMan.patrolPoints.Clone();
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
        TikiManJson tikiMan = new TikiManJson();
        tikiMan.speed = this.speed;
        tikiMan.health = this.health;
        tikiMan.damage = this.damage;
        tikiMan.party = this.party;
        tikiMan.virtueValue = this.virtueValue;
        tikiMan.patrolPoints = this.patrolPoints == null ? null : (Vector4[])this.patrolPoints.Clone();
        return JsonUtility.ToJson(tikiMan);
    }
}
