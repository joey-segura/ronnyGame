using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RonnyJson
{
    public int virtue;
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
        float distance = .25f;
        if (this.transform.position.x > 0)
        {
            distance = distance * -1;
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
    public void CurrentActionDecrement()
    {
        this.index--;
        if (this.index < 0) this.index = actionList.Count - 1;
        this.SetNewAction(this.actionList[this.index]);
    }
    public void CurrentActionIncrement()
    {
        this.index++;
        if (this.index > (actionList.Count - 1)) this.index = 0;
        this.SetNewAction(this.actionList[this.index]);
    }
  
    public override void InitializeBattle()
    {
        this.health = 60; //default value
        playerMovement pm = this.GetComponent<playerMovement>();
        pm.anim.SetFloat("Facing", 1);
        pm.enabled = false;
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
                this.virtue = ronny.virtue;
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
        this.actionList = new List<FighterAction>();
        //this.actionList.Add(new Attack(3, this.damage * this.damageMultiplier, null));
        this.actionList.Add(new Heal(3, 5, null));
        this.actionList.Add(new BuffAttack(3, 3, 5f, null));
        //this.actionList.Add(new Cleave(3, this.damage * this.damageMultiplier, null));
        this.actionList.Add(new CommandToAttack(3, null));
        this.actionList.Add(new TauntAll(3, null));
        base.RecalculateActions();
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
    private void SetNewAction(FighterAction action)
    {
        if (this.currentAction != null)
        {
            action.targets = this.currentAction.targets;
        }
        action.originator = this.gameObject;
        this.currentAction = action;

        BattleMaster battleMaster = this.transform.GetComponentInParent<BattleMaster>();
        battleMaster.SetActionText(action.name);
        StartCoroutine(this.DrawIntentions(this.currentAction));
        if (this.currentAction.targets != null) battleMaster.PredictVirtueGainWithPlayerAction(this.currentAction);

    }
    public FighterAction Turn(ListBeingData allFighters, List<FighterAction> actionList)
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CurrentActionIncrement();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CurrentActionDecrement();
        }
        else if (this.currentAction == null)
        {
            this.SetNewAction(this.actionList[this.index]);
        }
        
        if (this.currentAction.IsActionAOE())
        {
            this.currentAction.targets = this.currentAction.GetAOETargets(allFighters);
        } else
        {
            GameObject newTarget = this.ChooseTarget(allFighters);
            if (newTarget != null)
            {
                string relation = this.TargetRelationToSelf(newTarget);
                if (this.currentAction.IsValidAction(relation))
                {
                    this.currentAction.targets = new GameObject[] { newTarget };
                    StartCoroutine(this.DrawIntentions(this.currentAction));
                    this.transform.GetComponentInParent<BattleMaster>().PredictVirtueGainWithPlayerAction(this.currentAction);
                }
            }
        }
        
        if (Input.GetKey(KeyCode.Return) && this.currentAction.targets != null)
        {
            return this.currentAction;
        } else
        {
            return null;
        }
    }
    public void ToggleMovementAndCamera()
    {
        playerMovement pm = this.GetComponent<playerMovement>();
        cameraRotation cr = this.GetComponent<cameraRotation>();
        pm.Idle();
        pm.enabled = pm.isActiveAndEnabled ? false : true;
        cr.enabled = cr.isActiveAndEnabled ? false : true;
    }
    public override string UpdateBeingJsonData()
    {
        RonnyJson ronny = new RonnyJson();
        ronny.speed = this.speed;
        ronny.health = this.health;
        ronny.damage = this.damage;
        ronny.virtue = this.virtue;
        
        return JsonUtility.ToJson(ronny);
    }
}
