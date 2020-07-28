using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuMaster : MonoBehaviour
{
    public Button newGame, loadGame, options, quit;
    public GameObject Kami;
    private SceneMaster sceneMaster;
    protected static string KamiDataPath = "Assets/Resources/Kami/Kami_Data.txt";
    void Awake()
    {
        Kami = GameObject.Find("Kami");
        sceneMaster = Kami.GetComponent<SceneMaster>();
        newGame.onClick.AddListener(NewGame);
        loadGame.onClick.AddListener(LoadGame);
        options.onClick.AddListener(Options);
        quit.onClick.AddListener(Quit);
    }
    void NewGame()
    {
        Debug.LogWarning("New Game currently set to Ritter scene when we plan on having Joeys dungeon first");
        File.WriteAllText(KamiDataPath, string.Empty);
        sceneMaster.NewGame();
        sceneMaster.ChangeScene("Ritter");
    }
    void LoadGame()
    {
        StreamReader reader = new StreamReader(KamiDataPath);
        DataMasterJson data = JsonUtility.FromJson<DataMasterJson>(reader.ReadLine());
        reader.Close();
        sceneMaster.ChangeScene(data.levelName);
    }
    void Options()
    {

    }
    void Quit()
    {
        if (!Application.isEditor)
        {
            Application.Quit();
        } else
        {
            Debug.Log("would have quit but this is the editor!");
        }
    }
}
