using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoeyJson
{
    public GameObject ronny;
    public int virtue, damage;
    public float speed, health;
    public bool member;
}
public class Joey : Human
{
    public GameObject ronny;
    public bool member = true, follow = true, rage = false;
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
        GameObject target = null;
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            if (allFighters.BeingDatas[i].gameObject.tag == "Enemy")
            {
                target = allFighters.BeingDatas[i].gameObject;
                break;
            }
        }
        FighterAction action = new Attack(2, .5f, this.damage, null, this.attackSounds[0]);
        action.originator = this.gameObject;
        action.targets = new GameObject[] { target };
        this.currentAction = action;
        return action;
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
    public override void LoadAttackSounds()
    {
        this.attackSounds = new AudioClip[1];
        this.attackSounds[0] = Resources.Load($"Sounds/Scenes/Joey/Fighters/Joey/Firehit_1", typeof(AudioClip)) as AudioClip;
    }
    public void RageMode()
    {
        this.rage = true;
        this.damage = 9999;
        this.health = 9999;
        SpriteRenderer spr = this.GetComponent<SpriteRenderer>();
        spr.color = Color.red;
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