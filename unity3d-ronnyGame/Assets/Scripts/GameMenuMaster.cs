using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuMaster : Kami
{
    public Canvas canvas;
    public Button save, options, quit;

    // Update is called once per frame
    private void Start()
    {
        save.onClick.AddListener(Save);
        options.onClick.AddListener(Options);
        quit.onClick.AddListener(Quit);
        canvas.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            string sceneName = sceneMaster.GetCurrentSceneName();
            if (!sceneName.Contains("Battle") && sceneName != "Main_Menu")
            {
                canvas.gameObject.SetActive(!canvas.gameObject.activeInHierarchy);
            }
        }
    }
    private void Save()
    {
        if (!sceneMaster.GetCurrentSceneName().Contains("Battle")) 
        {
            dataMaster.Save();
            //gameMaster.SaveMasterData();
        } else
        {
            Debug.LogWarning("Cannot save during battle");
        }
    }
    private void Options()
    {
        this.ToggleMainButtons();
        Debug.Log("Options menu!");
    }
    private void Quit()
    {
        gameMaster.DestroyAllBeings();
        canvas.gameObject.SetActive(false);
        sceneMaster.ChangeScene("Main_Menu");
    } 
    private void ToggleMainButtons()
    {
        save.gameObject.SetActive(!save.gameObject.activeInHierarchy);
        options.gameObject.SetActive(!options.gameObject.activeInHierarchy);
        quit.gameObject.SetActive(!quit.gameObject.activeInHierarchy);
    }
}
