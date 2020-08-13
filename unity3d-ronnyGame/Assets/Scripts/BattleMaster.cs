﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class BattleMaster : Kami
{
    private bool turn;
    public bool isBattle = false;
    private int enemyID { get; set; }
    private int turnCounter;
    private List<Action> intentions = new List<Action>();
    public ListBeingData allFighters = new ListBeingData();
    public ListBeingData partyMembers = new ListBeingData();
    public ListBeingData enemyMembers = new ListBeingData();

    private string worldSceneName;
    private string battleSceneName;

    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private Text virtue, action;
    private int virtueValue = 0, virtueMax = 0;

    public void AddFighter(BeingData being)
    {
        allFighters.BeingDatas.Add(being);
    }
    private void AssignScenes()
    {
        worldSceneName = sceneMaster.GetCurrentSceneName();
        battleSceneName = sceneMaster.GetBattleSceneName(worldSceneName);
    }
    public void AddToVirtue(int value)
    {
        if (value < 1)
        {
            value = 1;
        }
        this.virtueValue += value;
        UpdateVirtueText(this.virtueValue);
    }
    public void BattleEndCheck()
    {
        if (partyMembers.BeingDatas.Count == 0)
        {
            isBattle = false;
            StartCoroutine("EndBattle", false);
            //load save data because you died
            return;
        } else if (enemyMembers.BeingDatas.Count == 0)
        {
            isBattle = false;
            StartCoroutine("EndBattle", true);
            //maybe track rewards? anyways load the initial scene
            return;
        } else
        {
            return;
        }
    }
    private void CalculateVirtueMax()
    {
        int partyMember = 0;
        int enemies = 0;
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            switch (allFighters.BeingDatas[i].gameObject.tag)
            {
                case "Party":
                    Fighter fighter = allFighters.BeingDatas[i].gameObject.GetComponent<Fighter>();
                    partyMember += Mathf.RoundToInt(((fighter.health / 2) + fighter.damage) / 3);
                    break;
                case "Enemy":
                    Fighter enemy = allFighters.BeingDatas[i].gameObject.GetComponent<Fighter>();
                    enemies += Mathf.RoundToInt((enemy.health + enemy.damage) / 2);
                    break;
                default:
                    break;

            }
        }
        Debug.Log($"party ->{partyMember} and then enemies{enemies}");
        this.virtueMax = Mathf.RoundToInt((enemies * 2) / partyMember);
    }
    private static int CompareActionsByOriginatorTag(Action x, Action y)
    {
        if (x.originator.tag == "Party")
        {
            return 1;
        } else if (x.originator.tag == y.originator.tag)
        {
            return 0;
        } else
        {
            return -1;
        }
    }
    private void DestroyAllFighters()
    {
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            Destroy(allFighters.BeingDatas[i].gameObject);
            allFighters.BeingDatas.Remove(allFighters.BeingDatas[i]);
        }
    }
    private IEnumerator EndBattle(bool victory)
    {
        if (victory)
        {
            //play victory animations and ui and stuff
            Debug.Log("Victory!");
            yield return new WaitForSeconds(1);
            SubmitVirtueToAlly(this.virtueValue);
            //victory UI (rewards?) wait on click to load back to normal scene but till now we will just force our way back
            //yield return StartCoroutine(WaitForKeyDown());
            this.DestroyAllFighters();
            this.RemoveMemberReferenceInGameMasterByID(this.enemyID); //destroy the enemy reference before we load back in
            Destroy(Camera.main.gameObject);
            StopAllCoroutines();
            this.canvas.gameObject.SetActive(false);
            gameMaster.isSceneChanging = true;
            this.intentions = new List<Action>();
            this.allFighters = new ListBeingData();
            this.partyMembers = new ListBeingData();
            this.enemyMembers = new ListBeingData();
            this.isBattle = false;
            //yield return new WaitForSeconds(1); // have to wait to destroy every object before moving to the next scene
            sceneMaster.ChangeScene(worldSceneName);

        } else
        {
            //play defeat animations and ui and stuff
            Debug.Log("Defeat :(");
            yield return new WaitForSeconds(2);
            //reload last save UI
            //I.E (Continue?)
        }
        yield return null;
    }
    public void FillMembers(ListBeingData ronnyParty, ListBeingData enemyParty)
    {
        partyMembers = ronnyParty;
        enemyMembers = enemyParty;
    }
    private List<Action> GetIntentions()
    {
        List<Action> actions = new List<Action>();

        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            if (allFighters.BeingDatas[i].gameObject.tag != "Player")
            {
                Fighter fighter = allFighters.BeingDatas[i].gameObject.GetComponent<Fighter>();
                actions.Add(fighter.GetIntention(allFighters));
            }
        }
        actions.Sort(CompareActionsByOriginatorTag);
        return actions;
    }
    private GameObject GetPlayerObject()
    {
        GameObject player = null;
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            if (allFighters.BeingDatas[i].gameObject.tag == "Player")
            {
                player = allFighters.BeingDatas[i].gameObject;
            }
        }
        return player;
    }
    public void InitializeBattle(ListBeingData partyMembers, ListBeingData enemyMembers)
    {
        this.AssignScenes();
        for (int i = 0; i < partyMembers.BeingDatas.Count; i++)
        {
            partyMembers.BeingDatas[i].location = new Vector3(-1.45f, .4f, -1.5f + (1.5f * i));
        }
        for (int i = 0; i < enemyMembers.BeingDatas.Count; i++)
        {
            enemyMembers.BeingDatas[i].location = new Vector3(1.45f, .4f, -1.5f + (1.5f * i));
        }
        this.FillMembers(partyMembers, enemyMembers);
        sceneMaster.ChangeScene(this.battleSceneName);
        this.turnCounter = 0;
        this.virtueValue = 0;
        this.InitializeFighters();
        this.MoveCameraTo(1.4f, 4, -6);
        this.CalculateVirtueMax();
        this.UpdateVirtueText(0);
        this.canvas.gameObject.SetActive(true);
        this.isBattle = true;
        this.NewTurn();
    }
    public void InitializeFighters()
    {
        ListBeingData allFigthers = new ListBeingData();
        for (int i = 0; i < partyMembers.BeingDatas.Count; i++)
        {
            allFigthers.BeingDatas.Add(partyMembers.BeingDatas[i]);
        }
        for (int i = 0; i < enemyMembers.BeingDatas.Count; i++)
        {
            allFigthers.BeingDatas.Add(enemyMembers.BeingDatas[i]);
        }
        for (int i = 0; i < allFigthers.BeingDatas.Count; i++)
        {
            BeingData being = allFigthers.BeingDatas[i];
            gameMaster.InstantiateObject(JsonUtility.ToJson(being));
        }
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            allFighters.BeingDatas[i].gameObject.GetComponent<Fighter>().InitializeBattle(); //stops all movement scripts of each fighter
        }
        this.UpdateBothPartiesFromAllFigthers();
    }
    private void MoveCameraTo(float x, float y, float z)
    {
        Camera cam = Camera.main;
        cam.transform.position += new Vector3(x, y, z);
        cam.transform.parent = this.transform;
    }
    private void NewTurn()
    {
        intentions = new List<Action>();
        intentions = this.GetIntentions();
        StartCoroutine("PlayerAction");
    }
    private void PlayAnimation(Animation anim)
    {
        if (anim != null)
        {
            anim.Play();
        } else
        {
            Debug.LogWarning("There is no animation to play in the action");
            return;
        }
    }
    private IEnumerator PlayerAction()
    {
        this.turn = true;
        GameObject player = this.GetPlayerObject();
        Ronny ronny = player.GetComponent<Ronny>();
        ronny.ApplyEffects();
        ronny.RecalculateActions();
        ronny.InitializeTurn();
        List<Action> actionList = ronny.GetActions().ToList<Action>();
        Action action = null;
        yield return new WaitUntil(() => (action = ronny.Turn(allFighters, actionList)) != null);
        this.turnCounter++;
        ronny.ToggleCanvas();
        ronny.StartCoroutine("BattleMove");
        this.PlayAnimation(action.animation);
        yield return new WaitForSeconds(action.duration);
        CoroutineWithData cd = new CoroutineWithData(this, this.ProcessAction(action));
        while (!cd.finished)
        {
            yield return new WaitForEndOfFrame();
        }
        ronny.StartCoroutine("MoveToBattlePosition");
        turn = false;
        StartCoroutine("ProcessAIActions");
    }
    private IEnumerator ProcessAction(Action action)
    {
        if (action.targets != null)
        {
            Debug.Log($"{action.originator.name} just used {action.name} on {action.targets[0].name} for {action.GetValue()} whatever!");
            CoroutineWithData cd = new CoroutineWithData(this, action.Execute());
            while (!cd.finished)
            {
                yield return new WaitForEndOfFrame();
            }
            float damage = action.GetValue();
            if (damage != 0 && action.originator.tag == "Party")
            {
                Debug.Log($"Local Virtue increased by {Mathf.RoundToInt(Mathf.Abs(action.virtueValue) / 3)}");
                this.AddToVirtue(Mathf.RoundToInt(Mathf.Abs(action.virtueValue) / 3));
            }
            yield return true;
        } else
        {
            yield return true;
        }
    }
    public IEnumerator ProcessAIActions()
    {
        yield return new WaitForEndOfFrame(); //waiting for action data to settle in case Ronny(player) influences an AI's action
        for (int i = 0; i < intentions.Count; i++)
        {
            if (intentions[i].originator != null)
            {
                Fighter fighter = intentions[i].originator.GetComponent<Fighter>();
                fighter.ApplyEffects();
                if (fighter != null)
                {
                    fighter.RecalculateActions();
                    Action action = fighter.GetCurrentAction(); //gets current action instead of playing intention incase ronny influences it
                    this.PlayAnimation(action.animation);
                    Debug.Log($"{action.originator.name}'s turn!");
                    fighter.ToggleCanvas();
                    fighter.StartCoroutine("BattleActionMove", action);
                    yield return new WaitForSeconds(action.duration);

                    StartCoroutine("ProcessAction", action);
                }
            }
        }
        yield return new WaitForSeconds(1); // wait 1 second for new turn to start!;
        this.NewTurn();
        yield return null;
    }
    private void RemoveAllMembers()
    {
        partyMembers = new ListBeingData();
        enemyMembers = new ListBeingData();
    }
    public void RemoveMemberByID(int ID) //Removes any member from either party by comparing its object ID to those in each party
    {
        for (int i = 0; i < partyMembers.BeingDatas.Count; i++)
        {
            if (partyMembers.BeingDatas[i].objectID == ID)
            {
                partyMembers.BeingDatas.Remove(partyMembers.BeingDatas[i]);
                return;
            }
        }
        for (int i = 0; i < enemyMembers.BeingDatas.Count; i++)
        {
            if (enemyMembers.BeingDatas[i].objectID == ID)
            {
                enemyMembers.BeingDatas.Remove(enemyMembers.BeingDatas[i]);
            }
        }
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            if (allFighters.BeingDatas[i].objectID == ID)
            {
                allFighters.BeingDatas.Remove(allFighters.BeingDatas[i]);
            }
        }
    }
    private void RemoveMemberReferenceInGameMasterByID(int ID)
    {
        gameMaster.RemoveBeingFromList(ID);
    }
    public void SetActionText(string text)
    {
        action.text = $"Action: {text}";
    }
    public void SetEnemyID(int ID)
    {
        this.enemyID = ID;
    }
    private void SubmitVirtueToAlly(int virtue)
    {
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            if (allFighters.BeingDatas[i].gameObject.tag == "Party")
            {
                allFighters.BeingDatas[i].gameObject.GetComponent<Human>().AddToVirtue(virtue);
            }
        }
    }
    private void UpdateBothPartiesFromAllFigthers()
    {
        ListBeingData allyMembers = new ListBeingData();
        ListBeingData foeMembers = new ListBeingData();
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            GameObject self = allFighters.BeingDatas[i].gameObject;
            if (self.gameObject.tag == "Enemy")
            {
                foeMembers.BeingDatas.Add(allFighters.BeingDatas[i]);
            } else
            {
                allyMembers.BeingDatas.Add(allFighters.BeingDatas[i]);
            }
        }
        this.FillMembers(allyMembers, foeMembers);
    }
    public void UpdateVirtueText(int increase)
    {
        this.virtue.text = $"Virtue expectation: {virtueValue}/{this.virtueMax}";
    }
}