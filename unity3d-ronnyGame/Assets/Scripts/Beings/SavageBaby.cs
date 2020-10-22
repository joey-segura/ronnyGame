﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavageBabyJson
{
    public float health, virtueValue;
}
public class SavageBaby : Enemy
{
    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {

            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);
            if (being.jsonData != null && being.jsonData != string.Empty)
            {
                SavageBabyJson savageBaby = JsonUtility.FromJson<SavageBabyJson>(being.jsonData);
                this.health = savageBaby.health;
                this.virtueValue = savageBaby.virtueValue;
            }
            else
            {
                this.health = 8;
                this.virtueValue = 2;
            }
            this.ID = being.objectID;
            this.beingData = jsonData;
        }
        return;
    }
    public override void DeathTrigger(bool shadow)
    {
        this.Sacrifice(shadow);
    }
    public override void RecalculateActions()
    {
        this.actionList = new List<FighterAction>();
    }
    private void Sacrifice(bool shadow)
    {
        if (battleMasterScript == null)
        {
            battleMasterScript = this.GetComponentInParent<BattleMaster>();
        }
        FighterAction sacrifice = new BuffAttack(3, 3, 4, null);
        GameObject joey = null;
        if (joey = battleMasterScript.GetAllyObject())
        {
            if (shadow)
            {
                for (int i = 0; i < joey.transform.childCount; i++)
                {
                    if (joey.transform.GetChild(i).name.Contains("Shadow"))
                    {
                        joey = joey.transform.GetChild(i).gameObject;
                    }
                }
            }
            sacrifice.targets = new GameObject[] { joey };
            sacrifice.originator = this.gameObject;
            //battleMasterScript.StartCoroutine(sacrifice.Execute());

            battleMasterScript.StartCoroutine(battleMasterScript.ProcessAction(sacrifice));
        }
    }
    public override FighterAction TurnAction(ListBeingData allFighters)
    {
        return null;
    }
    public override string UpdateBeingJsonData()
    {
        SavageBabyJson savageBaby = new SavageBabyJson();
        savageBaby.health = this.health;
        savageBaby.virtueValue = this.virtueValue;
        return JsonUtility.ToJson(savageBaby);
    }
}
