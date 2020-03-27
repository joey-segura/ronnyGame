using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [SerializeField]
    public Rigidbody rb;
    //private CharacterController controller;
    private Vector3 movement;
    public float moveSpeed = 2f;
    public Animator anim;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        movement = new Vector3(Input.GetAxisRaw("Horizontal")*moveSpeed, 0, Input.GetAxisRaw("Vertical")*moveSpeed);
        rb.velocity = movement;

        //controller.Move(movement * Time.deltaTime);

        //movement.x = moveSpeed * Input.GetAxis("Horizontal");
        //movement.z = moveSpeed * Input.GetAxis("Vertical");
        
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
