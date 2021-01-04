using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rogue : Enemy
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
                this.health = 4;
                this.damage = 3;
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
        this.actionList.Add(new AttackAndBuff(3, Mathf.FloorToInt(this.damage * this.damageMultiplier), 2, 2, null));
        base.RecalculateActions();
    }
    public override string UpdateBeingJsonData()
    {
        return GenericEnemyJsonify();
    }
}
