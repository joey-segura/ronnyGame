using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RitterJson
{
    public GameObject ronny;
    public float courage, speed, health, damage;
}
public class Ritter : Human
{
    public GameObject ronny;
    public float courage, speed;
    private void Update()
    {
        if (ronny != null)
        {
            this.FollowRonny();
        } else
        {
            ronny = this.GetRonny();
        }
    }
    public override Action ChooseAction(GameObject target)
    {
        return base.ChooseAction(target);
    }
    public override GameObject ChooseTarget(ListBeingData allFighters)
    {
        return base.ChooseTarget(allFighters);
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

        RitterJson ritter = new RitterJson();
        ritter.damage = this.damage;
        ritter.health = this.health;
        ritter.ronny = this.ronny;
        ritter.speed = this.speed;

        being.jsonData = JsonUtility.ToJson(ritter);

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
        this.speed = 0;
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
        this.actionList = new List<Action>();
        this.actionList.Add(new Attack(3, this.damage * this.damageMultiplier, null));
        this.actionList.Add(new Attack(3, this.damage * this.damageMultiplier, null));
        this.actionList.Add(new WeakAttack(3, 3, 2, null));
        this.actionList.Add(new BuffAttack(3, 3, 2, null));
        this.actionList.Add(new BolsterDefense(3, 3, 2, null));
        this.actionList.Add(new VulnerableAttack(3, 3, 2, null));
    }
}