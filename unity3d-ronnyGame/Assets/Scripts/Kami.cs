using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Kami : MonoBehaviour
{
    protected static BattleMaster battleMaster;
    protected static GameMaster gameMaster;
    protected static SceneMaster sceneMaster;
    protected static UIMaster uiMaster;
    protected static DataMaster dataMaster;

    protected void Awake()
    {
        this.LoadClasses();
    }
    protected void LoadClasses()
    {
        battleMaster = this.GetComponent<BattleMaster>();
        gameMaster = this.GetComponent<GameMaster>();
        sceneMaster = this.GetComponent<SceneMaster>();
        uiMaster = this.GetComponent<UIMaster>();
        dataMaster = this.GetComponent<DataMaster>();
    }
    public virtual string GetMasterData()
    {
        return JsonUtility.ToJson(this);
    }
}
