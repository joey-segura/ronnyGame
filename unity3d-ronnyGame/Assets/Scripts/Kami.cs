﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kami : MonoBehaviour
{
    protected static BattleMaster battleMaster;
    protected static GameMaster gameMaster;
    protected static SceneMaster sceneMaster;

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
}
