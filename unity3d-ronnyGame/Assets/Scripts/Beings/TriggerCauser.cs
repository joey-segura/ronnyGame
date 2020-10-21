using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCauserJson
{
    public string causerName, targetName, triggerName;
    public bool togglePlayerMovement;
}
public class TriggerCauser : Being
{
    public string causerName, targetName, triggerName;
    private GameObject causer;
    public GameObject target;
    public BoxCollider triggerBox;
    private Trigger trigger;
    public bool togglePlayerMovement = false;

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
        if (togglePlayerMovement)
        {
            gameMasterScript.GetPlayerGameObject().GetComponent<Ronny>().ToggleMovementAndCamera();
        }
        DestroyBeing();
        yield return null;
    }
    public IEnumerator InitializeTrigger()
    {
        if (togglePlayerMovement)
        {
            gameMasterScript.GetPlayerGameObject().GetComponent<Ronny>().ToggleMovementAndCamera();
        }
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
                this.togglePlayerMovement = causer.togglePlayerMovement;
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
        while (Vector3.Distance(cam.transform.position, destination) > .5f)
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, destination, .1f);
            yield return new WaitForEndOfFrame();
        }
        cam.transform.position = destination;
        //yield return new WaitForSeconds(2);
        yield return true;
    }
    public IEnumerator MoveCameraBack()
    {
        Camera cam = Camera.main;
        if (!cam.transform.IsChildOf(gameMasterScript.GetPlayerGameObject().transform))
        {
            yield return true;
        }
        else
        {
            Vector3 destination = new Vector3(0, 7, -12.38f);
            float timeElapsed = 0;
            while (Vector3.Distance(cam.transform.localPosition, destination) > .25f)
            {
                cam.transform.localPosition = Vector3.MoveTowards(cam.transform.localPosition, destination, .05f);
                timeElapsed += Time.deltaTime;
                if (timeElapsed > 5)
                {
                    Debug.LogError("took longer than 5 seconds to move the camera back");
                    cam.transform.localPosition = destination;
                    break;
                }
                yield return new WaitForEndOfFrame();
            }
            cam.transform.localPosition = destination;
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
    private void Start()
    {
        AttachTrigger();
    }
    public override string UpdateBeingJsonData()
    {
        TriggerCauserJson causer = new TriggerCauserJson();
        causer.causerName = this.causerName;
        causer.targetName = this.targetName;
        causer.triggerName = this.triggerName;
        causer.togglePlayerMovement = this.togglePlayerMovement;

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
