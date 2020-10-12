using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
public class DataMasterJson
{
    public bool loadFromFile;
    public string levelName;
    private static string KamiDataPath;

    public DataMasterJson(bool _loadFromFile)
    {
        this.loadFromFile = _loadFromFile;
    } 
}
public class GameMasterJson
{
    private GameObject PLAYER;
    public ListBeingData GameMasterBeingDataList;
    public bool isSceneChanging, firstBattle;
    public int objectIDCounter;
    private string sceneJsonData;
    private string LEVELDATAPATH;
    private string PREFABPATH;
}
public class SceneMasterJson
{
    public bool[] levelLoaded;
    public bool newGame;
    public static string[] SCENENAMES;
    public string currentSceneName;
    public string lastSceneName;
}
public class DataMaster : Kami
{
    private bool loadFromFile;
    public string levelName;
    private static string KamiDataPath = "Assets/Resources/Kami/Kami_Data.txt";
    public void Save()
    {
        if (sceneMaster.GetCurrentSceneName() == "Main_Menu")
        {
            return;
        }
        DataMasterJson dataMasterJson = new DataMasterJson(true); //Since DataMaster is a very individual class, it is necessary to construct its Json class
        dataMasterJson.levelName = sceneMaster.GetCurrentSceneName();
        string dataMasterData = JsonUtility.ToJson(dataMasterJson);
        gameMaster.UpdateAllBeingsInList();
        string gameMasterData = gameMaster.GetMasterData();
        string sceneMasterData = sceneMaster.GetMasterData();
        File.WriteAllText(KamiDataPath, string.Empty);
        StreamWriter streamWriter = new StreamWriter(KamiDataPath);
        streamWriter.WriteLine(dataMasterData);
        streamWriter.WriteLine(gameMasterData);
        streamWriter.WriteLine(sceneMasterData);
        streamWriter.Close();
        return;
    }
    public void LoadDataFromFile()
    {
        StreamReader reader = new StreamReader(KamiDataPath);
        DataMasterJson dataMasterJson = JsonUtility.FromJson<DataMasterJson>(reader.ReadLine());
        GameMasterJson gameMasterJson = JsonUtility.FromJson<GameMasterJson>(reader.ReadLine());
        SceneMasterJson sceneMasterJson = JsonUtility.FromJson<SceneMasterJson>(reader.ReadLine());
        reader.Close();
        if (dataMasterJson != null && gameMasterJson != null && sceneMasterJson != null)
        {
            gameMaster.DestroyAllBeings();
            gameMaster.InjectData(gameMasterJson);
            sceneMaster.InjectData(sceneMasterJson);
        }
    }
    public string GetMasterJson(string name)
    {
        StreamReader reader = new StreamReader(KamiDataPath);
        string dataMaster = reader.ReadLine();
        string gameMaster = reader.ReadLine();
        string sceneMaster = reader.ReadLine();

        switch (name)
        {
            case "dataMaster":
                return dataMaster;
            case "gameMaster":
                return gameMaster;
            case "sceneMaster":
                return sceneMaster;
            default:
                return null;
        }
    }
}
