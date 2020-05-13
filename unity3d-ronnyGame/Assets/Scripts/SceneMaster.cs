using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMaster : Kami
{
    bool[] levelLoaded = { false, false, false };


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameMaster.isSceneChanging = false;
        if (!LevelLoaded(scene.name))
        {
            gameMaster.LoadInitialSceneData(scene.name);
        }
        else
        {
            gameMaster.LoadGameMasterSceneData();
        }
    }
    public void SceneChange(string scenename)
    {
        gameMaster.isSceneChanging = true;
        SceneManager.LoadScene(scenename);
    }
    private bool LevelLoaded(string sceneName)
    {
        switch (sceneName)
        {
            case "Ritter":
                //if level_x is false change it to true and then return !level_x to send false otherwise just return level_x (which should just be true)
                break;
            case "Pixeltic":
                break;
            case "Boss":
                break;
            default:
                return false;
        }
        return false;
    }
}
