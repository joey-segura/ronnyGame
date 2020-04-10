using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class playerMovement : MonoBehaviour
{
    [SerializeField] 
    public Transform cam;
    public Rigidbody rb;
    private Vector3 movement;
    public float moveSpeed = 1f, speedEqualizer = 1.2f;
    public Animator anim;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GameObject.Find("Main Camera").GetComponent<Transform>();
    }

    void FixedUpdate()
    {

        movement = Vector3.zero;
        if(Input.GetKey(KeyCode.W))movement += cam.forward * speedEqualizer;
        if(Input.GetKey(KeyCode.S))movement += -cam.forward * speedEqualizer;
        {
            transform.position += movement.normalized * speedEqualizer * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.A))movement += -cam.right * moveSpeed;
        if(Input.GetKey(KeyCode.D))movement += cam.right * moveSpeed;
        {
            transform.position += movement.normalized * moveSpeed * Time.deltaTime;
        }

        movement.y = 0f;

        anim.SetFloat("Horizontal", movement.x);
        anim.SetFloat("Vertical", movement.z);
        anim.SetFloat("Speed", movement.sqrMagnitude);
    }
}
