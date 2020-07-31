using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RonnyJson
{
    public float speed, health, damage;
}
public class Ronny : Fighter
{
    private GameObject target = null;
    public float speed;
    public void ChangeSpeed(float newSpeed)
    {
        this.gameObject.GetComponent<playerMovement>().speed = newSpeed;
    }
    public override Action ChooseAction(GameObject target)
    {
        Action action = this.actionList[0]; //replace this with a function that lets you choose what action you want to do
        action.originator = this.gameObject;
        action.target = target;
        return action;
    }
    public override GameObject ChooseTarget(ListBeingData allFighters)
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 25))
            {
                this.target = hit.collider.gameObject;
            }
        }
        return this.target;
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

        RonnyJson ronny = new RonnyJson();
        //
        //! needs to assign RonnyJson values to updated values
        ronny.speed = this.speed;
        ronny.health = this.health;
        ronny.damage = this.damage;
        //
        being.jsonData = JsonUtility.ToJson(ronny);

        return JsonUtility.ToJson(being);
    }
    public override void InitializeBattle()
    {
        this.GetComponent<playerMovement>().enabled = false;
        this.GetComponent<cameraRotation>().enabled = false;
    }
    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {
            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);

            if (being.jsonData != null && being.jsonData != string.Empty)
            {
                RonnyJson ronny = JsonUtility.FromJson<RonnyJson>(being.jsonData);

                this.health = ronny.health;
                this.damage = ronny.damage;
                this.speed = ronny.speed;
                this.ChangeSpeed(this.speed);
            }
            this.ID = being.objectID;
            this.beingData = jsonData;
        }
        return;
    }
    public override void Interact()
    {
        
    }

    public override void RecalculateActions()
    {
        this.actionList = new List<Action>();
        this.actionList.Add(new Attack(3, this.damage, null));
    }
}
