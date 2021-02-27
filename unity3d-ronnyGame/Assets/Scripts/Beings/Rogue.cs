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
    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        return base.TurnAction(allFighters);
    }
    public override string UpdateBeingJsonData()
    {
        return GenericEnemyJsonify();
    }
}
