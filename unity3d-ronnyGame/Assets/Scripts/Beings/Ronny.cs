using System.Collections;
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
    private GameObject turnTarget = null;
    private bool drawn = false;
    private int index = 0;
    private string relation = null;
    //end turn data
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
        this.isBattle = true;
        this.GetComponent<playerMovement>().enabled = false;
        this.GetComponent<cameraRotation>().enabled = false;
    }
    public void InitializeTurn()
    {
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

    public override void RecalculateActions()
    {
        this.actionList = new List<Action>();
        this.actionList.Add(new Attack(3, this.damage * this.damageMultiplier, null));
        this.actionList.Add(new Heal(3, 3, null));
        this.actionList.Add(new BuffAttack(3, 3, 5, null));
    }
    public Action Turn(ListBeingData allFighters, List<Action> actionList)
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.index++;
            if (this.index > (actionList.Count - 1)) this.index = 0;
            Action newAction = actionList[this.index];
            if (this.currentAction != null) newAction.target = this.currentAction.target; //pass action.target along if you just choose a new action but have same target
            this.currentAction = newAction;
            this.currentAction.originator = this.gameObject;
            //yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.RightArrow)); // Jank because GetKeyDown Doesn't work in a coroutine

        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.index--;
            if (this.index < 0) this.index = actionList.Count - 1;
            Action newAction = actionList[this.index];
            if (this.currentAction != null) newAction.target = this.currentAction.target; //pass action.target along if you just choose a new action but have same target
            this.currentAction = newAction;
            this.currentAction.originator = this.gameObject;
            //yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.LeftArrow));
        }
        else if (this.currentAction == null)
        {
            this.currentAction = actionList[index];
            this.currentAction.originator = this.gameObject;
        }
        
        GameObject newTarget = this.ChooseTarget(allFighters);
        if ((this.turnTarget == null || (newTarget != this.turnTarget)) && newTarget != null)
        {
            this.turnTarget = newTarget;
            this.currentAction.target = this.turnTarget;
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

        if (Input.GetKey(KeyCode.Return) && relation != null && this.currentAction.IsValidAction(relation))
        {
            this.currentAction.target = this.turnTarget;
            return this.currentAction;
        } else
        {
            return null;
        }
    }
}
