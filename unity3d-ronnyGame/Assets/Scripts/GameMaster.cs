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

    public void InstatiateObject (string name, Vector3 location, Quaternion angle, string jsonData)
    {
        GameObject entity = Instantiate(Resources.Load(("Prefabs/" + name), typeof(GameObject)), location, angle, this.gameObject.transform) as GameObject;
        entity.transform.name = name;

        //need to forawrd json data there should always be json sent to the object in all cases of instantiuation
        if (jsonData == null) //thinking about getting rid of this by throwing json in the instantiation files which would baiscally move the assignment of beingData to
        {
            BeingData beingData = new BeingData();
            beingData.prefabName = name;
            beingData.objectID = objectIDCounter;
            beingData.gameObject = entity.gameObject;
            this.objectIDCounter++;
            jsonData = JsonUtility.ToJson(beingData);
            if(GameMasterBeingDataList.BeingDatas != null)
            {
                this.AddBeingToList(beingData);
            }
        } 

        Being being = entity.GetComponent<Being>();
        if (being != null)
        {
            being.InjectData(jsonData);

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
            InstatiateObject(beingData.prefabName, beingData.gameObject.transform.position, angle, beingData.jsonData);
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
            string[] data = line.Split(',');
            string objectName = data[0];
            Vector3 location = new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]));
            Quaternion angle = new Quaternion(float.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6]), float.Parse(data[7]));

            InstatiateObject(objectName, location, angle, null);
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

