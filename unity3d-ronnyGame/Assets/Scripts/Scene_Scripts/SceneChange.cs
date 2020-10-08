using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public float seconds;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ChangeScene());
    }
    public IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene("Main_Menu");
    }
}
