using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RitterJson
{
    public GameObject ronny;
    public int virtue, damage;
    public float speed, health;
}
public class Ritter : Human
{
    public GameObject ronny;
    public bool follow = true;
    private new void Update()
    {
        if (ronny != null && follow)
        {
            this.FollowRonny();
        } else
        {
            ronny = this.GetRonny();
        }
    }
    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        return base.TurnAction(allFighters);
    }
    private void FollowRonny()
    {
        if (Vector3.Distance(this.transform.position, ronny.transform.position) > .5f)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, ronny.transform.position, this.speed * Time.deltaTime);
        }
    }
    private GameObject GetRonny()
    {
        return Kami.GetComponent<GameMaster>().GetPlayerGameObject();
    }
    public override void InitializeBattle()
    {
        switch (GetLevel())
        {
            case 0:
                health = 10;
                break;
            case 1:
                health = 20;
                break;
            case 2:
                health = 30;
                break;
            case 3:
                health = 40;
                break;
            case 4:
                health = 50;
                break;
            case 5:
                health = 60;
                break;
        }
        this.follow = false;
        base.InitializeBattle();
    }
    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {
            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);

            if (being.jsonData != null && being.jsonData != string.Empty)
            {
                RitterJson ritter = JsonUtility.FromJson<RitterJson>(being.jsonData);

                this.damage = ritter.damage;
                this.health = ritter.health;
                this.ronny = ritter.ronny;
                this.speed = ritter.speed;
                this.virtue = ritter.virtue;
            }
            this.ID = being.objectID;
            this.beingData = jsonData;
        }
        return;
    }
    public override void Interact()
    {
        //say something!
    }
    public override string UpdateBeingJsonData()
    {
        RitterJson ritter = new RitterJson();
        ritter.damage = this.damage;
        ritter.health = this.health;
        ritter.ronny = this.ronny;
        ritter.speed = this.speed;
        ritter.virtue = this.virtue;

        return JsonUtility.ToJson(ritter);
    }
}