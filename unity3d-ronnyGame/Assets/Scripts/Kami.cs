using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Kami : MonoBehaviour
{
    protected static BattleMaster battleMaster;
    protected static GameMaster gameMaster;
    protected static SceneMaster sceneMaster;

    private string battleMasterData;
    private string gameMasterData;
    private string sceneMasterData;

    private static string KamiDataPath = "Assets/Resources/Kami/Kami_Data.txt";

    protected void Awake()
    {
        this.LoadClasses();
    }

    protected void LoadClasses()
    {
        battleMaster = this.GetComponent<BattleMaster>();
        gameMaster = this.GetComponent<GameMaster>();
        sceneMaster = this.GetComponent<SceneMaster>();
    }
    public virtual string GetMasterData()
    {
        Debug.Log(JsonUtility.ToJson(this));
        return JsonUtility.ToJson(this);
    }
    public void SaveMasterData()
    {
        this.battleMasterData = battleMaster.GetMasterData();
        this.gameMasterData = gameMaster.GetMasterData();
        this.sceneMasterData = sceneMaster.GetMasterData();
        File.WriteAllText(KamiDataPath, string.Empty);
        StreamWriter streamWriter = new StreamWriter(KamiDataPath);
        streamWriter.WriteLine(this.battleMasterData);
        streamWriter.WriteLine(this.gameMasterData);
        streamWriter.WriteLine(this.sceneMasterData);
        streamWriter.Close();
        return;
    }
    public void SetMasterData(string _battleMasterData, string _gameMasterData, string _sceneMasterData)
    {
        this.battleMasterData = _battleMasterData;
        this.gameMasterData = _gameMasterData;
        this.sceneMasterData = _sceneMasterData;
    }

}
