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
        entity.transform.name = beingData.prefabName;

        beingData.gameObject = entity.gameObject;
        beingData.objectID = objectIDCounter;
        objectIDCounter++;

        if(GameMasterBeingDataList.BeingDatas != null)
        {
            this.AddBeingToList(beingData);
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
        foreach (BeingData beingData in GameMasterBeingDataList.BeingDatas)
        {
            Quaternion angle = new Quaternion(0, 0, 0, 0);
            InstantiateObject(JsonUtility.ToJson(beingData));
        }
    }
    public void LoadInitialSceneData(string sceneName)
    {
        LoadGameMasterSceneData();

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
    public void UpdateAllBeingsInList() //this will be called before scene change
    {
        for(int x = 0; x < this.gameObject.transform.childCount; x++)
        {
            GameObject child = this.gameObject.transform.GetChild(x).gameObject;
            Being being = child.GetComponent<Being>();
            UpdateBeingInList(being.ID, being.CompactBeingDataIntoJson());
        }
    }
    public void UpdateBeingInList(int ID, string jsonData)
    {
        BeingData being = GetBeingDataByID(ID);
        RemoveBeingFromList(ID, being);
        being.jsonData = jsonData;
        AddBeingToList(being);
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

