using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMaster : Kami
{
    public static SceneMaster Instance { get; private set; }
    public bool[] levelLoaded = { false, false, false, false, true, false, false, true, true, true};

    public static string[] SCENENAMES = { "exploreScene", "rotation_testScene", "Battle_Scene_Test", "Ritter", "Ritter_Battle", "Joey", "Joey_Battle" };

    private string currentSceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void ChangeScene(string sceneName)
    {
        bool validSceneName = false;
        int count = SCENENAMES.Length;

        for (int i = 0; i < count; i++)
        {
            if (SCENENAMES[i] == sceneName)
            {
                validSceneName = true;
            }
        }
        if (validSceneName)
        {
            gameMaster.UpdateAllBeingsInList();
            gameMaster.DestroyAllBeings();
            gameMaster.isSceneChanging = true;
            SceneManager.LoadScene(sceneName);
            currentSceneName = sceneName;
        } else
        {
            Debug.LogError("Scene name invalid " + sceneName + " Check unity build settings?");
        }
    }
    public string GetBattleSceneName(string sceneName)
    {
        string battleSceneName = string.Empty;
        for (int i = 0; i < SCENENAMES.Length; i++)
        {
            if (SCENENAMES[i] == sceneName)
            {
                battleSceneName = SCENENAMES[i] + "_Battle";
            }
        }
        return battleSceneName;
    }
    public string GetCurrentSceneName()
    {
        return this.currentSceneName;
    }
    private bool LevelLoaded(string sceneName)
    {
        switch (sceneName)
        {
            case "rotation_testScene":
                if (!levelLoaded[0])
                {
                    levelLoaded[0] = true;
                    return false;
                } else
                {
                    return true;
                }
            case "Ritter":
                //if level[x] is false change it to true and then return !level[x] to send false otherwise just return level[x] (which should just be true)
                if (!levelLoaded[1])
                {
                    levelLoaded[1] = true;
                    return false;
                }
                else
                {
                    return true;
                }
            case "Joey":
                if (!levelLoaded[8])
                {
                    levelLoaded[8] = true;
                    return false;
                }
                else
                {
                    return true;
                }
            case "Ronny":
                break;
            case "Battle_Scene_Test":
                if (!levelLoaded[4])
                {
                    levelLoaded[4] = true;
                    return false;
                }
                else
                {
                    return true;
                }
            case "exploreScene":
                if (!levelLoaded[5])
                {
                    levelLoaded[5] = true;
                    return false;
                }
                else
                {
                    return true;
                }
            default:
                return false;
        }
        return false;
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.LoadClasses();
        if(Instance != this)
        {
            Destroy(this.gameObject);
        }
        this.currentSceneName = scene.name;
        if (!scene.name.Contains("Battle"))
        {
            gameMaster.isSceneChanging = false;
            if (!LevelLoaded(scene.name))
            {
                Debug.Log("Level not loaded before");
                gameMaster.LoadInitialSceneData(scene.name);
            }
            else
            {
                Debug.Log("Level loaded before! gameMaster do your thing!");
                gameMaster.LoadGameMasterSceneData();
            }
        } else
        {
            return;
        }
    }
}
