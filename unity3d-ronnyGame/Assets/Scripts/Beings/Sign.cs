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
    public override string UpdateBeingJsonData()
    {
        SignJson sign = new SignJson();
        sign.message = this.message;

        return JsonUtility.ToJson(sign);
    }
}
