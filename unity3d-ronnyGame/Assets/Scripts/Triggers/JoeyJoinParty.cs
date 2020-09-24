using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoeyJoinParty : Trigger
{
    // Start is called before the first frame update
    public override IEnumerator EndTrigger(GameObject target)
    {
        target.GetComponent<Joey>().JoinParty();
        yield return true;
    }
}
