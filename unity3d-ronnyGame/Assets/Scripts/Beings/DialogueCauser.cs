using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueCauserJson
{
    public string[] messages;
    public string targetName, triggerName;
}
public class DialogueCauser : Being
{
    public string[] messages;
    public string targetName, triggerName;
    public float chatSpeed = 1f;
    public GameObject target;
    public BoxCollider triggerBox;
    public Image headImage;
    public Text text;
    public Sprite ronnyHead, joeyHead;
    private Ronny ronny;
    private Trigger trigger;

    public IEnumerator BeginDialogue()
    {
        foreach (string message in messages)
        {
            SetHeadImage(message);
            char[] chars = message.ToCharArray();
            text.text = string.Empty;
            chatSpeed = 1f; //default value
            for (int i = 0; i < chars.Length; i++)
            {
                text.text += chars[i];
                if (!char.IsWhiteSpace(chars[i]))
                {
                    yield return new WaitForSeconds(.1f / chatSpeed);
                } else
                {
                    yield return new WaitForSeconds(.5f / chatSpeed);
                }
            }
            bool wait = false;
            float waitTime = 0;
            while (!wait)
            {
                if (Input.GetKey(KeyCode.Mouse0) || waitTime > 3)
                {
                    wait = true;
                }
                waitTime += Time.deltaTime; 
                yield return new WaitForEndOfFrame();
            }
        }
        StartCoroutine(DeinitializeDialogue());
        yield return null;
    }
    public IEnumerator DeinitializeDialogue ()
    {
        if (trigger && target)
        {
            StartCoroutine(trigger.EndTrigger(target));
            CoroutineWithData cd = new CoroutineWithData(this, MoveCameraBack(ronny.transform.position));
            while (!cd.finished)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        ronny.ToggleMovementAndCamera();
        gameMasterScript.RemoveBeingFromList(ID);
        DestroyBeing();
        yield return null;
    }
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
        } else
        {
            return target;
        }
    }
    public void InitializeDialogue(GameObject player)
    {
        if ((target = FindTarget()) != null)
        {
            StartCoroutine(MoveCameraBetweenPoints(player.transform.position, target.transform.position));
            //how to play target animations? (have this class have index number for animations?)
            //stop target from moving (being.initializeDialogue?)
        }
        if (trigger && target)
        {
            StartCoroutine(trigger.StartTrigger(target));
        }
        ronny = player.GetComponent<Ronny>();
        ronny.ToggleMovementAndCamera();
        this.ToggleCanvas();
        StartCoroutine(BeginDialogue());
    }
    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {
            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);

            if (being.jsonData != null && being.jsonData != string.Empty)
            {
                DialogueCauserJson causer = JsonUtility.FromJson<DialogueCauserJson>(being.jsonData);

                this.messages = causer.messages;
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
    public IEnumerator MoveCameraBack(Vector3 ronnyPos)
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
        if (other.gameObject.tag == "Player")
        {
            InitializeDialogue(other.gameObject);
        }
    }
    public void SetHeadImage(string message)
    {
        if (message.Contains("Ronny:"))
        {
            headImage.sprite = ronnyHead;
        } else if (message.Contains("Joey:"))
        {
            headImage.sprite = joeyHead;
        }
    }
    private void Start()
    {
        AttachTrigger();
    }
    private new void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            this.chatSpeed = 4;
        }
    }
    public override string UpdateBeingJsonData()
    {
        DialogueCauserJson causer = new DialogueCauserJson();
        causer.messages = this.messages;
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
                case "JoeyJoinParty":
                    trigger = this.gameObject.AddComponent<JoeyJoinParty>();
                    break;
                default:
                    break;
            }
        }
    }
}
