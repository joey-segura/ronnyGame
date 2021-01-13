using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicPlayer : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject cam;
    public UnityEngine.Video.VideoPlayer vidPlayer;
    public GameObject Kami;
    private SceneMaster sceneMaster;
    private void Awake()
    {
        Kami = GameObject.Find("Kami");
        sceneMaster = Kami.GetComponent<SceneMaster>();
        
        cam = Camera.main.gameObject;
        UnityEngine.Video.VideoPlayer vidPlayer = cam.GetComponent<UnityEngine.Video.VideoPlayer>();
        vidPlayer.loopPointReached += EndReached;
        StartCoroutine(PlayVideo(vidPlayer));
    }

    public IEnumerator PlayVideo (UnityEngine.Video.VideoPlayer vidPlayer)
    {
        vidPlayer.Prepare();
        while (!vidPlayer.isPrepared)
        {
            yield return new WaitForEndOfFrame();
        }
        vidPlayer.Play();
    }

    public void Update()
    {
        if (Input.anyKeyDown)
        {
            this.End();
        }
    }
    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        this.End();
    }
    private void End()
    {
        sceneMaster.ChangeScene("Joey");
        sceneMaster.NewGame(); // have to call new game after so sceneMaster's data is finalized with new game data
    }
}
