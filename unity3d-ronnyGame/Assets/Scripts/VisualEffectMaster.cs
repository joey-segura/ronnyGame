using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffectMaster : Kami
{
    
    public GameObject InstantiateVisualSprite(Object obj, Vector3 position, Quaternion rotation, Transform parent, float time = 0)
    {
        GameObject entity = Instantiate(obj, position, rotation, parent) as GameObject;
        if (time != 0)
        {
            StartCoroutine(DestroyObjectAfterTime(time, entity));
            return entity;
        } else
        {
            return entity;
        }
    }
    public IEnumerator DestroyObjectAfterTime(float time, GameObject entity)
    {
        yield return new WaitForSeconds(time);
        Destroy(entity);
    }
}
