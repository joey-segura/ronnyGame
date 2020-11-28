using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GluttonJson
{
    public int damage;
    public float speed, health, virtueValue;
    public string[] party;
}
public class Glutton : Enemy
{
    public float speed;

    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {

            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);
            if (being.jsonData != null && being.jsonData != string.Empty)
            {
                GluttonJson glutton = JsonUtility.FromJson<GluttonJson>(being.jsonData);
                this.speed = glutton.speed;
                this.health = glutton.health;
                this.damage = glutton.damage;
                this.party = glutton.party;
                this.virtueValue = glutton.virtueValue;
            }
            else
            {
                this.speed = 2;
                this.health = 30;
                this.damage = 2;
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
        this.actionList.Add(new Attack(3, this.damage, null));
        base.RecalculateActions();
    }
    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        this.RecalculateActions();
        GameObject joey = battleMasterScript.GetAllyObject();
        if (joey != null)
        {
            FighterAction action = this.actionList.Find(x => x.name == "Attack");
            action.targets = new GameObject[] { joey };
            action.originator = this.gameObject;
            this.currentAction = action;
            return action;
        } else
        {
            Debug.LogError("Can't find Joey target, returning skip action!");
            FighterAction skip = new Skip(1, null);
            return skip;
        }
    }
    public override string UpdateBeingJsonData()
    {
        GluttonJson glutton = new GluttonJson();
        glutton.speed = this.speed;
        glutton.health = this.health;
        glutton.damage = this.damage;
        glutton.party = this.party;
        glutton.virtueValue = this.virtueValue;
        return JsonUtility.ToJson(glutton);
    }
}
