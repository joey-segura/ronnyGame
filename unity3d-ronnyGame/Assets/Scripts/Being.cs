using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Being : MonoBehaviour
{
    GameObject gameMasterGameObject;
    GameMaster gameMasterScript;
    public int ID;

    private void Start()
    {
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
    private void DestroyBeing()
    {
        Destroy(this.gameObject);
    }
    public void InjectData(string jsonData)
    {
        //will be overriden by extended objects but left to be referenced by Being.InjectData()
    }
    public void Interact()
    {
        //will be overriden by extended objects but left to be referenced by Being.Interact()
    }
    private void OnDestroy()
    {
        if (!gameMasterScript.isSceneChanging) //this check is to see if the object is being removed intrinsically or being removed from the scene changing
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
    
}
