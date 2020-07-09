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
    
    public override void Action()
    {
        base.Action();
    }
    public override string CompactBeingDataIntoJson()
    {
        BeingData being = JsonUtility.FromJson<BeingData>(this.beingData);
        being.location = this.gameObject.transform.position;
        being.angle = this.gameObject.transform.rotation;
        being.scale = this.gameObject.transform.localScale;

        RogueJson rogue = new RogueJson();
        rogue.speed = this.speed;
        rogue.health = this.health;
        rogue.damage = this.damage;
        rogue.party = this.party;

        being.jsonData = JsonUtility.ToJson(rogue);

        return JsonUtility.ToJson(being);
    }
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
                this.health = 10;
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

}
