using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMaster : Kami
{
    public static SceneMaster Instance { get; private set; }
    public bool[] levelLoaded = { false, false, false, false, false, false, false, false, false, false, false, false };

    public static string[] SCENENAMES = { "exploreScene", "rotation_testScene", "Battle_Scene_Test", "Ritter", "Ritter_Battle", "Joey", "Joey_Battle", "Main_Menu", "Opening_Cinematic" };

    public string currentSceneName;
    public string lastSceneName = "Main_Menu";

    public bool newGame = false;

    private new void Awake()
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
        base.Awake();
    }
    public void ChangeScene(string sceneName)
    {
        //this.LoadClasses();
        if (Instance != this)
        {
            return;
        }
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
            
            this.lastSceneName = this.currentSceneName;
            SceneManager.LoadScene(sceneName);
            currentSceneName = sceneName;
        } else
        {
            Debug.LogError($"Scene name invalid {sceneName}, Check unity build settings?");
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
    public void InjectData(SceneMasterJson data)
    {
        this.levelLoaded = data.levelLoaded;
        this.currentSceneName = data.currentSceneName;
        this.newGame = data.newGame;
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
    public void NewGame()
    {
        this.newGame = true;
        for (int i = 0; i < levelLoaded.Length; i++)
        {
            levelLoaded[i] = false;
        }
        //this.currentSceneName = "Main_Menu";
        this.lastSceneName = "Main_Menu";
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameMaster.isSceneChanging = true;
        this.LoadClasses();
        if (Instance != this)
        {
            Destroy(this.gameObject);
        }
        this.currentSceneName = scene.name;

        if (scene.name == "Main_Menu")
        {
            return;
        }

        if (lastSceneName == "Main_Menu")
        {
            if (this.newGame) // this gets flagged true in MainMenuMaster.cs by calling sceneMaster.NewGame()
            {
                //wipe data and load initial scene data of first dungeon
                gameMaster.NewGame();
                LevelLoaded(scene.name);
                this.newGame = false;
            } else
            {
                dataMaster.LoadDataFromFile(); // this eventually will call gameMaster.LoadGameMasterSceneData
            }
        } else
        {
            if (!scene.name.Contains("Battle"))
            {
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
            }
        }
    }
    public bool WasLastSceneBattle()
    {
        if (this.lastSceneName.Contains("Battle"))
        {
            return true;
        }
        else 
        {
            return false;
        }
    }
}
