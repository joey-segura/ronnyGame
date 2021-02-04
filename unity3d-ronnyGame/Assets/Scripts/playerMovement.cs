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
    public Ronny ronny;

    public void Awake()
    {
        ronny = this.GetComponent<Ronny>();
    }

    void FixedUpdate()
    {

        float x = 0f;
        float z = 0f;

        angle = this.gameObject.transform.rotation.eulerAngles.y * -1;

        if (Input.GetKey(KeyCode.W))
        {
            anim.SetFloat("Facing", 1);
            x = Mathf.Cos(Mathf.Deg2Rad * (angle + 90));
            z = Mathf.Sin(Mathf.Deg2Rad * (angle + 90));
        } else if (Input.GetKey(KeyCode.S))
        {
            anim.SetFloat("Facing", -1);
            x = Mathf.Cos(Mathf.Deg2Rad * (angle + 270));
            z = Mathf.Sin(Mathf.Deg2Rad * (angle + 270));
        }
        if (Input.GetKey(KeyCode.D))
        {
            anim.SetFloat("Facing", 1);
            x = Mathf.Cos(Mathf.Deg2Rad * angle);
            z = Mathf.Sin(Mathf.Deg2Rad * angle);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            anim.SetFloat("Facing", -1);
            x = Mathf.Cos(Mathf.Deg2Rad * (angle + 180));
            z = Mathf.Sin(Mathf.Deg2Rad * (angle + 180));
        }
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
        {
            anim.SetFloat("Facing", 1);
            x = Mathf.Cos(Mathf.Deg2Rad * (angle + 45));
            z = Mathf.Sin(Mathf.Deg2Rad * (angle + 45));
        }
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
        {
            anim.SetFloat("Facing", -1);
            x = Mathf.Cos(Mathf.Deg2Rad * (angle + 135));
            z = Mathf.Sin(Mathf.Deg2Rad * (angle + 135));
        }
        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            anim.SetFloat("Facing", 1);
            x = Mathf.Cos(Mathf.Deg2Rad * (angle + 315));
            z = Mathf.Sin(Mathf.Deg2Rad * (angle + 315));
        }
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
        {
            anim.SetFloat("Facing", -1);
            x = Mathf.Cos(Mathf.Deg2Rad * (angle + 225));
            z = Mathf.Sin(Mathf.Deg2Rad * (angle + 225));
        }
        Vector3 newPos = this.transform.position + new Vector3(x * speed * Time.deltaTime, 0, z * speed * Time.deltaTime);
        RaycastHit hit;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            anim.SetBool("Moving", true);
            if (Physics.Raycast(newPos, Vector3.down, out hit, 4))
            {
                //newpos is an actual place to hit! hurray! now check if wall is in the way!
            }
            else
            {
                //Start wallride operation (slip and slide!)
                Vector3 reverse = newPos - this.transform.position;
                Vector3 startingPos = new Vector3(newPos.x, newPos.y - 2.75f, newPos.z);
                float angle = Mathf.Atan2(reverse.z, reverse.x) * Mathf.Rad2Deg;
                float r = Mathf.Sqrt(reverse.x * reverse.x + reverse.z * reverse.z);
                Vector3 posTest = new Vector3(r * Mathf.Cos((angle + 5) * Mathf.Deg2Rad) * -1, 0, r * Mathf.Sin((angle + 5) * Mathf.Deg2Rad) * -1);
                Vector3 negTest = new Vector3(r * Mathf.Cos((angle - 5) * Mathf.Deg2Rad) * -1, 0, r * Mathf.Sin((angle - 5) * Mathf.Deg2Rad) * -1);
                RaycastHit posHit;
                RaycastHit negHit;
                bool calc = false, pos = false, neg = false;
                float dist1 = 0, dist2 = 0;
                Vector3 point1 = Vector3.zero, point2 = Vector3.zero;

                if (Physics.Raycast(startingPos, posTest.normalized, out posHit, 2f))
                {
                    dist1 = posHit.distance;
                    point1 = posHit.point;
                    pos = true;
                }
                if (Physics.Raycast(startingPos, negTest.normalized, out negHit, 2f))
                {
                    dist2 = negHit.distance;
                    point2 = negHit.point;
                    neg = true;
                }

                Vector3 lineVector = point2 - point1;
                if (pos != neg)
                {
                    //Debug.LogError("oh shit!! backup ray!");
                    RaycastHit playa;
                    Debug.DrawRay(startingPos, new Vector3(reverse.x * -1, 0, reverse.z * -1).normalized, Color.yellow);
                    if (Physics.Raycast(startingPos, new Vector3(reverse.x * -1, 0, reverse.z * -1).normalized, out playa, 1))
                    {
                        if (!pos)
                        {
                            dist1 = playa.distance;
                            lineVector = point2 - playa.point;
                        } else
                        {
                            dist2 = playa.distance;
                            lineVector = playa.point - point1;
                        }
                    }
                }
                if (dist1 != 0 && dist2 != 0 && (dist1 + dist2) < 1f)
                {
                    anim.SetBool("Moving", true);
                    calc = true;
                }

                newPos = this.transform.position;

                if (calc && (dist2 - dist1) > 0)
                {
                    newPos -= lineVector.normalized * speed * Time.deltaTime;
                } else if (calc)
                {
                    newPos += lineVector.normalized * speed * Time.deltaTime;
                } else
                {
                    anim.SetBool("Moving", false);
                }
                
            }
        } else
        {
            anim.SetBool("Moving", false);
        }
        if (newPos != this.transform.position)
        {
            RaycastHit final;
            LayerMask wall = LayerMask.GetMask("Wall");
            Debug.DrawRay(this.transform.position, (newPos - this.transform.position).normalized);
            if (Physics.Raycast(this.transform.position, (newPos - this.transform.position).normalized, out final, 2, wall))
            {
                Debug.Log("Uh oh wall!");
            }
            else
            {
                if (Physics.Raycast(newPos, Vector3.down, out hit, 4))
                {
                    this.transform.position = new Vector3(newPos.x, hit.point.y + 2.75f, newPos.z);
                    if (!ronny.playingWalkingSound)
                    {
                        MeshRenderer mesh = hit.collider.gameObject.GetComponent<MeshRenderer>();
                        AudioClip sound = ronny.soundBankMaster.GetSoundOfTexture("Test", hit.textureCoord);
                        ronny.StartCoroutine(ronny.PlayWalkingSound(sound));
                    }
                    //final test to see if we are running off the board, lame solution will need to fix this later
                }
            }
        }
    }
    public void Idle()
    {
        anim.SetBool("Moving", false);
    }
}