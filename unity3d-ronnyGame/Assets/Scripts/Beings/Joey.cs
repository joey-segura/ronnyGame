using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoeyJson
{
    public GameObject ronny;
    public int virtue;
    public float speed, health, damage;
    public bool member;
}
public class Joey : Human
{
    public GameObject ronny;
    public float speed;
    public bool member = true, follow = true;
    private new void Update()
    {
        if (ronny != null && follow)
        {
            this.FollowRonny();
        }
        else
        {
            ronny = this.GetRonny();
        }
        base.Update();
    }
    private void Start()
    {
        if (!member)
        {
            follow = false;
            Debug.LogWarning("Start Joey wait sleep animation");
            //this.sleep.play() or this.animator.SetBool("sleep", true) or whatever
        }
    }
    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        return base.TurnAction(allFighters);
    }
    private void FollowRonny()
    {
        if (Vector3.Distance(this.transform.position, ronny.transform.position) > 5)
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
                JoeyJson joey = JsonUtility.FromJson<JoeyJson>(being.jsonData);

                this.damage = joey.damage;
                this.health = joey.health;
                this.ronny = joey.ronny;
                this.speed = joey.speed;
                this.virtue = joey.virtue;
                this.member = joey.member;
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
    public void JoinParty() 
    {
        this.member = true;
        this.follow = true;
    }
    public override void RecalculateActions()
    {
        this.actionList = new List<FighterAction>();
        switch (GetLevel())
        {
            case 0:
                this.actionList.Add(new Attack(3, this.damage * this.damageMultiplier, null));
                break;
            case 1:
                this.actionList.Add(new Attack(3, this.damage * this.damageMultiplier, null));
                this.actionList.Add(new Cleave(3, this.damage * this.damageMultiplier, null));
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
        }
        
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
        joey.member = this.member;

        return JsonUtility.ToJson(joey);
    }
}