using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class companionFollow : MonoBehaviour
{
    public GameObject ronny;
    public float targetDistance, allowedDistance = 1, followSpeed;
    public GameObject companion1, companion2;
    public RaycastHit ray;
    
    void FixedUpdate()
    {
        transform.LookAt(ronny.transform);
        transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up) * transform.rotation;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward),out ray))
        {
            targetDistance = ray.distance;
            if (targetDistance >= allowedDistance)
            {
                followSpeed = 1f;
                transform.position = Vector3.MoveTowards(transform.position, ronny.transform.position, followSpeed);
                //walking_anim
            }
        }
        else
        {
            followSpeed = 0;
            //idle_anim
        }
    }
}
