using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudGenerator : MonoBehaviour
{
    public GameObject cloud1, cloud2, cloud3, cloud4;
    public float xMin, xMax, zMin, zMax, yMin, yMax, spawnFrequency;
    private float xDif, zDif, yDif;
    public int cloudMin, cloudMax, currentClouds;
    public Vector3 cloudDirection = Vector3.left, startingBound = new Vector3(1,0,0);
    public GameObject[] clouds;
    public Bounds bound;

    public void Awake()
    {
        FillBounds();
        float xCenter = (xMin + xMax) / 2;
        float zCenter = (zMin + zMax) / 2;
        float yCenter = (yMin + yMax) / 2;
        bound = new Bounds(new Vector3(xCenter, yCenter, zCenter), new Vector3(Mathf.Abs(xDif), Mathf.Abs(yDif), Mathf.Abs(zDif)));
    }
    public void Update()
    {
        if (currentClouds < cloudMin)
        {
            int increase = Random.Range(1, (cloudMax - cloudMin));
            int spawn = 0;
            for (int i = 0; i < clouds.Length; i++)
            {
                if (clouds[i] == null)
                {
                    clouds[i] = SpawnCloud(false, i);
                    spawn++;
                    if (spawn >= increase)
                    {
                        break;
                    }
                }
            }
        }
        if (currentClouds < cloudMax)
        {
            int max = 100000;
            int rand = Random.Range(1, max);
            if (rand <= max * spawnFrequency)
            {
                for (int i = 0; i < clouds.Length; i++)
                {
                    if (clouds[i] == null)
                    {
                        clouds[i] = SpawnCloud(false, i);
                        break;
                    }
                }
            }
        }
    }
    public void FillBounds()
    {
        int rand = Random.Range(cloudMin, cloudMax);
        xDif = xMax - xMin;
        zDif = zMax - zMin;
        yDif = yMax - yMin;
        clouds = new GameObject[cloudMax];

        for (int i = 0; i < rand; i++)
        {
            clouds[i] = SpawnCloud(true, i);
        }
    }
    public void RemoveCloud(int ID)
    {
        clouds[ID] = null;
        currentClouds--;
    }
    public GameObject SpawnCloud(bool initial, int ID)
    {
        Vector3 pos = new Vector3(0,0,0);
        if (initial)
        {
            
            bool valid = false;
            int failcheck = 0;
            while (!valid)
            {
                float xVariance = xDif * Random.value;
                float zVariance = zDif * Random.value;
                float yVariance = yDif * Random.value;
                pos = new Vector3(xMin + xVariance, yMin + yVariance, zMin + zVariance);
                valid = true;
                for (int i = 0; i < clouds.Length; i++)
                {
                    if (clouds[i] != null && Vector3.Distance(clouds[i].transform.position, pos) < 15) {
                        valid = false;
                        break;
                    }
                }
                failcheck++;
                if (failcheck > 100)
                {
                    Debug.Log("fuck");
                    valid = true;
                }
            }
        } else
        {
            if (startingBound.magnitude != Mathf.Sqrt(1))
            {
                Debug.LogError("is your startingBound only have a singular variable? I.E., 1,0,0 or -1,0,0 or 0,0,1 you can't have it split 1,1,1");
            } else
            {
                int fail = 0;
                if (startingBound.x != 0)
                {
                    fail++;
                }
                if (startingBound.y != 0)
                {
                    fail++;
                }
                if (startingBound.z != 0)
                {
                    fail++;
                }
                if (fail > 1)
                {
                    Debug.LogError("startingBound values incompatible, bug found");
                }
            }
            if (startingBound.x == 1)
            {
                pos.x = xMax;
            }
            else
            {
                float variance = xDif * Random.value;
                pos.x = xMin + variance;
            }
            if (startingBound.y == 1)
            {
                pos.y = yMax;
            }
            else
            {
                float variance = yDif * Random.value;
                pos.y = yMin + variance;
            }
            if (startingBound.z == 1)
            {
                pos.z = zMax;
            }
            else
            {
                float variance = zDif * Random.value;
                pos.z = zMin + variance;
            }
            
        }
        GameObject cloud = null;
        
        switch (Random.Range(1,4))
        {
            case 1:
                cloud = cloud1;
                break;
            case 2:
                cloud = cloud2;
                break;
            case 3:
                cloud = cloud3;
                break;
            case 4:
                cloud = cloud4;
                break;
        }
        Quaternion rotation = Quaternion.Euler(0, 90, 0);
        GameObject obj = Instantiate(cloud, pos, rotation, this.transform);
        Cloud cloudScript = obj.GetComponent<Cloud>();
        cloudScript.direction = cloudDirection;
        cloudScript.id = ID;
        currentClouds++;
        return obj;
    }
}
