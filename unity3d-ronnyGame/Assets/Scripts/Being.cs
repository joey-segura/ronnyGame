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

    protected GameObject Kami;
    protected GameMaster gameMasterScript;

    private bool debugMode = false;

    public bool interactable = false;

    protected bool isHovering = false;

    public int ID { get; set; }

    public string beingData;

    public float multiplier;

    private Camera cam;

    public Canvas canvas;

    private void OnGUI()
    {
        if (debugMode && this.GetComponent<Renderer>().isVisible)
        {
            Vector3[] verts;
            if (this.gameObject.GetComponent<MeshFilter>() != null) //3D objects
            {
                Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
                verts = mesh.vertices;
            }
            else //2D
            {
                SpriteRenderer spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
                Vector3[] vert = new Vector3[spriteRenderer.sprite.vertices.Length];
                for (int i = 0; i < vert.Length; i++)
                {
                    vert[i] = new Vector3(spriteRenderer.sprite.vertices[i].x, spriteRenderer.sprite.vertices[i].y, 0);
                }
                verts = vert;
            }
                
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

                GUI.Box(new Rect(minX, (Screen.height - maxY), width, height), $"{this.gameObject.name} {ID.ToString()}");
        }
    }
    private void Update()
    {
        if (interactable && Input.GetKeyDown(KeyCode.R)) //Interact Key (need to generalize in the future to allow remapping etc)
        {
            this.Interact();
        }
    }

    private void Start()
    {
        if (this.gameObject.GetComponentInChildren<Canvas>() != null)
        {
            
            this.canvas = this.gameObject.GetComponentInChildren<Canvas>();
            this.ToggleCanvas();
        }
        cam = Camera.main;
        Kami = this.gameObject.transform.parent.gameObject;
        gameMasterScript = Kami.GetComponent<GameMaster>();
    }
    public void ChangeTransparancy(float alpha = .7f)
    {
        this.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, alpha);
    }
    public virtual string CompactBeingDataIntoJson()
    {
        //the function overriding this one will need to return string jsonData
        //will be overriden by extended objects but left to be referenced by Being.CompactBeingDataIntoJson
        //need to bundle up class specific json (jsonUtility.ToJson('individual class'))
        //need to update values in beingData (location, angle, scale, jsonData (class specific json))
        return JsonUtility.ToJson(beingData);
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

}
