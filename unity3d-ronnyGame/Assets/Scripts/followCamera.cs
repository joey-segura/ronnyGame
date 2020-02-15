using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class followCamera : MonoBehaviour
{
    public Transform focus;
    public float followSpeed = 10f;
    public Vector3 distance;
    
    void Start()
    {
        focus = GameObject.Find("testChar_001").transform;
    }


    private void LateUpdate()
    {
        Vector2 desiredPosition = focus.position + distance;
        Vector2 smoothedPosition = Vector2.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
