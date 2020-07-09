using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Action
{
    public GameObject originator, target;
    public delegate void Affect();
    public Affect method;
    public int duration;
}

public class BattleMaster : Kami
{
    private bool turn;
    public ListBeingData allFighters = new ListBeingData();
    public ListBeingData partyMembers = new ListBeingData();
    public ListBeingData enemyMembers = new ListBeingData();

    private string worldSceneName;
    private string battleSceneName;

    public void Update()
    {
        if (turn)
        {
            //gameMaster.GetPlayerGameObject.this.getComponent<Fighter>().actions
            this.AllyActions();
        }
    }
    public void AddFighter(BeingData being)
    {
        allFighters.BeingDatas.Add(being);
    }
    private void AllyActions()
    {
        for (int i = 0; i < partyMembers.BeingDatas.Count; i++)
        {
            if (partyMembers.BeingDatas[i].gameObject != gameMaster.GetPlayerGameObject())
            {
                //do actions to enemy members or to party members fighter.action(ListBeginData partyMembers, ListBeingData enemyMembers)
            }
        }
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
            //load save data because you died
            return;
        } else  if (enemyMembers.BeingDatas.Count == 0)
        {
            //maybe track rewards? anyways load the initial scene
            return;
        } else
        {
            return;
        } 
    }
    private void EnemyActions() 
    {
        for(int i = 0; i < enemyMembers.BeingDatas.Count; i++)
        {
            //do actions against party members fighter.action(partyMembers, enemyMembers)
        }
    }
    public void FillMembers(ListBeingData ronnyParty, ListBeingData enemyParty) 
    {
        partyMembers = ronnyParty;
        enemyMembers = enemyParty;
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
        this.turn = true;
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

    }
    private void MoveCameraTo(float x, float y, float z)
    {
        Camera cam = Camera.main;
        cam.transform.position += new Vector3(x,y,z);
    }
    private void DestroyAllFighters()
    {
        for(int i = 0; i < allFighters.BeingDatas.Count; i++)
        {
            Being being = allFighters.BeingDatas[i].gameObject.GetComponent<Being>();
            being.DestroyBeing();
            allFighters.BeingDatas.Remove(allFighters.BeingDatas[i]);
        }
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
}