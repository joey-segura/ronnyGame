using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public virtual IEnumerator EndTrigger(GameObject target)
    {
        yield return null;
    }
    public virtual IEnumerator StartTrigger(GameObject target)
    {
        yield return null;
    }
}
