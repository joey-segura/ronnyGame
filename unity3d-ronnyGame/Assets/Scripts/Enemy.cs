using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GenericEnemy
{
    public int damage;
    public float speed, health, virtueValue;
    public string[] party;
    public Vector3[] patrolPoints;
    public Vector2[] patrolInformation;
}

public class Enemy : Fighter
{
    //public Vector4[] patrolPoints; //x, y, z, w (duration of time spent there)
    public Vector3[] patrolPoints;
    public Vector2[] patrolInformation;
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
    public void GenericEnemyInject(string genericJsonData)
    {
        GenericEnemy generic = JsonUtility.FromJson<GenericEnemy>(genericJsonData);
        this.speed = generic.speed;
        this.health = generic.health;
        this.damage = generic.damage;
        this.party = generic.party;
        this.virtueValue = generic.virtueValue;
        this.patrolPoints = generic.patrolPoints == null ? null : (Vector3[])generic.patrolPoints.Clone();
        this.patrolInformation = generic.patrolInformation == null ? null : (Vector2[])generic.patrolInformation.Clone();
    }
    public string GenericEnemyJsonify()
    {
        GenericEnemy generic = new GenericEnemy();
        generic.speed = this.speed;
        generic.health = this.health;
        generic.damage = this.damage;
        generic.party = this.party;
        generic.virtueValue = this.virtueValue;
        generic.patrolPoints = this.patrolPoints == null ? null : (Vector3[])this.patrolPoints.Clone();
        generic.patrolInformation = this.patrolInformation == null ? null : (Vector2[])this.patrolInformation.Clone();
        return JsonUtility.ToJson(generic);
    }
    public IEnumerator PatrolPoints()
    {
        if (patrolPoints != null && patrolPoints.Length >= 2) // need at least 2 points to go between them
        {
            if (patrolPoints.Length != patrolInformation.Length)
            {
                Debug.LogError("PATROL POINT INFORMATION VECTOR DOES NOT MATCH SAME LENGTH AS PATROL INFORMATION VECTOR");
            }
            bool finished = false;
            Vector3[] points = new Vector3[patrolPoints.Length];
            float[] duration = new float[patrolInformation.Length];
            int[] animationIndex = new int[patrolInformation.Length];
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                points[i] = new Vector3(patrolPoints[i].x, patrolPoints[i].y, patrolPoints[i].z);
                duration[i] = patrolInformation[i].x;
                animationIndex[i] = (int)patrolInformation[i].y;
            }
            while (!finished) // we actually never want to escape this while within itself but externally and that will be called with StopCoroutine(currentPlan)
            {
                Vector3 loc = points[patrolIndex];
                while (this.transform.position != loc)
                {
                    this.transform.position = Vector3.MoveTowards(this.transform.position, loc, speed * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }
                PlayPointAnimation(animationIndex[patrolIndex]);
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
