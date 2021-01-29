using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class BattleMaster : Kami
{
    private bool turn;
    public bool isBattle = false, playingShadows = false;
    private int enemyID { get; set; }
    public int turnCounter;
    public List<FighterAction> intentions = new List<FighterAction>();
    public ListBeingData allFighters = new ListBeingData();
    public ListBeingData partyMembers = new ListBeingData();
    public ListBeingData enemyMembers = new ListBeingData();

    private string worldSceneName;
    private string battleSceneName;
    public GameObject[] shadows;
    public FighterShadow[] shadowScripts;

    [SerializeField]
    private Canvas canvas;
    public Image abilityKeys, actionIcon, actionIconModifier;
    public Button upArrow, downArrow, simulateButton, executeButton;
    public Sprite[] actionIcons; //0 minus - 1 plus - 2 atk - 3 cmnd - 4 def
    public Texture holder, healthBar, damageBar, costBar, virtueBar;
    public float expectedDamageTaken, costDamage, expectedVirtueGain;
    public bool isFlashing = false;
    private float ronnyMaxHP = 0, guiX, guiY;
    private int virtueValue = 0, virtueMax = 0;

    public new void Awake()
    {
        //save.onClick.AddListener(Save);
        upArrow.onClick.AddListener(delegate { ChangeActionModifier(true); });
        downArrow.onClick.AddListener(delegate { ChangeActionModifier(false); });
        simulateButton.onClick.AddListener(SimulateShadowsButton);
        executeButton.onClick.AddListener(ExecuteTurn);
        base.Awake();
    }

    public void AddFighter(BeingData being)
    {
        allFighters.BeingDatas.Add(being);
    }
   
    public void AddToVirtue(int virt)
    {
        Debug.Log($"Local virtue changed by {virt}");
        this.virtueValue += virt;
        if (virtueValue >= virtueMax)
        {
            AllyVirtueFail();
        }
    }
    public void AllyVirtueFail()
    {
        GameObject ally = GetAllyObject();
        if (ally.name == "Joey")
        {
            ally.GetComponent<Joey>().RageMode();
        } else if (ally.name == "Ritter")
        {

        }
    }
    private void AssignScenes()
    {
        worldSceneName = sceneMaster.GetCurrentSceneName();
        battleSceneName = sceneMaster.GetBattleSceneName(worldSceneName);
    }
    public void BattleEndCheck()
    {
        if (partyMembers.BeingDatas.Count == 0)
        {
            isBattle = false;
            StartCoroutine(EndBattle(false));
            //load save data because you died
            return;
        } else if (enemyMembers.BeingDatas.Count == 0)
        {
            isBattle = false;
            StartCoroutine(EndBattle(true));
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
    private void ChangeActionModifier(bool increase)
    {
        GetPlayerObject().GetComponent<Ronny>().UpdateSkills(increase);
    }
    public void CleanseAllFighters()
    {
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            allFighters.BeingDatas[i].gameObject.GetComponent<Fighter>().RemoveAllEffects();
        }
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
        if (y.gameObject.tag == "Party")
        {
            return -1;
        }
        else if (y.gameObject.tag == "Player")
        {
            return 1;
        }
        else
        {
            return 0;
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
            CleanseAllFighters();
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
    private void ExecuteTurn()
    {
        GetPlayerObject().GetComponent<Ronny>().exec = true;
    }
    public IEnumerator FlashingBars ()
    {
        while(isFlashing != !isFlashing)
        {
            yield return new WaitForSecondsRealtime(.5f);
            isFlashing = !isFlashing;
        }
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
    public GameObject GetShadow(GameObject parent)
    {
        GameObject shadow = null;
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            if (allFighters.BeingDatas[i].gameObject == parent)
            {
                for (int j = 0; j < parent.transform.childCount; j++)
                {
                    if (parent.transform.GetChild(j).name.Contains("Shadow"))
                    {
                        shadow = parent.transform.GetChild(j).gameObject;
                        break;
                    }
                }
            }
        }
        return shadow;
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
        this.UpdateBothPartiesFromAllFigthers();
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            allFighters.BeingDatas[i].gameObject.GetComponent<Fighter>().InitializeBattle(); //stops all movement scripts of each fighter
        }
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            allFighters.BeingDatas[i].gameObject.GetComponent<Fighter>().BattleStart(); //stops all movement scripts of each fighter
        }

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
            expectedVirtueGain = 0;
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
            float healthWidth = (Screen.width / 2) * (ronny.health / ronnyMaxHP) / 1.085f;
            float costWidth = 0;
            //Rect holderRect = new Rect(guiX, guiY/2, Screen.width / 2, Screen.height / 25);
            Rect virtueHolderRect = new Rect(guiX, guiY/2, Screen.width / 2, Screen.height / 10);
            Rect healthRect = new Rect(guiX * 1.0965f, guiY / .965f, healthWidth, Screen.height / 90);
            float increment = virtueHolderRect.width / 20;
            int index = this.virtueValue;
            float offset = increment * .4f;
            int maxVirt = this.virtueMax;
            Rect maxVirtueRect = new Rect(guiX * 1.035f + (maxVirt * (Screen.width / 41.45f)), guiY / 1.365f, Screen.width / 42f, Screen.height / 42);
            //Rect testRect = new Rect(guiX + (increment * index) + offset, (healthRect.y / 1.8f + offset), increment / 1.17f, Screen.height / 40);
            /*for (int i = 0; i < 20; i++)
            {
                if (i % 2 == 0)
                {
                    Rect rect = new Rect(guiX * 1.035f + (i * (Screen.width / 41.45f)), guiY / 1.365f, Screen.width / 42f, Screen.height / 42);
                    GUI.DrawTexture(rect, healthBar);
                }
            }*/
            
            GUI.DrawTexture(maxVirtueRect, damageBar);
            for (int i = 0; i < index; i++)
            {
                Rect rect = new Rect(guiX * 1.035f + (i * (Screen.width / 41.45f)), guiY / 1.365f, Screen.width / 42f, Screen.height / 42);
                GUI.DrawTexture(rect, healthBar);
            }
            if (isFlashing && expectedVirtueGain != 0)
            {
                for (int i = virtueValue; i < (virtueValue + expectedVirtueGain); i++)
                {
                    Rect rect = new Rect(guiX * 1.035f + (i * (Screen.width / 41.45f)), guiY / 1.365f, Screen.width / 42f, Screen.height / 42);
                    GUI.DrawTexture(rect, costBar);
                }
            }
            //GUI.DrawTexture(virtueHolderRect, virtueBar, ScaleMode.ScaleToFit);
            //GUI.DrawTexture(virtueHolderRect, virtueBar);
            GUI.DrawTexture(healthRect, healthBar);
            
            //GUI.DrawTexture(holderRect, holder);

            if (costDamage > 0 && isFlashing)
            {
                costWidth = healthWidth * (costDamage / ronny.health);
                Rect costRect = new Rect((guiX * 1.1f) + (healthWidth - costWidth), healthRect.y, costWidth, healthRect.height);
                GUI.DrawTexture(costRect, costBar);
            }
            if (expectedDamageTaken < 0 && isFlashing)
            {
                float damageWidth = healthWidth / (ronny.health / Mathf.Abs(expectedDamageTaken));
                Rect damageRect = new Rect((guiX * 1.1f) + (healthWidth - damageWidth - costWidth), healthRect.y, damageWidth, healthRect.height);
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
        ronny.StartCoroutine(ronny.BattleMove());
        this.PlayAnimation(action.animation);
        yield return new WaitForSeconds(action.duration);
        CoroutineWithData cd = new CoroutineWithData(this, this.ProcessAction(action));
        while (!cd.finished)
        {
            yield return new WaitForEndOfFrame();
        }
        ronny.AddToHealth(action.GetCost() * -1, ronny);
        ronny.StartCoroutine(ronny.MoveToBattlePosition()); ;
        costDamage = 0;
        turn = false;
        StartCoroutine(ProcessAIActions());
    }
    public IEnumerator ProcessAction(FighterAction action)
    {
        if (action.targets != null && action.targets[0] != null)
        {
            Debug.Log($"{action.originator.name} just used {action.name} on {action.targets[0].name}!");
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
                if (fighter.isStunned) 
                {
                    fighter.TickEffects();
                    continue;
                }
                fighter.TickEffects();
                if (fighter != null && fighter.currentAction != null)
                {
                    if (fighter.currentAction.targets == null)
                    {
                        continue;
                    } else
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
                        CoroutineWithData act = new CoroutineWithData(this, this.ProcessAction(action));
                        while (!act.finished)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                        CoroutineWithData moveBack = new CoroutineWithData(this, fighter.MoveToBattlePosition());
                        while (!moveBack.finished)
                        {
                            yield return new WaitForEndOfFrame();
                        }
                        yield return new WaitForSeconds(1);
                    }
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
    public void SetActionIcon(int lastAction, FighterAction a)
    {
        actionIcon.enabled = true;
        actionIconModifier.enabled = true;
        switch (lastAction)
        {
            case (int)KeyCode.W:
                actionIcon.sprite = actionIcons[3];
                break;
            case (int)KeyCode.D:
                actionIcon.sprite = actionIcons[2];
                break;
            case (int)KeyCode.S:
                actionIcon.enabled = false;
                break;
            case (int)KeyCode.A:
                actionIcon.sprite = actionIcons[4];
                break;
        }
        if(a.name.Contains("Buff") || a.name.Contains("Bolster") || a.name.Contains("Mark"))
        {
            actionIconModifier.sprite = actionIcons[1];
        } else if (a.name.Contains("Weak") || a.name.Contains("Vulnerable") || a.name.Contains("Taunt"))
        {
            actionIconModifier.sprite = actionIcons[0];
        } else
        {
            actionIconModifier.enabled = false;
        }
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
        DestroyAllShadows();
        expectedDamageTaken = 0;
        expectedVirtueGain = 0;
        shadows = new GameObject[allFighters.BeingDatas.Count];
        shadowScripts = new FighterShadow[allFighters.BeingDatas.Count];
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            Vector3 spawnLoc = allFighters.BeingDatas[i].gameObject.transform.position + new Vector3(allFighters.BeingDatas[i].gameObject.transform.position.x > 0 ? -6f : 6f, 0,0);
            Fighter origin = allFighters.BeingDatas[i].gameObject.GetComponent<Fighter>();
            shadows[i] = visualEffectMaster.InstantiateVisualSprite(Resources.Load("Prefabs/Shadow"),
                spawnLoc,
                allFighters.BeingDatas[i].gameObject.transform.rotation,
                allFighters.BeingDatas[i].gameObject.transform);
            shadowScripts[i] = shadows[i].GetComponent<FighterShadow>();
            shadows[i].GetComponent<SpriteRenderer>().enabled = false;
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
    private void SimulateShadowsButton()
    {
        playingShadows = false;
        StartCoroutine(SimulateShadowActions());
    }
    public IEnumerator SimulateShadowActions()
    {
        SimulateBattle();
        playingShadows = true;
        ToggleShadowSprites();
        for (int i = 0; i < shadowScripts.Length; i++)
        {
            if (shadowScripts[i].currentAction != null && shadowScripts[i].currentAction.targets != null)
            {
                Debug.Log(shadowScripts[i].name);
                CoroutineWithData sim = new CoroutineWithData(shadowScripts[i], shadowScripts[i].PlayAnimations());
                while (!sim.finished && playingShadows)
                {
                    yield return new WaitForEndOfFrame();
                }
                if (!playingShadows)
                {
                    for (int j = 0; j < shadowScripts.Length; j++) // put everyone back
                    {
                        shadowScripts[j].gameObject.transform.position = shadowScripts[j].battlePosition;
                    }
                    break;
                }
            }
        }
        ToggleShadowSprites();
        playingShadows = false;
        yield return true;
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
    private void ToggleShadowSprites()
    {
        for (int i = 0; i < shadowScripts.Length; i++)
        {
            SpriteRenderer sr = shadowScripts[i].GetComponent<SpriteRenderer>();
            sr.enabled = !sr.enabled;
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
    public void UpdateVirtueBar()
    {
        //move needle just a bit
    }
}