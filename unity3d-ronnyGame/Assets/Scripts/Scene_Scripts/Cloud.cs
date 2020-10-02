using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public int id;
    public Vector3 direction;
    public float speed = 2.5f;
    public CloudGenerator parentGenerator;

    public void Awake()
    {
        parentGenerator = this.transform.GetComponentInParent<CloudGenerator>();
    }
    public void Update()
    {
        if (!parentGenerator.bound.Contains(this.transform.localPosition))
        {
            parentGenerator.RemoveCloud(id);
            Destroy(this.gameObject);
        }
        if (direction != null)
        {
            this.transform.localPosition += (direction * (Time.deltaTime * speed));
        }
        
    }
}
