using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class Being : MonoBehaviour
{
    // functions needed to extend this class (quick reference)
    // CompactBeingDataIntoJson() <- needs to return json of the class BeingData
    // InjectData(string jsonData) <- instantiates individual class data (jsonData should be in class format)
    // Interact() <- no return value

    public GameObject Kami;
    public Kami kami;
    public GameMaster gameMasterScript;
    public BattleMaster battleMasterScript;
    public SoundMaster soundMasterScript;
    public SoundBankMaster soundBankMaster;

    public float speed;

    private bool debugMode = false;
    public bool isStaticToCamera = false, interactable = false, inTransit = false;
    protected bool isHovering = false;

    public int ID { get; set; }

    public string beingData;

    private Camera cam;

    public Canvas canvas;

    protected void OnGUI()
    {
        if (debugMode && (this.GetComponent<Renderer>().isVisible || this.GetComponent<SpriteRenderer>().isVisible))
        {
            Vector3[] verts;
            if (this.gameObject.GetComponent<MeshFilter>() != null) //3D objects
            {
                Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
                verts = mesh.vertices;
            }
            else//2D
            {
                SpriteRenderer spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
                Vector3[] vert = new Vector3[spriteRenderer.sprite.vertices.Length];
                for (int i = 0; i < vert.Length; i++)
                {
                    vert[i] = new Vector3(spriteRenderer.sprite.vertices[i].x, spriteRenderer.sprite.vertices[i].y, 0);
                }
                verts = vert;
            }
            cam = Camera.main;
            Vector3 center = cam.WorldToScreenPoint(this.transform.position);
            float minX = center.x, maxX = center.x, minY = center.y, maxY = center.y;

            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 truePoint = transform.TransformPoint(verts[i]);
                Vector3 screenPoint = cam.WorldToScreenPoint(truePoint);
                if (screenPoint.x < minX)
                {
                    minX = screenPoint.x;
                }
                if (screenPoint.x > maxX)
                {
                    maxX = screenPoint.x;
                }
                if (screenPoint.y < minY)
                {
                    minY = screenPoint.y;
                }
                if (screenPoint.y > maxY)
                {
                    maxY = screenPoint.y;
                }
            }

            float width = maxX - minX;
            float height = maxY - minY;

            GUI.Box(new Rect(minX, (Screen.height - maxY), width, height), $"{this.gameObject.name}\n {ID.ToString()}");
        }
    }
    protected void Update()
    {
        if (interactable && Input.GetKeyDown(KeyCode.R)) //Interact Key (need to generalize in the future to allow remapping etc)
        {
            this.Interact();
        }
        if (this.isStaticToCamera)
        {
            this.transform.rotation = gameMasterScript.GetPlayerGameObject().transform.rotation;
        }
    }
    protected void Awake()
    {
        cam = Camera.main;
        Kami = this.gameObject.transform.parent.gameObject;
        gameMasterScript = Kami.GetComponent<GameMaster>();
        battleMasterScript = Kami.GetComponent<BattleMaster>();
        soundMasterScript = Kami.GetComponent<SoundMaster>();
        soundBankMaster = Kami.GetComponent<SoundBankMaster>();
        if (this.gameObject.GetComponentInChildren<Canvas>() != null)
        {
            
            this.canvas = this.gameObject.GetComponentInChildren<Canvas>();
            this.ToggleCanvas();
        }
    }
    public void ChangeTransparancy(float alpha = .7f)
    {
        this.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, alpha);
    }
    public IEnumerator ChaseObject(GameObject obj)
    {
        if (inTransit)
        {
            inTransit = false;
            yield return new WaitForEndOfFrame();
        }
        Vector3 loc = obj.transform.position;
        inTransit = true;
        while (this.transform.position != loc)
        {
            if (!inTransit || obj == null) //someone kicked us out
                break;
            loc = obj.transform.position;
            this.transform.position = Vector3.MoveTowards(this.transform.position, loc, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        inTransit = false;
        yield return true;
    }
    public virtual string CompactBeingDataIntoJson()
    {
        BeingData being = JsonUtility.FromJson<BeingData>(this.beingData);
        being.location = this.gameObject.transform.position;
        being.angle = this.gameObject.transform.rotation;
        being.scale = this.gameObject.transform.localScale;
        being.gameObject = this.gameObject;
        being.prefabName = this.gameObject.name;
        being.objectID = this.ID;

        being.jsonData = this.UpdateBeingJsonData();

        return JsonUtility.ToJson(being);
    }
    public void DestroyBeing()
    {
        Destroy(this.gameObject);
    }
    public virtual void InjectData(string jsonData)
    {
        //will be overriden by extended objects but left to be referenced by Being.InjectData()
        //need to convert jsonData into the class of beingData and set the object ID to that
        //need to convert beingData.jsonData (this is the being class specific) to its individual class
        //need to set values of that individual class E.G., for the sign class it assigns the 'message' string
    }
    protected IEnumerator InstantiateMessage(string[] message)
    {
        Text text = canvas.gameObject.GetComponentInChildren<Text>();
        for (int i = 0; i < message.Length; i++)
        {
            text.text = message[i];
            this.ToggleCanvas();
            yield return new WaitForSeconds(5);
            Debug.LogWarning("Arbitrary constant set for all instantiated UI objects");
            this.ToggleCanvas();
        }
        yield return null;
    }
    public virtual void Interact()
    {
        //will be overriden by extended objects but left to be referenced by Being.Interact()
    }
    public IEnumerator MoveToStaticLoc(Vector3 loc)
    {
        if (!inTransit)
        {
            inTransit = true;
            while (this.transform.position != loc)
            {
                if (!inTransit) //someone kicked us out
                    break;
                this.transform.position = Vector3.MoveTowards(this.transform.position, loc, speed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            inTransit = false;
        }
        yield return true;
    }
    private void OnDestroy()
    {
        bool changing;
        if (gameMasterScript != null)
        {
            changing = gameMasterScript.isSceneChanging;
        } else
        {
            changing = true;
        }
        if (!changing) //this check is to see if the object is being removed intrinsically or being removed from the scene changing
        {
            gameMasterScript.RemoveBeingFromList(ID);
        }
    }
    private void OnMouseEnter()
    {
        this.isHovering = true;
    }
    private void OnMouseExit()
    {
        this.isHovering = false;
    }
    public void OnTriggerEnter(Collider other)
    {
        interactable = true;
        if (this.gameObject.tag == "Enemy" && other.gameObject.tag == "Player")
        {
            gameMasterScript.InitializeBattle(gameMasterScript.GetBeingDataByID(this.ID));
        }
    }
    public void OnTriggerExit(Collider other)
    {
        interactable = false;
    }
    public void Say(string[] text)
    {
        if (canvas != null && text.Length > 0)
        {
            StartCoroutine("InstantiateMessage", text);
        }
    }
    public void TeleportTo(Vector3 location)
    {
        this.transform.position = location;
    }
    public void ToggleCanvas()
    {
        if (this.canvas == null)
        {
            this.canvas = this.gameObject.GetComponentInChildren<Canvas>();
        }
        this.canvas.gameObject.SetActive(!this.canvas.gameObject.activeInHierarchy);
    }
    public void ToggleDebug()
    {
        this.debugMode = !this.debugMode;
    }
    public virtual string UpdateBeingJsonData()
    {
        return beingData;
    }  
}
