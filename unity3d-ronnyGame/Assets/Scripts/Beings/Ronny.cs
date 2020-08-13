﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RonnyJson
{
    public float speed, health, damage;
}
public class Ronny : Human
{
    private GameObject target = null;
    public float speed;

    //turn data
    public GameObject turnTarget = null;
    private bool drawn = false;
    private int index = 0;
    private string relation = null;
    //end turn data

    public IEnumerator BattleMove()
    {
        float distance = 0;
        if (this.transform.position.x > 0)
        {
            distance = -.25f;
        }
        else
        {
            distance = .25f;
        }
        Vector3 newPos = new Vector3(this.transform.position.x + distance, this.transform.position.y, this.transform.position.z);

        while (this.transform.position != newPos)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, newPos, .01f);
            yield return new WaitForEndOfFrame();
        }
    }
    
    public void ChangeSpeed(float newSpeed)
    {
        this.gameObject.GetComponent<playerMovement>().speed = newSpeed;
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
        base.InitializeBattle();
    }
    public void InitializeTurn()
    {
        this.currentAction = null;
        this.turnTarget = null;
        this.drawn = false;
        this.index = 0;
        this.relation = null;
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
    public IEnumerator MoveToBattlePosition()
    {
        while (this.transform.position != this.battlePosition)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, this.battlePosition, .01f);
            yield return new WaitForEndOfFrame();
        }
    }

    public override void RecalculateActions()
    {
        this.actionList = new List<Action>();
        this.actionList.Add(new Attack(3, this.damage * this.damageMultiplier, null));
        this.actionList.Add(new Heal(3, 3, null));
        this.actionList.Add(new BuffAttack(3, 3, 5, null));
        this.actionList.Add(new CommandToAttack(3, null));
    }
    public GameObject ReturnChoosenGameObject()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 25))
            {
                return hit.collider.gameObject;
            }
        }
        return null;
    }
    private void SetNewAction(Action action)
    {
        if (this.currentAction != null)
        {
            action.targets = this.currentAction.targets;
        }
        action.originator = this.gameObject;
        this.currentAction = action;
        this.transform.GetComponentInParent<BattleMaster>().SetActionText(action.name);
    }
    public Action Turn(ListBeingData allFighters, List<Action> actionList)
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.index++;
            if (this.index > (actionList.Count - 1)) this.index = 0;
            this.SetNewAction(this.actionList[this.index]);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.index--;
            if (this.index < 0) this.index = actionList.Count - 1;
            this.SetNewAction(this.actionList[this.index]);
        }
        else if (this.currentAction == null)
        {
            this.SetNewAction(this.actionList[this.index]);
        }
        
        if (this.currentAction.IsActionAOE())
        {
            this.currentAction.GetAOETargets(allFighters);
        } else
        {
            GameObject newTarget = this.ChooseTarget(allFighters);
            if ((this.turnTarget == null || (newTarget != this.turnTarget)) && newTarget != null)
            {
                this.turnTarget = newTarget;
                this.currentAction.targets = new GameObject[] { this.turnTarget };
                relation = this.TargetRelationToSelf(newTarget.tag);
                if (!this.currentAction.IsValidAction(relation) && drawn) this.ToggleCanvas();
                this.drawn = false;
            }

            if (this.turnTarget != null && this.currentAction != null && !this.drawn && relation != null && this.currentAction.IsValidAction(relation))
            {
                this.drawn = true;
                StartCoroutine(this.DrawIntentions(this.currentAction));
            }
            else if (relation != null && !this.currentAction.IsValidAction(relation) && this.drawn)
            {
                this.drawn = false;
                this.ToggleCanvas();
            }
        }
        

        if (Input.GetKey(KeyCode.Return) && relation != null && this.currentAction.IsValidAction(relation))
        {
            return this.currentAction;
        } else
        {
            return null;
        }
    }
}
