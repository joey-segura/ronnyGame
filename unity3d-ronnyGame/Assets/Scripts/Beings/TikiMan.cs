using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TikiManJson
{
    public int damage;
    public float speed, health, virtueValue;
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
                this.health = 16;
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
    public override void RecalculateActions()
    {
        this.actionList = new List<FighterAction>();
        //this.actionList.Add(new AttackAndBuff(3, this.damage * this.damageMultiplier, 2, 2, null));
        //this.actionList.Add(new Attack(3, Mathf.FloorToInt(this.damage * this.damageMultiplier), null));
        this.actionList.Add(new StunAttack(3, this.damage, 1, null));
        this.actionList.Add(new Skip(1, null)); //play resting animation
        base.RecalculateActions();
    }
    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        this.RecalculateActions();
        GameObject joey = battleMasterScript.GetAllyObject();
        if (battleMasterScript.turnCounter % 2 == 0)
        {
            if (joey != null)
            {
                FighterAction action = this.actionList.Find(x => x.name == "Stun Attack");
                action.targets = new GameObject[] { joey };
                action.originator = this.gameObject;
                this.currentAction = action;
                return action;
            }
        }
        FighterAction skip = this.actionList.Find(x => x.name == "Skip");
        skip.originator = this.gameObject;
        this.currentAction = skip;
        return skip;
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
