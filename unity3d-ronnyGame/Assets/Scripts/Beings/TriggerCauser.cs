using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCauserJson
{
    public string causerName, targetName, triggerName;
}
public class TriggerCauser : Being
{
    public string causerName, targetName, triggerName;
    private GameObject causer;
    public GameObject target;
    public BoxCollider triggerBox;
    private Trigger trigger;

    private GameObject FindTarget()
    {
        if (targetName == null)
        {
            return null;
        }
        int duplicateCheck = 0;
        for (int i = 0; i < Kami.transform.childCount; i++)
        {
            GameObject child = Kami.transform.GetChild(i).gameObject;
            if (child.name == targetName)
            {
                target = child;
                duplicateCheck++;
            }
        }
        if (duplicateCheck > 1)
        {
            return null;
        }
        else
        {
            return target;
        }
    }
    public IEnumerator DeInitializeTrigger()
    {
        CoroutineWithData trig = new CoroutineWithData(this, trigger.EndTrigger(target));
        while (!trig.finished)
        {
            yield return new WaitForEndOfFrame();
        }
        CoroutineWithData move = new CoroutineWithData(this, MoveCameraBack());
        while (!move.finished)
        {
            yield return new WaitForEndOfFrame();
        }
        
        yield return null;
    }
    public IEnumerator InitializeTrigger()
    {
        CoroutineWithData move = new CoroutineWithData(this, MoveCameraBetweenPoints(causer.transform.position, target.transform.position));
        while (!move.finished)
        {
            yield return new WaitForEndOfFrame();
        }
        CoroutineWithData trig = new CoroutineWithData(this, trigger.StartTrigger(target));
        while (!trig.finished)
        {
            yield return new WaitForEndOfFrame();
        }
        StartCoroutine(DeInitializeTrigger());
        
        yield return null;
    }
    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {
            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);

            if (being.jsonData != null && being.jsonData != string.Empty)
            {
                TriggerCauserJson causer = JsonUtility.FromJson<TriggerCauserJson>(being.jsonData);

                this.causerName = causer.causerName;
                this.targetName = causer.targetName;
                this.triggerName = causer.triggerName;
            }
            this.ID = being.objectID;
            this.beingData = jsonData;
        }
        return;
    }
    public IEnumerator MoveCameraBetweenPoints(Vector3 x, Vector3 y)
    {
        Camera cam = Camera.main;
        Vector3 midPoint = (y - x);
        Vector3 destination = cam.transform.position + new Vector3(midPoint.x / 2, 0, midPoint.z / 2);
        while (cam.transform.position != destination)
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, destination, .1f);
            yield return new WaitForEndOfFrame();
        }
        //yield return new WaitForSeconds(2);
        yield return true;
    }
    public IEnumerator MoveCameraBack()
    {
        Camera cam = Camera.main;
        Vector3 destination = new Vector3(0, 7, -12.38f);
        while (cam.transform.localPosition != destination)
        {
            cam.transform.localPosition = Vector3.MoveTowards(cam.transform.localPosition, destination, .0125f);
            yield return new WaitForEndOfFrame();
        }
        yield return true;
    }
    new public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == causerName)
        {
            causer = other.gameObject;
            target = FindTarget();
            if (target && trigger)
            {
                StartCoroutine(InitializeTrigger());
            }
        }
    }
    private new void Start()
    {
        base.Start();
        AttachTrigger();
    }
    public override string UpdateBeingJsonData()
    {
        TriggerCauserJson causer = new TriggerCauserJson();
        causer.causerName = this.causerName;
        causer.targetName = this.targetName;
        causer.triggerName = this.triggerName;

        return JsonUtility.ToJson(causer);
    }
    private void AttachTrigger()
    {
        if (triggerName != null)
        {
            switch (triggerName)
            {
                case "JoeyBurnFence":
                    trigger = this.gameObject.AddComponent<JoeyBurnFence>();
                    break;
                default:
                    Debug.LogError($"Can't locate trigger {triggerName}!");
                    break;
            }
        }
    }
}
