using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Kami : MonoBehaviour
{
    protected static BattleMaster battleMaster;
    protected static GameMaster gameMaster;
    protected static SceneMaster sceneMaster;
    protected static DataMaster dataMaster;
    protected static VisualEffectMaster visualEffectMaster;
    protected static SoundMaster soundMaster;

    protected void Awake()
    {
        this.LoadClasses();
    }
    protected void LoadClasses()
    {
        battleMaster = this.GetComponent<BattleMaster>();
        gameMaster = this.GetComponent<GameMaster>();
        sceneMaster = this.GetComponent<SceneMaster>();
        dataMaster = this.GetComponent<DataMaster>();
        visualEffectMaster = this.GetComponent<VisualEffectMaster>();
        soundMaster = this.GetComponent<SoundMaster>();
    }
    public virtual string GetMasterData()
    {
        return JsonUtility.ToJson(this);
    }
}
