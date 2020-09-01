using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueJson
{
    public float speed, health, damage;
    public string[] party;
}
public class Rogue : Fighter
{
    public float speed;
   
    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {

            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);
            if (being.jsonData != null && being.jsonData != string.Empty)
            {
                RogueJson rogue = JsonUtility.FromJson<RogueJson>(being.jsonData);
                this.speed = rogue.speed;
                this.health = rogue.health;
                this.damage = rogue.damage;
                this.party = rogue.party;
            }
            else
            {
                this.speed = 0;
                this.health = 5;
                this.damage = 3;
                this.party = null;
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
        this.actionList.Add(new AttackAndBuff(3, this.damage * this.damageMultiplier, 2, 2, null));
        //this.actionList.Add(new Attack(3, this.damage * this.damageMultiplier, null));
        /*
        this.actionList.Add(new WeakAttack(3, 3, 2, null));
        this.actionList.Add(new BuffAttack(3, 3, 2, null));
        this.actionList.Add(new BolsterDefense(3, 3, 2, null));
        this.actionList.Add(new VulnerableAttack(3, 3, 2, null));
        */
        base.RecalculateActions();
    }
    public override string UpdateBeingJsonData()
    {
        RogueJson rogue = new RogueJson();
        rogue.speed = this.speed;
        rogue.health = this.health;
        rogue.damage = this.damage;
        rogue.party = this.party;
        return JsonUtility.ToJson(rogue);
    }
}
