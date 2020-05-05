using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMaster : GameMaster
{
    bool[] levelLoaded = { false, false, false };
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneChanging = false;
        if (!LevelLoaded(scene.name))
        {
            this.LoadInitialSceneData(scene.name);
        }
        else
        {
            this.LoadGameMasterSceneData();
        }
    }
    public void SceneChange(string scenename)
    {
        this.isSceneChanging = true;
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
