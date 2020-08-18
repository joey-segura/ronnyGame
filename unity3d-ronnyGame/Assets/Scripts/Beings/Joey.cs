using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoeyJson
{
    public GameObject ronny;
    public int virtue;
    public float speed, health, damage;
}
public class Joey : Human
{
    public GameObject ronny;
    public float speed;
    private bool follow = true;
    private void Update()
    {
        if (ronny != null && follow)
        {
            this.FollowRonny();
        }
        else
        {
            ronny = this.GetRonny();
        }
    }
    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        return base.TurnAction(allFighters);
    }
    public override string CompactBeingDataIntoJson()
    {
        BeingData being = JsonUtility.FromJson<BeingData>(this.beingData);
        being.location = this.gameObject.transform.position;
        being.angle = this.gameObject.transform.rotation;
        being.scale = this.gameObject.transform.localScale;
        being.gameObject = this.gameObject;
        being.prefabName = this.gameObject.name;
        being.objectID = this.ID;

        being.jsonData = this.UpdateBeingJsonData();

        return JsonUtility.ToJson(being);
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
        GameObject Kami = this.transform.parent.gameObject;
        return Kami.GetComponent<GameMaster>().GetPlayerGameObject();
    }
    public override void InitializeBattle()
    {
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
                JoeyJson joey = JsonUtility.FromJson<JoeyJson>(being.jsonData);

                this.damage = joey.damage;
                this.health = joey.health;
                this.ronny = joey.ronny;
                this.speed = joey.speed;
                this.virtue = joey.virtue;
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
    public override void RecalculateActions()
    {
        this.actionList = new List<FighterAction>();
        this.actionList.Add(new Attack(3, this.damage * this.damageMultiplier, null));
        //this.actionList.Add(new Cleave(3, this.damage * this.damageMultiplier, null));
        //this.actionList.Add(new ApplyThorns(3, 3, .5f, null));
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
        JoeyJson joey = new JoeyJson();
        joey.damage = this.damage;
        joey.health = this.health;
        joey.ronny = this.ronny;
        joey.speed = this.speed;
        joey.virtue = this.virtue;

        return JsonUtility.ToJson(joey);
    }
}