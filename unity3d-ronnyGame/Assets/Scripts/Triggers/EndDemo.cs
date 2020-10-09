using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndDemo : Trigger
{
    public override IEnumerator EndTrigger(GameObject target)
    {
        Being self = this.gameObject.GetComponent<Being>();
        GameObject ronny = target.GetComponent<Being>().gameMasterScript.GetPlayerGameObject();
        Ronny ronnyScript = ronny.GetComponent<Ronny>();
        ronnyScript.battlePosition = new Vector3(192, 2.75f, -81);
        ronnyScript.StartCoroutine(ronnyScript.MoveToBattlePosition());
        ronny.GetComponent<Animator>().SetBool("Moving", true);
        Camera.main.transform.SetParent(ronny.transform.parent);
        self.gameMasterScript.RemoveBeingFromList(self.ID);
        self.DestroyBeing();
        yield return true;
    }
    public override IEnumerator StartTrigger(GameObject target)
    {
        GameObject gate = GameObject.Find("Ruin_Gate"); //yuck yuck yuck yuck yuck yuck but fuck it
        Vector3 newPos = new Vector3(gate.transform.position.x, 3, gate.transform.position.z);
        Camera cam = Camera.main;
        Vector3 camPos = new Vector3(cam.transform.localPosition.x, cam.transform.localPosition.y, cam.transform.localPosition.z);
        while (gate.transform.position != newPos)
        {
            gate.transform.position = Vector3.MoveTowards(gate.transform.position, newPos, Time.deltaTime * 1);
            Camera.main.transform.localPosition = camPos + (Random.insideUnitSphere * .01f);
            Debug.Log("Screen shake!");
            yield return new WaitForEndOfFrame();
        }
        yield return true;
    }
}
