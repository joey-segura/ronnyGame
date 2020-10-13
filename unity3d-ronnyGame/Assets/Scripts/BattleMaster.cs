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
    public int turnCounter;
    public List<FighterAction> intentions = new List<FighterAction>();
    public ListBeingData allFighters = new ListBeingData();
    public ListBeingData partyMembers = new ListBeingData();
    public ListBeingData enemyMembers = new ListBeingData();

    private string worldSceneName;
    private string battleSceneName;
    public GameObject[] shadows;

    [SerializeField]
    private Canvas canvas;
    public Text virtue, virtueExpectation, action;
    public Image abilityKeys;
    public Texture holder, healthBar, damageBar, costBar, virtueBar;
    public float expectedDamageTaken, costDamage;
    private bool isFlashing = false;
    private float ronnyMaxHP = 0, guiX, guiY;
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
    public void AddToVirtue(int virt)
    {
        Debug.Log($"Local virtue changed by {virt}");
        this.virtueValue += virt;
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
        float virt = 0;
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            if (allFighters.BeingDatas[i].gameObject.tag == "Enemy")
            {
                virt += allFighters.BeingDatas[i].gameObject.GetComponent<Enemy>().virtueValue;
            }
        }
        this.virtueMax = Mathf.RoundToInt(virt);
    }
    private static int CompareActionsByOriginatorTag(FighterAction x, FighterAction y)
    {
        if (x.originator.CompareTag("Party"))
        {
            return 1;
        } else if (y.originator.CompareTag(x.originator.tag))
        {
            return 0;
        } else
        {
            return -1;
        }
    }
    private static int CompareShadowsByTag(FighterShadow x, FighterShadow y)
    {
        if (x.CompareTag("Party"))
        {
            return 1;
        }
        else if (y.CompareTag(x.tag))
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }
    private void DestroyAllFighters()
    {
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            Being being = allFighters.BeingDatas[i].gameObject.GetComponent<Being>();
            BeingData beingData = allFighters.BeingDatas[i];
            beingData.jsonData = being.UpdateBeingJsonData();
            gameMaster.UpdateBeingJsonDataInList(beingData);
            Destroy(allFighters.BeingDatas[i].gameObject);
        }
        this.RemoveAllMembers();
    }
    private void DestroyAllShadows()
    {
        for (int i = 0; i < shadows.Length; i++)
        {
            DestroyImmediate(shadows[i]);
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
            this.canvas.gameObject.SetActive(false);
            this.intentions = new List<FighterAction>();
            gameMaster.isSceneChanging = true;
            this.isBattle = false;
            //yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(1); // have to wait to destroy every object before moving to the next scene
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
    public IEnumerator FlashingBars ()
    {
        isFlashing = !isFlashing;
        yield return new WaitForSeconds(.5f);
        StartCoroutine(FlashingBars());
    }
    public void FillMembers(ListBeingData ronnyParty, ListBeingData enemyParty)
    {
        partyMembers = ronnyParty;
        enemyMembers = enemyParty;
    }
    public GameObject GetAllyObject()
    {
        GameObject ally = null;
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            if (allFighters.BeingDatas[i].gameObject.name == "Joey" || allFighters.BeingDatas[i].gameObject.name == "Ritter")
            {
                ally = allFighters.BeingDatas[i].gameObject;
            }
        }
        return ally;
    }
    private List<FighterAction> GetIntentions()
    {
        List<FighterAction> actions = new List<FighterAction>();

        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            if (allFighters.BeingDatas[i].gameObject != null && allFighters.BeingDatas[i].gameObject.tag != "Player")
            {
                Fighter fighter = allFighters.BeingDatas[i].gameObject.GetComponent<Fighter>();
                FighterAction action = fighter.GetIntention(allFighters);
                if (action != null)
                {
                    actions.Add(action);
                }
            }
        }
        actions.Sort(CompareActionsByOriginatorTag);
        return actions;
    }
    private GameObject GetPlayerObject()
    {
        GameObject player = null;
        foreach (BeingData data in allFighters.BeingDatas)
        {
            if (data.gameObject != null && data.gameObject.tag == "Player")
            {
                player = data.gameObject;
            }
        }
        return player;
    }
    public void InitializeBattle(ListBeingData partyMembers, ListBeingData enemyMembers)
    {
        this.AssignScenes();
        for (int i = 0; i < partyMembers.BeingDatas.Count; i++)
        {
            partyMembers.BeingDatas[i].location = new Vector3(-12, 0, -7.5f + (12 * i));
        }
        for (int i = 0; i < enemyMembers.BeingDatas.Count; i++)
        {
            enemyMembers.BeingDatas[i].location = new Vector3(12, 0, -7.5f + (12 * i));
        }
        this.FillMembers(partyMembers, enemyMembers);
        sceneMaster.ChangeScene(this.battleSceneName);
        this.turnCounter = 0;
        this.virtueValue = 0;
        this.InitializeFighters();
        this.ronnyMaxHP = GetPlayerObject().GetComponent<Ronny>().health;
        this.MoveCameraTo(-0.05f, 7.23f, -13.4f, 30.066f, 0, 0);
        this.CalculateVirtueMax();
        StartCoroutine(FlashingBars());
        this.UpdateVirtueText(0);
        this.canvas.gameObject.SetActive(true);
        this.isBattle = true;
        if (gameMaster.firstBattle)
        {
            StartCoroutine(ShowKeys());
            gameMaster.firstBattle = false;
        }
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
            being.angle = new Quaternion(0, 0, 0, 0);
            gameMaster.InstantiateObject(JsonUtility.ToJson(being));
        }
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            allFighters.BeingDatas[i].gameObject.GetComponent<Fighter>().InitializeBattle(); //stops all movement scripts of each fighter
        }
        this.UpdateBothPartiesFromAllFigthers();
    }
    private void MoveCameraTo(float x, float y, float z, float ex, float ey, float ez)
    {
        Camera cam = Camera.main;
        cam.transform.position = new Vector3(x, y, z);
        cam.transform.rotation = Quaternion.Euler(ex, ey, ez);
        cam.transform.parent = this.transform;
    }
    private void NewTurn()
    {
        if (this.isBattle)
        {
            intentions = new List<FighterAction>();
            intentions = this.GetIntentions();
            virtueExpectation.text = "0";
            expectedDamageTaken = 0;
            costDamage = 0;
            StartCoroutine(PlayerAction());
        }
    }
    private void OnGUI()
    {
        if (isBattle)
        {
            guiX = (Screen.width / 2) - (Screen.width / 2) / 2;
            guiY = Screen.height / 10;
            Ronny ronny = GetPlayerObject().GetComponent<Ronny>();
            float healthWidth = (Screen.width / 2) * (ronny.health / ronnyMaxHP) / 1.0875f;
            float costWidth = 0;
            Rect holderRect = new Rect(guiX, guiY/2, Screen.width / 2, Screen.height / 25);
            Rect virtueHolderRect = new Rect(guiX, guiY/2, Screen.width / 2, Screen.height / 10);
            Rect healthRect = new Rect(guiX * 1.1f, guiY/1.13f, healthWidth, Screen.height / 30);
            GUI.DrawTexture(virtueHolderRect, virtueBar, ScaleMode.ScaleToFit);
            //GUI.DrawTexture(virtueHolderRect, virtueBar);
            GUI.DrawTexture(healthRect, healthBar);
            //GUI.DrawTexture(holderRect, holder);

            if (costDamage > 0 && isFlashing)
            {
                costWidth = healthWidth * (costDamage / ronny.health);
                Rect costRect = new Rect((guiX * 1.1f) + (healthWidth - costWidth), guiY / 1.13f, costWidth, Screen.height / 30);
                GUI.DrawTexture(costRect, costBar);
            }
            if (expectedDamageTaken < 0 && isFlashing)
            {
                float damageWidth = healthWidth / (ronny.health / Mathf.Abs(expectedDamageTaken));
                Rect damageRect = new Rect((guiX * 1.1f) + (healthWidth - damageWidth - costWidth), guiY / 1.13f, damageWidth, Screen.height / 30);
                GUI.DrawTexture(damageRect, damageBar);
            }
        }
    }
    private void PlayAnimation(Animation anim)
    {
        if (anim != null)
        {
            Debug.Log("Animation tied to battlemaster? seems wrong");
            anim.Play();
        } else
        {
            Debug.LogWarning("There is no animation to play in the action");
            return;
        }
    }
    private IEnumerator PlayerAction()
    {
        if (this.turnCounter == 0)
        {
            yield return new WaitForEndOfFrame(); // make sure all data is initialized like all entity sprites (yes this was a problem)
        }
        this.turn = true;
        GameObject player = this.GetPlayerObject();
        Ronny ronny = player.GetComponent<Ronny>();
        ronny.TickEffects();
        ronny.RecalculateActions();
        ronny.InitializeTurn();
        List<FighterAction> actionList = ronny.GetActions().ToList<FighterAction>();
        FighterAction action = null;
        yield return new WaitUntil(() => (action = ronny.Turn(allFighters, actionList)) != null);
        ronny.UnhighlightAll();
        this.turnCounter++;
        DestroyAllShadows();
        //ronny.ToggleCanvas();
        ronny.StartCoroutine("BattleMove");
        this.PlayAnimation(action.animation);
        yield return new WaitForSeconds(action.duration);
        CoroutineWithData cd = new CoroutineWithData(this, this.ProcessAction(action));
        while (!cd.finished)
        {
            yield return new WaitForEndOfFrame();
        }
        ronny.AddToHealth(action.GetCost() * -1, ronny);
        ronny.StartCoroutine("MoveToBattlePosition");
        costDamage = 0;
        turn = false;
        StartCoroutine("ProcessAIActions");
    }
    public IEnumerator ProcessAction(FighterAction action)
    {
        if (action.targets != null)
        {
            Debug.Log($"{action.originator.name} just used {action.name} on {action.targets[0].name} for {action.GetValue()} whatever!");
            CoroutineWithData cd = new CoroutineWithData(this, action.Execute());
            while (!cd.finished)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        yield return true;
    }
    public IEnumerator ProcessAIActions()
    {
        yield return new WaitForEndOfFrame(); //waiting for action data to settle in case Ronny(player) influences an AI's action
        for (int i = 0; i < intentions.Count; i++)
        {
            if (intentions[i].originator != null)
            {
                Fighter fighter = intentions[i].originator.GetComponent<Fighter>();
                fighter.TickEffects();
                if (fighter != null && fighter.currentAction != null)
                {
                    FighterAction action = fighter.currentAction; //gets current action instead of playing intention in case ronny influences it
                    Debug.Log($"{action.originator.name}'s turn!");
                    CoroutineWithData move = new CoroutineWithData(this, fighter.MoveToBattleTarget(action));
                    while (!move.finished)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                    this.PlayAnimation(action.animation);
                    yield return new WaitForSeconds(action.duration);
                    fighter.RecalculateActions();
                    action = fighter.currentAction;
                    StartCoroutine("ProcessAction", action);
                    CoroutineWithData moveBack = new CoroutineWithData(this, fighter.MoveToBattlePosition());
                    while (!moveBack.finished)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                    yield return new WaitForSeconds(1);
                }
            }
        }
        yield return new WaitForSeconds(3); // wait 1 second for new turn to start!;
        this.NewTurn();
        yield return null;
    }
    private void RemoveAllMembers()
    {
        allFighters = new ListBeingData();
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
    public void SetActionText(FighterAction a)
    {
        action.text = $"{a.name} \n{a.description}";
    }
    public void SetEnemyID(int ID)
    {
        this.enemyID = ID;
    }
    public IEnumerator ShowKeys()
    {
        float timeElapsed = 0;
        bool active = false;
        while (timeElapsed < 5)
        {
            abilityKeys.gameObject.SetActive(active = !active);
            yield return new WaitForSeconds(.5f);
            timeElapsed += .5f;
            yield return new WaitForEndOfFrame();
        }
        
        //yield return new WaitForSeconds(5);
        abilityKeys.gameObject.SetActive(false);
        yield return null;
    }
    public void SimulateBattle()
    {
        this.virtueExpectation.text = "Expected Gain: 0";

        DestroyAllShadows();
        expectedDamageTaken = 0;
        shadows = new GameObject[allFighters.BeingDatas.Count];
        FighterShadow[] shadowScripts = new FighterShadow[allFighters.BeingDatas.Count];
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            Vector3 spawnLoc = allFighters.BeingDatas[i].gameObject.transform.position + new Vector3(allFighters.BeingDatas[i].gameObject.transform.position.x > 0 ? -6f : 6f, 0,0);
            Fighter origin = allFighters.BeingDatas[i].gameObject.GetComponent<Fighter>();
            shadows[i] = visualEffectMaster.InstantiateVisualSprite(Resources.Load("Prefabs/Shadow"),
                spawnLoc,
                allFighters.BeingDatas[i].gameObject.transform.rotation,
                allFighters.BeingDatas[i].gameObject.transform);
            shadowScripts[i] = shadows[i].GetComponent<FighterShadow>();
            shadowScripts[i].InjectShadowData(origin);
        }
        List<FighterShadow> list = shadowScripts.ToList<FighterShadow>();
        list.Sort(CompareShadowsByTag);
        shadowScripts = list.ToArray();
        for (int i = 0; i < shadowScripts.Length; i++)
        {
            shadowScripts[i].SimulateAction();
        }
    }
    private void SubmitVirtueToAlly(int virtue)
    {
        int virtueGain = virtue - this.virtueMax;
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            if (allFighters.BeingDatas[i].gameObject.name == "Joey" || allFighters.BeingDatas[i].gameObject.name == "Ritter")
            {
                allFighters.BeingDatas[i].gameObject.GetComponent<Human>().AddToVirtue(virtueGain);
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