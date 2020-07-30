using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignJson
{
    public string[] message;
}
public class Sign : Being
{
   
    public string[] message;

    public override string CompactBeingDataIntoJson()
    {
        BeingData being = JsonUtility.FromJson<BeingData>(this.beingData);
        being.location = this.gameObject.transform.position;
        being.angle = this.gameObject.transform.rotation;
        being.scale = this.gameObject.transform.localScale;
        being.gameObject = this.gameObject;
        being.prefabName = this.gameObject.name;
        being.objectID = this.ID;
       
        SignJson sign = new SignJson();
        sign.message = this.message;

        being.jsonData = JsonUtility.ToJson(sign);

        return JsonUtility.ToJson(being);
    }
    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {
            
            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);

            if (being.jsonData != null && being.jsonData != string.Empty)
            {
                SignJson sign = JsonUtility.FromJson<SignJson>(being.jsonData);
                this.message = sign.message;
            } else
            {
                this.message[0] = "Default";
            }
            
            this.ID = being.objectID;
            this.beingData = jsonData;
        } 

        return;
    }
    public override void Interact()
    {
        this.Say(message);
    }
}
