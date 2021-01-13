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
public class SoundMasterJson
{
    public float masterVolume;
    public float[] volumeTypes = new float[3];
}
public class DataMaster : Kami
{
    private bool loadFromFile;
    public string levelName;
    private static string KamiDataPath = "/Resources/Kami/Kami_Data.txt";
    public void Save()
    {
        if (sceneMaster.GetCurrentSceneName() == "Main_Menu")
        {
            return;
        }
        DataMasterJson dataMasterJson = new DataMasterJson(true); //Since DataMaster is a very individual class, it is necessary to construct its Json class
        dataMasterJson.levelName = sceneMaster.GetCurrentSceneName();
        gameMaster.UpdateAllBeingsInList();
        string dataMasterData = JsonUtility.ToJson(dataMasterJson);
        string gameMasterData = gameMaster.GetMasterData();
        string sceneMasterData = sceneMaster.GetMasterData();
        string soundMasterData = soundMaster.GetMasterData();
        if (!Directory.Exists(Application.dataPath + KamiDataPath) && !File.Exists(Application.dataPath + KamiDataPath))
        {
            File.CreateText(Application.dataPath + KamiDataPath);
        }
        StreamWriter streamWriter = new StreamWriter(Application.dataPath + KamiDataPath);
        streamWriter.WriteLine(dataMasterData);
        streamWriter.WriteLine(gameMasterData);
        streamWriter.WriteLine(sceneMasterData);
        streamWriter.WriteLine(soundMasterData);
        streamWriter.Close();
        return;
    }
    public void LoadDataFromFile()
    {
        if (!File.Exists(Application.dataPath + KamiDataPath))
        {
            Debug.LogError("No File Found");
            return;
        }
        StreamReader reader = new StreamReader(Application.dataPath + KamiDataPath);
        if (reader.Peek() != 1)
        {
            Debug.Log("Empty file");
        }
        DataMasterJson dataMasterJson = JsonUtility.FromJson<DataMasterJson>(reader.ReadLine());
        GameMasterJson gameMasterJson = JsonUtility.FromJson<GameMasterJson>(reader.ReadLine());
        SceneMasterJson sceneMasterJson = JsonUtility.FromJson<SceneMasterJson>(reader.ReadLine());
        SoundMasterJson soundMasterJson = JsonUtility.FromJson<SoundMasterJson>(reader.ReadLine());
        reader.Close();
        if (dataMasterJson != null && gameMasterJson != null && sceneMasterJson != null)
        {
            gameMaster.DestroyAllBeings();
            gameMaster.InjectData(gameMasterJson);
            sceneMaster.InjectData(sceneMasterJson);
            soundMaster.InjectData(soundMasterJson);
        }
    }
}
