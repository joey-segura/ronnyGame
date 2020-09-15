using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public float speed = 0f;
    public float angle = 0f;
    public Animator anim;
    public CharacterController controller;

    void FixedUpdate()
    {

        float x = 0f;
        float z = 0f;

        angle = this.gameObject.transform.rotation.eulerAngles.y;

        if (Input.GetKey(KeyCode.W))
        {
            anim.SetFloat("Facing", 1);
            x += Mathf.Cos(Mathf.Deg2Rad * angle);
            z += Mathf.Sin(Mathf.Deg2Rad * angle);
        } else if (Input.GetKey(KeyCode.S))
        {
            anim.SetFloat("Facing", -1);
            x += Mathf.Cos(Mathf.Deg2Rad * (angle + 180));
            z += Mathf.Sin(Mathf.Deg2Rad * (angle + 180));
        } else
        {
            x += 0;
            z += 0;
        }
        if (Input.GetKey(KeyCode.D))
        {
            anim.SetFloat("Facing", 1);
            x += Mathf.Cos(Mathf.Deg2Rad * (angle + 90));
            z += Mathf.Sin(Mathf.Deg2Rad * (angle + 90));
        }
        else if (Input.GetKey(KeyCode.A))
        {
            anim.SetFloat("Facing", -1);
            x += Mathf.Cos(Mathf.Deg2Rad * (angle - 90));
            z += Mathf.Sin(Mathf.Deg2Rad * (angle - 90));
        }
        else
        {
            x += 0;
            z += 0;
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            anim.SetBool("Moving", true);
        } else
        {
            anim.SetBool("Moving", false);
        }

        Vector3 newPos = this.transform.position + new Vector3(z * speed * Time.deltaTime, 0, x * speed * Time.deltaTime);
        //Vector3 newEnd = new Vector3(newPos.x, newPos.y - 4, newPos.z); //where we want to go
        //Vector3 direction = (newEnd - this.transform.position).normalized;
        RaycastHit hit;

        /*if (Physics.Raycast(this.transform.position, direction, out hit, 4)) // tried to go out from player pos but going up parallel to the wall didn't work
        {
            if (hit.distance > 2f)
            {
                this.transform.position = newPos;
            } else
            {
                Debug.Log(hit.transform.gameObject.name);
            }
        }
        else
        {
            anim.SetBool("Moving", false);
        }*/
        if (Physics.Raycast(newPos, Vector3.down, out hit, 4))
        {
            RaycastHit hit2;
            LayerMask wall = LayerMask.GetMask("Wall");
            if (Physics.Raycast(this.transform.position, (newPos - this.transform.position).normalized, out hit2, 2, wall))
            {
                Debug.Log("Uh oh wall!");
            } else
            {
                this.transform.position = newPos;
            }
            
            Debug.Log(hit.transform.gameObject.name);
        }
        else
        {
            anim.SetBool("Moving", false);
        }


    }
    public void Idle()
    {
        //anim.SetFloat("Facing", 1);
        anim.SetBool("Moving", false);
    }
}
