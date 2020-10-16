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
    protected static string KamiDataPath = "/Resources/Kami/Kami_Data.txt";
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
        if (!Directory.Exists(Application.dataPath + KamiDataPath) && !File.Exists(Application.dataPath + KamiDataPath))
        {
            Directory.CreateDirectory(Application.dataPath + "/Resources/Kami");
        }
        File.Create(Application.dataPath + KamiDataPath).Close();
        sceneMaster.ChangeScene("Opening_Cinematic");
    }
    void LoadGame()
    {
        if (!File.Exists(Application.dataPath + KamiDataPath))
        {
            Debug.LogWarning("No file found");
            return;
        }
        StreamReader reader = new StreamReader(Application.dataPath + KamiDataPath);
        if (reader.Peek() != -1)
        {
            DataMasterJson data = JsonUtility.FromJson<DataMasterJson>(reader.ReadLine());
            reader.Close();
            sceneMaster.ChangeScene(data.levelName);
        } else
        {
            Debug.LogWarning("No file data found");
            return;
        }
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
