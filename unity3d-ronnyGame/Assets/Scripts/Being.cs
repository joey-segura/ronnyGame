using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Being : MonoBehaviour
{
    GameObject gameMasterGameObject;
    GameMaster gameMasterScript;

    private bool debugMode = false;

    public int ID;

    public float multiplier;

    private Camera cam;

    private Renderer renderer;

    private void OnGUI()
    {
        if (debugMode && renderer.isVisible)
        {
            Mesh mesh = this.gameObject.GetComponent<MeshFilter>().mesh;
            Vector3 center = cam.WorldToScreenPoint(this.transform.position);
            float minX = center.x, maxX = center.x, minY = center.y, maxY = center.y;

            for (int x = 0; x < mesh.vertices.Length; x++)
            {
                Vector3 scaledVector = new Vector3(mesh.vertices[x].x * this.transform.localScale.x, mesh.vertices[x].y * this.transform.localScale.y, mesh.vertices[x].z * this.transform.localScale.z);
                Vector3 truePoint = new Vector3((this.transform.position.x + scaledVector.x), (this.transform.position.y + scaledVector.y), (this.transform.position.z + scaledVector.z));
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
        
            GUI.Box(new Rect(minX, (Screen.height - maxY), width, height), "This is a box");
        }
    }

    private void Start()
    {
        renderer = this.GetComponent<Renderer>();
        cam = Camera.main;
        gameMasterGameObject = this.gameObject.transform.parent.gameObject;
        gameMasterScript = gameMasterGameObject.GetComponent<GameMaster>();
    }
    public void ChangeTransparancy(float alpha = .7f)
    {
        this.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, alpha);
    }
    public string CompactBeingDataIntoJson()
    {
        //the function overriding this one will need to return string jsonData
        //will be overriden by extended objects but left to be referenced by Being.CompactBeingDataIntoJson
        return null;
    }
    public void DestroyBeing()
    {
        Destroy(this.gameObject);
    }
    public virtual void InjectData(string jsonData)
    {
        //will be overriden by extended objects but left to be referenced by Being.InjectData()
    }
    public void Interact()
    {
        //will be overriden by extended objects but left to be referenced by Being.Interact()
    }
    private void OnDestroy()
    {
        bool changing = gameMasterScript.isSceneChanging;
        if (!changing) //this check is to see if the object is being removed intrinsically or being removed from the scene changing
        {
            gameMasterScript.RemoveBeingFromList(ID);
        }
    }
    public void Say(string text)
    {
        //instatiate word box object
        //fill with 'text'
    }
    public void TeleportTo(Vector3 location)
    {
        this.transform.position = location;
    }
    public void ToggleDebug()
    {
        this.debugMode = !this.debugMode;
    }
    
}
