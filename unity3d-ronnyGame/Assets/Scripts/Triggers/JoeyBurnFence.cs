using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoeyBurnFence : Trigger
{
    Joey joeyScript = null;
    public override IEnumerator EndTrigger(GameObject target)
    {
        joeyScript.follow = true;
        joeyScript.gameMasterScript.RemoveBeingFromList(target.GetComponent<Being>().ID);
        Destroy(target);
        yield return true;
    }
    public override IEnumerator StartTrigger(GameObject target)
    {
        GameObject joey = target.GetComponent<Being>().gameMasterScript.GetAllyGameObject();
        joeyScript = joey.GetComponent<Joey>();
        joeyScript.follow = false;
        Vector3 destination = new Vector3(target.transform.position.x, joey.transform.position.y, target.transform.position.z);
        float joeySpeed = joey.GetComponent<Joey>().speed;
        while (Vector3.Distance(joey.transform.position, destination) > 1)
        {
            joey.transform.position = Vector3.MoveTowards(joey.transform.position, destination, Time.deltaTime * joeySpeed);
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("made it to the fence, play animation!");
        yield return true;
    }
}
