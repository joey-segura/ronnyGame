using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relater : Being
{
    private int commonID;
    private GameObject relative;
    private Relater relativeComponent;

    private new void Awake()
    {
        Invoke("FindRelative", 1); // give it a second for all things to load in (necessary)
        base.Awake();
    }
    private void FindRelative()
    {
        ListBeingData list = gameMasterScript.GameMasterBeingDataList;
        for (int i = 0; i < list.BeingDatas.Count; i++)
        {
            Relater r = null;

            if ((r = list.BeingDatas[i].gameObject.GetComponent<Relater>()) != null && r.GetCommonID() == this.commonID)
            {
                relative = list.BeingDatas[i].gameObject;
                relativeComponent = r;
            }
        }
    }
    public int GetCommonID()
    {
        return this.commonID;
    }
    public override void Interact()
    {
        this.SendTrigger();
    }
    public void SendTrigger()
    {
        relativeComponent.Trigger();
    }
    public virtual void Trigger()
    {
        Debug.Log("This being was triggered to do something!");
    }
}
