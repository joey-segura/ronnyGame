﻿using System;
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
    public float moveSpeed = 2f;
    public Animator anim;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GameObject.Find("Main Camera").GetComponent<Transform>();
    }

    void Update()
    {
        //movement = new Vector3(Input.GetAxisRaw("Horizontal")*moveSpeed, 0, Input.GetAxisRaw("Vertical")*moveSpeed);
        
        movement = Vector3.zero;
        if(Input.GetKey(KeyCode.W))movement += cam.forward * moveSpeed;
        if(Input.GetKey(KeyCode.S))movement += -cam.forward * moveSpeed;;
        if(Input.GetKey(KeyCode.A))movement += -cam.right * moveSpeed;;
        if(Input.GetKey(KeyCode.D))movement += cam.right * moveSpeed;;

        movement.y = 0f;

        transform.position += movement.normalized * moveSpeed * Time.deltaTime;
        
        //rb.velocity = movement;

        anim.SetFloat("Horizontal", movement.x);
        anim.SetFloat("Vertical", movement.z);
        anim.SetFloat("Speed", movement.sqrMagnitude);
    }
}
