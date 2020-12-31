using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RonnyJson
{
    public int virtue, damage;
    public float speed, health;
}
public class Ronny : Human
{
    private GameObject target = null;
    public bool exec = false;
    //turn data
    public GameObject turnTarget = null;
    public int lastActionSelected = 0, wLevel, dLevel, sLevel, aLevel;
    //end turn data

    public Material Outline;

    public override void AddToHealth(int change, Fighter causer)
    {
        if (causer != this) battleMasterScript.expectedDamageTaken -= change;
        base.AddToHealth(change, causer);
    }
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
            if (Physics.Raycast(ray, out hit, 35))
            {
                this.target = hit.collider.gameObject;
            }
        } else
        {
            this.target = null;
        }
        return this.target;
    }
    public override void InitializeBattle()
    {
        this.health = 30; //default value
        playerMovement pm = this.GetComponent<playerMovement>();
        pm.anim.SetFloat("Facing", 1);
        pm.enabled = false;
        this.GetComponent<cameraRotation>().enabled = false;
        this.GetComponent<SkyboxRoatation>().enabled = false;
        base.InitializeBattle();
    }
    public void InitializeTurn()
    {
        this.currentAction = null;
        this.turnTarget = null;
        this.lastActionSelected = 0;
        this.wLevel = 1;
        this.dLevel = 1;
        this.sLevel = 1;
        this.aLevel = 1;
        this.UnhighlightAll();
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
    public override void RecalculateActions()
    {
        this.actionList = new List<FighterAction>();
        //this.actionList.Add(new BlockAll(3, null));
        //this.actionList.Add(new CommandToBlock(3, null));
        //this.actionList.Add(new Attack(3, this.damage * this.damageMultiplier, null));
        //this.actionList.Add(new Heal(3, 5, null));
        this.actionList.Add(new Mark(1, null));
        this.actionList.Add(new WeakAttack(3, 3, 1, null));
        this.actionList.Add(new BuffAttack(3, 3, 1, null));
        this.actionList.Add(new BolsterDefense(3, 3, 1, null));
        this.actionList.Add(new VulnerableAttack(3, 3, 1, null));
        this.actionList.Add(new Taunt(3, null));
        this.actionList.Add(new Skip(1, null));
        //this.actionList.Add(new Cleave(3, this.damage * this.damageMultiplier, null));
        //this.actionList.Add(new TauntAll(3, null));
        //base.RecalculateActions();
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
    private GameObject[] SetActionTargets(GameObject[] targets)
    {
        UnhighlightAll();
        if (targets == null)
        {
            return null;
        }
        for (int i = 0; i < targets.Length; i++)
        {
            GameObject targ = targets[i];
            SpriteRenderer targRend = targ.GetComponent<SpriteRenderer>();
            if (targRend.material.name.Contains("Outline"))
            {
                targ.GetComponent<SpriteOutline>().enabled = true;
            } else
            {
                targRend.material = Outline;
                targ.AddComponent<SpriteOutline>();
                targ.GetComponent<SpriteOutline>().enabled = true;
            }
        }
        return targets;
    }
    private void SetNewAction(FighterAction action)
    {
        if (action == null) // if this happens then death
        {
            Debug.LogError("null");
            Debug.LogError(currentAction);
        }
        switch (lastActionSelected)
        {
            case (int)KeyCode.W:
                action.skillLevel = Mathf.Abs(wLevel);
                break;
            case (int)KeyCode.D:
                action.skillLevel = Mathf.Abs(dLevel);
                break;
            case (int)KeyCode.S:
                action.skillLevel = Mathf.Abs(sLevel);
                break;
            case (int)KeyCode.A:
                action.skillLevel = Mathf.Abs(aLevel);
                break;
        }
        action.originator = this.gameObject;
        action.LevelUpdate();
        
        if (action.IsActionAOE())
        {
            action.targets = SetActionTargets(this.currentAction.GetAOETargets(battleMasterScript.allFighters));
        } else
        {
            if (turnTarget != null)
            {
                if (action.IsValidAction(TargetRelationToSelf(turnTarget)))
                {
                    action.targets = SetActionTargets(new GameObject[] { turnTarget });
                } else
                {
                    action.targets = SetActionTargets(null);
                }
            } else
            {
                action.targets = SetActionTargets(null);
            }
        }
        this.currentAction = action;

        BattleMaster battleMaster = this.transform.GetComponentInParent<BattleMaster>();
        battleMaster.SetActionIcon(lastActionSelected, action);
        battleMaster.costDamage = action.GetCost();
        battleMaster.SimulateBattle();
    }
    public FighterAction Turn(ListBeingData allFighters, List<FighterAction> actionList)
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            lastActionSelected = (int)KeyCode.W;
            if (wLevel > 0)
                SetNewAction(GetActionByName("Mark"));
            else
                SetNewAction(GetActionByName("Taunt"));
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            lastActionSelected = (int)KeyCode.D;
            if (dLevel > 0)
                SetNewAction(GetActionByName("Buff Attack"));
            else
                SetNewAction(GetActionByName("Weak Attack"));
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            lastActionSelected = (int)KeyCode.S;
            SetNewAction(GetActionByName("Skip"));
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            lastActionSelected = (int)KeyCode.A;
            if (aLevel > 0)
                SetNewAction(GetActionByName("Bolster Defense"));
            else
                SetNewAction(GetActionByName("Vulnerable Attack"));
        }
        else if (this.currentAction == null)
        {
            lastActionSelected = (int)KeyCode.W;
            this.SetNewAction(GetActionByName("Mark"));
        }
        if (!this.currentAction.IsActionAOE())
        {
            GameObject newTarget = this.ChooseTarget(allFighters);
            if (newTarget != null)
            {
                turnTarget = newTarget;
                string relation = this.TargetRelationToSelf(newTarget);
                if (this.currentAction.IsValidAction(relation) && (this.currentAction.targets == null || this.currentAction.targets[0] != newTarget)) //we can access the first area because we know it only has 1 member (if/else proves this)
                {
                    this.currentAction.targets = SetActionTargets(new GameObject[] { newTarget });
                    battleMasterScript.SimulateBattle();
                }
            }
        }
        if ((Input.GetKey(KeyCode.Return) || exec) && (this.currentAction.targets != null || this.currentAction.name.Contains("Skip")))
        {
            return this.currentAction;
        } else
        {
            exec = false;
            return null;
        }
    }
    public void UpdateSkills(bool increase)
    {
        RecalculateActions();
        switch (lastActionSelected)
        {
            case (int)KeyCode.W:
                if (increase)
                {
                    if (wLevel == -1)
                    {
                        wLevel = 1;
                        SetNewAction(GetActionByName("Mark"));
                        currentAction.skillLevel = wLevel;
                        break;
                    }
                    if (currentAction.skillLevel < currentAction.levelCap || wLevel < 0)
                    {
                        wLevel++;
                        currentAction.skillLevel = Mathf.Abs(wLevel);
                    }
                }
                else
                {
                    if (wLevel == 1)
                    {
                        wLevel = -1;
                        SetNewAction(GetActionByName("Taunt"));
                        currentAction.skillLevel = wLevel;
                        break;
                    }
                    if (currentAction.skillLevel < currentAction.levelCap || wLevel > 0)
                    {
                        wLevel--;
                        currentAction.skillLevel = Mathf.Abs(wLevel);
                    }
                }
                break;
            case (int)KeyCode.D:
                if (increase)
                {
                    if (dLevel == -1)
                    {
                        dLevel = 1;
                        SetNewAction(GetActionByName("Buff Attack"));
                        currentAction.skillLevel = dLevel;
                        break;
                    }
                    if (currentAction.skillLevel < currentAction.levelCap || dLevel < 0)
                    {
                        dLevel++;
                        currentAction.skillLevel = Mathf.Abs(dLevel);
                    }
                } else
                {
                    if (dLevel == 1)
                    {
                        dLevel = -1;
                        SetNewAction(GetActionByName("Weak Attack"));
                        currentAction.skillLevel = dLevel;
                        break;
                    }
                    if (currentAction.skillLevel < currentAction.levelCap || dLevel > 0)
                    {
                        dLevel--;
                        currentAction.skillLevel = Mathf.Abs(dLevel);
                    }
                }
                break;
            case (int)KeyCode.S:
                break;
            case (int)KeyCode.A:
                if (increase)
                {
                    if (aLevel == -1)
                    {
                        aLevel = 1;
                        SetNewAction(GetActionByName("Bolster Defense"));
                        currentAction.skillLevel = aLevel;
                        break;
                    }
                    if (currentAction.skillLevel < currentAction.levelCap || aLevel < 0)
                    {
                        aLevel++;
                        currentAction.skillLevel = Mathf.Abs(aLevel);
                    }
                }
                else
                {
                    if (aLevel == 1)
                    {
                        aLevel = -1;
                        SetNewAction(GetActionByName("Vulnerable Attack"));
                        currentAction.skillLevel = aLevel;
                        break;
                    }
                    if (currentAction.skillLevel < currentAction.levelCap || aLevel > 0)
                    {
                        aLevel--;
                        currentAction.skillLevel = Mathf.Abs(aLevel);
                    }
                }
                break;
        }
        currentAction.LevelUpdate();
        SetNewAction(currentAction);
        battleMasterScript.SetActionIcon(lastActionSelected, currentAction);
    }
    public new void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            UpdateSkills(true);
        } else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            UpdateSkills(false);
        }
        base.Update();
    }
    public void ToggleMovementAndCamera()
    {
        playerMovement pm = this.GetComponent<playerMovement>();
        cameraRotation cr = this.GetComponent<cameraRotation>();
        pm.Idle();
        pm.enabled = pm.isActiveAndEnabled ? false : true;
        cr.enabled = cr.isActiveAndEnabled ? false : true;
    }
    public void UnhighlightAll()
    {
        for (int i = 0; i < battleMasterScript.allFighters.BeingDatas.Count; i++)
        {
            SpriteOutline outline = battleMasterScript.allFighters.BeingDatas[i].gameObject.GetComponent<SpriteOutline>();
            if (outline)
            {
                outline.enabled = false;
            }
        }
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
