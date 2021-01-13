using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    public Light buildLight;
    
    void Start()
    {
        buildLight = GetComponent<Light>();
        buildLight.enabled = false;
    }
}
