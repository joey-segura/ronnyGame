using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : Being
{
    public string message;

    new public string CompactBeingDataIntoJson()
    {
        return JsonUtility.ToJson(this);
    }
    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {
            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);
            this.ID = being.objectID;
        } else if (JsonUtility.FromJson<Sign>(jsonData) != null)
        {
            Sign sign = JsonUtility.FromJson<Sign>(jsonData);
            this.message = sign.message;
            this.ID = sign.ID;
        }
        
        return;
    }
    new public void Interact()
    {
        this.Say(message);
    }
}
