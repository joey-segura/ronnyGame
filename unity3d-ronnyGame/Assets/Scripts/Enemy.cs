using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Fighter
{
    public Vector4[] patrolPoints; //x, y, z, w (duration of time spent there)
    private int patrolIndex = 0;
    public float virtueValue;
    public bool chasingPlayer = false;
    public Coroutine currentPlan;

    public  void Start()
    {
        if (patrolPoints != null)
        {
            //start
            currentPlan = this.StartCoroutine(PatrolPoints());
        }
    }
    public IEnumerator ChasePlayer()
    {
        bool skip = false;
        if (gameMasterScript.GetAllyGameObject() != null)
        {
            if (gameMasterScript.GetAllyGameObject().name.Contains("Joey"))
            {
                if (!gameMasterScript.GetAllyGameObject().GetComponent<Joey>().follow)
                {
                    skip = true;
                }
            }
            else // Ritter
            {
                if (!gameMasterScript.GetAllyGameObject().GetComponent<Ritter>().follow)
                {
                    skip = true;
                }
            }
        } else
        {
            skip = true;
        }
        if (!skip)
        {
            CoroutineWithData cd = new CoroutineWithData(this, this.ChaseObject(gameMasterScript.GetPlayerGameObject()));
            currentPlan = cd.coroutine;
            StopCoroutine(currentPlan);
            float radius = 25;
            Vector3 startingLoc = this.transform.position;
            while (!cd.finished)
            {
                if (Vector3.Distance(startingLoc, this.transform.position) >= radius)
                {
                    this.inTransit = false;
                    StopCoroutine(cd.coroutine);
                    //yield return new WaitForEndOfFrame();
                    CoroutineWithData cd2 = new CoroutineWithData(this, this.MoveToStaticLoc(startingLoc));
                    currentPlan = cd2.coroutine;
                    while (!cd2.finished)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                    currentPlan = this.StartCoroutine(PatrolPoints());
                }
                yield return new WaitForEndOfFrame();
            }
        }
        yield return true;
    }
    public IEnumerator PatrolPoints()
    {
        if (patrolPoints != null && patrolPoints.Length >= 2) // need at least 2 points to go between them
        {
            bool finished = false;
            Vector3[] points = new Vector3[patrolPoints.Length];
            float[] duration = new float[patrolPoints.Length];
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                points[i] = new Vector3(patrolPoints[i].x, patrolPoints[i].y, patrolPoints[i].z);
                duration[i] = patrolPoints[i].w;
            }
            while (!finished) // we actually never want to escape this while within itself but externally and that will be called with StopCoroutine(currentPlan)
            {
                Vector3 loc = points[patrolIndex];
                while (this.transform.position != loc)
                {
                    this.transform.position = Vector3.MoveTowards(this.transform.position, loc, speed * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }
                PlayPointAnimation(patrolIndex);
                yield return new WaitForSeconds(duration[patrolIndex]);
                patrolIndex++;
                if (patrolIndex == patrolPoints.Length)
                {
                    patrolIndex = 0;
                }
            }
            yield return true;
        }
    }
    public virtual void PlayPointAnimation(int pointIndex)
    {
        //animator.setbool("Moving", false) -should be default animation
    }
}
