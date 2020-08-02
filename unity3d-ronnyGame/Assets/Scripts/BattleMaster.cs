using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleMaster : Kami
{
    private bool turn;
    private bool isBattle = false;
    private int enemyID { get; set; }
    private int turnCounter;
    private List<Action> aiActions = new List<Action>();
    public ListBeingData allFighters = new ListBeingData();
    public ListBeingData partyMembers = new ListBeingData();
    public ListBeingData enemyMembers = new ListBeingData();

    private string worldSceneName;
    private string battleSceneName;

    public void AddFighter(BeingData being)
    {
        allFighters.BeingDatas.Add(being);
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
            StartCoroutine("EndBattle", false);
            //load save data because you died
            return;
        } else  if (enemyMembers.BeingDatas.Count == 0)
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
    private static int CompareActionsByOriginatorTag(Action x, Action y)
    {
        if (x.originator.tag == "Party")
        {
            return -1;
        } else if (x.originator.tag == y.originator.tag)
        {
            return 0;
        } else
        {
            return 1;
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
            yield return new WaitForSeconds(2);
            //victory UI (rewards?) wait on click to load back to normal scene
            bool anyKey = false;
            Debug.Log("Press anykey please");
            while (!anyKey)
            {
                if (Input.anyKey)
                {
                    anyKey = true;
                    this.DestroyAllFighters();
                    this.RemoveMemberReferenceInGameMasterByID(this.enemyID); //destroy the enemy reference before we load back in
                    gameMaster.isSceneChanging = true;
                    this.aiActions = new List<Action>();
                    this.allFighters = new ListBeingData();
                    this.partyMembers = new ListBeingData();
                    this.enemyMembers = new ListBeingData();
                    this.isBattle = false;
                    yield return new WaitForSeconds(2); // have to wait to destroy every object before moving to the next scene
                    sceneMaster.ChangeScene(worldSceneName);
                }
                yield return new WaitForEndOfFrame();
            }
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

        for (int i = 0; i < allFighters.BeingDatas.Count; i ++)
        {
            if (allFighters.BeingDatas[i].gameObject.tag != "Player")
            {
                actions.Add(allFighters.BeingDatas[i].gameObject.GetComponent<Fighter>().GetIntention(allFighters));
            }
        }
        actions.Sort(CompareActionsByOriginatorTag);
        return actions;
    }
    private GameObject GetPlayerObject()
    {
        GameObject player = null;
        for(int i = 0; i < allFighters.BeingDatas.Count; i++)
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
        for(int i = 0; i < partyMembers.BeingDatas.Count; i++)
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
        this.InitializeFighters();
        this.MoveCameraTo(1.4f, 4, -6);
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
        cam.transform.position += new Vector3(x,y,z);
    }
    private void NewTurn()
    {
        aiActions = new List<Action>();
        aiActions = this.GetIntentions();
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
        bool decided = false;

        GameObject player = this.GetPlayerObject();
        Fighter fighter = player.GetComponent<Fighter>();
        fighter.ApplyEffects();
        fighter.RecalculateActions();
        GameObject target = null;
        Action action = null;

        while (decided == false)
        {
            
            target = fighter.ChooseTarget(allFighters);
            if (target != null)
            {
                action = fighter.ChooseAction(target);
            }
            if (Input.GetKey(KeyCode.Return) && action != null)
            {
                decided = true;
                this.turnCounter++;
                this.PlayAnimation(action.animation);
                yield return new WaitForSeconds(action.duration);
                this.ProcessAction(action);
                turn = false;
                StartCoroutine("ProcessAIActions");
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
    private void ProcessAction(Action action)
    {
        if (action.target != null)
        {
            action.Execute();
        }
    }
    public IEnumerator ProcessAIActions()
    {
        for (int i = 0; i < aiActions.Count; i++)
        {
            if (aiActions[i].originator != null)
            {
                Action action = aiActions[i];
                this.PlayAnimation(action.animation);
                yield return new WaitForSeconds(action.duration);
                this.ProcessAction(action);
            }
        }
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
    public void SetEnemyID(int ID)
    {
        this.enemyID = ID;
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
}