using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CharacterController))]

public class playerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 movement;
    public float moveSpeed = 2f;
    public Animator anim;
    
    //public Rigidbody rb;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        controller.Move(movement * Time.deltaTime);

        movement.x = moveSpeed * Input.GetAxis("Horizontal");
        movement.z = moveSpeed * Input.GetAxis("Vertical");
        
        //movement.x = Input.GetAxisRaw("Horizontal");
        //movement.z = Input.GetAxisRaw("Vertical");

        anim.SetFloat("Horizontal", movement.x);
        anim.SetFloat("Vertical", movement.z);
        anim.SetFloat("Speed", movement.sqrMagnitude);
    }

    private void FixedUpdate()
    {
        //rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
