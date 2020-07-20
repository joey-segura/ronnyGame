using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleMaster : Kami
{
    private bool turn;
    private bool isBattle = false;
    public ListBeingData allFighters = new ListBeingData();
    public ListBeingData partyMembers = new ListBeingData();
    public ListBeingData enemyMembers = new ListBeingData();

    private string worldSceneName;
    private string battleSceneName;

    public void AddFighter(BeingData being)
    {
        allFighters.BeingDatas.Add(being);
    }
    private IEnumerator AllyActions()
    {
        for (int i = 0; i < partyMembers.BeingDatas.Count; i++)
        {
            if (partyMembers.BeingDatas[i].gameObject != gameMaster.GetPlayerGameObject())
            {
                //do actions to enemy members or to party members fighter.action(ListBeginData partyMembers, ListBeingData enemyMembers)
            }
        }
        StartCoroutine("EnemyTurn");
        yield return null;
    }
    private void AssignScenes()
    {
        worldSceneName = sceneMaster.GetCurrentSceneName();
        battleSceneName = sceneMaster.GetBattleSceneName(worldSceneName);
    }
    public void BattleEndCheck()
    {
        if(partyMembers.BeingDatas.Count == 0)
        {
            isBattle = false;
            //load save data because you died
            return;
        } else  if (enemyMembers.BeingDatas.Count == 0)
        {
            isBattle = false;
            //maybe track rewards? anyways load the initial scene
            return;
        } else
        {
            return;
        } 
    }
    private void DestroyAllFighters()
    {
        for (int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            Being being = allFighters.BeingDatas[i].gameObject.GetComponent<Being>();
            being.DestroyBeing();
            allFighters.BeingDatas.Remove(allFighters.BeingDatas[i]);
        }
    }
    private IEnumerator EnemyTurn() 
    {
        for(int i = 0; i < enemyMembers.BeingDatas.Count; i++)
        {
            GameObject self = enemyMembers.BeingDatas[i].gameObject;
            if (self != null)
            {
                Fighter fighter = self.GetComponent<Fighter>();
                GameObject target = fighter.ChooseTarget(allFighters);
                Action action = fighter.ChooseAction(target);
                //fighter.animation(action) ?
                yield return new WaitForSeconds(action.duration);
                this.ProcessAction(action);
            }
        }
        StartCoroutine("PlayerAction");
        this.turn = true;
        yield return null;
    }
    public void FillMembers(ListBeingData ronnyParty, ListBeingData enemyParty) 
    {
        partyMembers = ronnyParty;
        enemyMembers = enemyParty;
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
        this.InitializeFighters();
        this.MoveCameraTo(1.4f, 4, -6);
        StartCoroutine("PlayerAction");
        this.turn = true;
        this.isBattle = true;
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
        this.UpdateBothPartiesFromAllFigthers();
    }
    private void MoveCameraTo(float x, float y, float z)
    {
        Camera cam = Camera.main;
        cam.transform.position += new Vector3(x,y,z);
    }
    private IEnumerator PlayerAction()
    {
        bool decided = false;

        yield return new WaitForSeconds(2); //waiting for stuff to initialize like fighter.ChooseAction(target);

        GameObject player = this.GetPlayerObject();
        Fighter fighter = player.GetComponent<Fighter>();
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
                //fighter.animation(action) ?
                yield return new WaitForSeconds(action.duration);
                this.ProcessAction(action);
                turn = false;
                StartCoroutine("AllyActions");
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
    private void ProcessAction(Action action)
    {
        action.Execute();
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
            if(partyMembers.BeingDatas[i].objectID == ID)
            {
                partyMembers.BeingDatas.Remove(partyMembers.BeingDatas[i]);
                return;
            }
        }
        for (int i = 0; i < partyMembers.BeingDatas.Count; i++)
        {
            if (enemyMembers.BeingDatas[i].objectID == ID)
            {
                enemyMembers.BeingDatas.Remove(enemyMembers.BeingDatas[i]);
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
}