using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnableFence : Being
{
    // Start is called before the first frame update

    public override void InjectData(string jsonData)
    {
        if (JsonUtility.FromJson<BeingData>(jsonData) != null)
        {
            BeingData being = JsonUtility.FromJson<BeingData>(jsonData);

            this.ID = being.objectID;
            this.beingData = jsonData;
        }
        return;
    }
    public override string UpdateBeingJsonData()
    {
        return JsonUtility.ToJson(this.beingData);
    }
}
