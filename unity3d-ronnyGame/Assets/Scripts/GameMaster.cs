using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class BeingData
{
    public GameObject gameObject;
    public int objectID;
    public string jsonData;
    public string prefabName;
    public Vector3 location;
    public Quaternion angle;
    public Vector3 scale;
}
[System.Serializable]
public class ListBeingData
{
    public List<BeingData> BeingDatas;
    public ListBeingData()
    {
        BeingDatas = new List<BeingData>();
    }
}
public class GameMaster : Kami
{
    private GameObject PLAYER;

    public ListBeingData GameMasterBeingDataList = new ListBeingData();

    public bool isSceneChanging = false;
    
    public int objectIDCounter;

    private string sceneJsonData;
    private string LEVELDATAPATH = "Assets/Resources/Level_Data/";
    private string PREFABPATH = "Assets/Resources/Prefabs/";

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void InstantiateObject (string jsonData)
    {
        BeingData beingData = JsonUtility.FromJson<BeingData>(jsonData);
        GameObject entity = Instantiate(Resources.Load(("Prefabs/" + beingData.prefabName), typeof(GameObject)), beingData.location, beingData.angle, this.gameObject.transform) as GameObject;
        
        if (beingData.scale != new Vector3(0,0,0))
        {
            entity.transform.localScale = beingData.scale;
        }

        if(GameMasterBeingDataList.BeingDatas != null && GetBeingDataByID(beingData.objectID) == null) //checks to see if the object being instantiated is already populated in list
        {
            entity.transform.name = beingData.prefabName;
            beingData.gameObject = entity.gameObject;
            beingData.objectID = objectIDCounter;
            objectIDCounter++;
            this.AddBeingToList(beingData);
        } else if (GetBeingDataByID(beingData.objectID) != null)
        {
            entity.transform.name = beingData.prefabName;
            beingData.gameObject = entity.gameObject;
            this.UpdateBeingInList(beingData);
        }

        Being being = entity.GetComponent<Being>();
        if (being != null)
        {
            being.InjectData(JsonUtility.ToJson(beingData));

            if (entity.tag == "Player")
            {
                PLAYER = entity;
            }
        }
    }
    public void LoadGameMasterSceneData()
    {
        ListBeingData list = new ListBeingData();
        list = GameMasterBeingDataList;
        for(int i = 0; i < list.BeingDatas.Count; i++)
        {
            InstantiateObject(JsonUtility.ToJson(list.BeingDatas[i]));
        }
    }
    public void LoadInitialSceneData(string sceneName)
    {
        string path = LEVELDATAPATH + sceneName + ".txt";

        //Read the text directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            InstantiateObject(line);
        }
        reader.Close();
    }
    private void AddBeingToList(BeingData being)
    {
        GameMasterBeingDataList.BeingDatas.Add(being);
    }
    public void DestroyAllBeings()
    {
        foreach (BeingData beingData in GameMasterBeingDataList.BeingDatas)
        {
            if (beingData.gameObject != null)
            {
                Destroy(beingData.gameObject);
            }
        }
    }
    public void UpdateAllBeingsInList() //this will be called before scene change
    {
        for(int x = 0; x < this.gameObject.transform.childCount; x++)
        {
            GameObject child = this.gameObject.transform.GetChild(x).gameObject;
            Being being = child.GetComponent<Being>();
            BeingData beingData = JsonUtility.FromJson<BeingData> (being.CompactBeingDataIntoJson());
            UpdateBeingInList(beingData);
        }
    }
    public void UpdateBeingInList(BeingData beingData)
    {
        for (int i = 0; i < GameMasterBeingDataList.BeingDatas.Count; i++)
        {
            if (beingData.objectID == GameMasterBeingDataList.BeingDatas[i].objectID)
            {
                GameMasterBeingDataList.BeingDatas[i] = beingData;
            }
        }
    }
    public void RemoveBeingFromList(int ID, BeingData being = null)
    {
        if (being != null)
        {
            GameMasterBeingDataList.BeingDatas.Remove(being);
        } else
        {
            GameMasterBeingDataList.BeingDatas.Remove(GetBeingDataByID(ID));
        }
    }
    public BeingData GetBeingDataByID(int ID)
    {
        foreach (BeingData beingData in GameMasterBeingDataList.BeingDatas)
        {
           if (beingData.objectID == ID)
            {
                return beingData;
            }
        }
        return null;
    }
    public GameObject GetPlayerGameObject()
    {
        if (PLAYER != null)
        {
           return PLAYER;
        } else
        {
           return this.gameObject;
        }
    }
}

