using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class walkScript : MonoBehaviour
{
    private float moveSpeed = 10f, gravity = 1f, jumpSpeed = 15f;

    private int jumpCount;
    private int jumpMax;
    
    public bool canMove;
    
    private Vector3 position = Vector3.zero;
    private CharacterController controller;
    private Animator anim;
    
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (canMove == true)
        {
            controller.Move(position*Time.deltaTime);
            position.x = moveSpeed*Input.GetAxis("Horizontal");
            position.y -= gravity;
            anim.SetFloat("Speed", Mathf.Abs(position.x));
        
            if (controller.isGrounded)
            {
                position.y = 0;
                jumpCount = 0;
            }
            
            if (controller.isGrounded)
            {
            
            }

            if (Input.GetButtonDown("Jump"))
            {
            
            }
            
            if (canMove == true && Input.GetKeyDown(KeyCode.LeftArrow))
            {
                transform.rotation = Quaternion.Euler(0,180,0);
            }
        
            if (canMove == true && Input.GetKeyDown(KeyCode.RightArrow))
            {
                transform.rotation = Quaternion.Euler(0,0,0);
            }
        }
    }
}