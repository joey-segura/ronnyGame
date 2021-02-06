using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Console
{
    public abstract class ConsoleCommand
    {
        public static GameObject Kami = GameObject.Find("Kami");
        public abstract string Command { get; set; }
        public abstract string Description { get; set; }
        public abstract string Help { get; set; }
        public abstract string Name { get; set; }
        public void AddThisToList()
        {
            ConsoleMaster.AddCommandToList(Command, this);
        }
        public abstract string ExecuteCommand(string[] args);
    }
    public class ConsoleMaster : MonoBehaviour
    {

        public string[] previousInputs;
        private int viewingIndex = -1;
        public static Dictionary<string,ConsoleCommand> Commands;

        public Canvas consoleCanvas;
        public Text consoleText;
        public Text inputText;
        public InputField consoleInput;

        private void Awake()
        {
            Commands = new Dictionary<string, ConsoleCommand>();
        }

        private void Start()
        {
            consoleCanvas.gameObject.SetActive(false);
            AddLineToConsole("Welcome, please type 'help' to get a list of commands");
            CreateCommands();
        }

        private void CreateCommands()
        {
            CommandBeingDebug.CreateCommand();
            CommandDestroyBeing.CreateCommand();
            CommandDescription.CreateCommand();
            CommandInstantiatePrefab.CreateCommand();
            CommandChangeScene.CreateCommand();
            CommandGenerateObjectJson.CreateCommand();
            CommandChangeSong.CreateCommand();

            CommandHelp.CreateCommand();
        }

        public static void AddCommandToList(string name, ConsoleCommand Command)
        {
            if (!Commands.ContainsKey(name))
            {
                Commands.Add(name, Command);
            }
        }
        public void AddLineToConsole(string input)
        {
            consoleText.text += $"{input} \n";
            //consoleText.text += input + "\n";
        }
        public void CollectInput(string input)
        {
            string[] placeHolder = new string[previousInputs.Length + 1];
            for (int i = 0; i < previousInputs.Length; i++)
            {
                placeHolder[i + 1] = previousInputs[i];
            }
            placeHolder[0] = input;
            previousInputs = placeHolder;
        }
        private void ParseInput(string input)
        {
            string[] args = input.Split(null);

            if (args.Length == 0 || args == null)
            {
                AddLineToConsole("Command not recognized");
                return;
            }

            if (!Commands.ContainsKey(args[0]))
            {
                AddLineToConsole($"{args[0]} is not recognized");
            }
            else
            {
                AddLineToConsole(Commands[args[0]].ExecuteCommand(args)); 
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                consoleCanvas.gameObject.SetActive(!consoleCanvas.gameObject.activeInHierarchy);
                consoleInput.Select();
                consoleInput.ActivateInputField();
                consoleInput.text = string.Empty;
            }
            if (consoleCanvas.gameObject.activeInHierarchy)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (inputText.text != "")
                    {
                        AddLineToConsole(inputText.text);
                        ParseInput(inputText.text);
                        CollectInput(inputText.text);
                        consoleInput.text = string.Empty;
                        consoleInput.Select();
                        consoleInput.ActivateInputField();
                        viewingIndex = -1;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (previousInputs.Length == 0)
                {
                    return;
                }
                viewingIndex++;
                if (viewingIndex > 9)
                {
                    viewingIndex = 9;
                }
                if (viewingIndex > previousInputs.Length - 1 || previousInputs[viewingIndex] == null || previousInputs[viewingIndex] == string.Empty)
                {
                    viewingIndex--;
                }
                consoleInput.text = previousInputs[viewingIndex];
            } else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (previousInputs.Length == 0)
                {
                    return;
                }
                viewingIndex--;
                if (viewingIndex < 0)
                {
                    viewingIndex = -1;
                    consoleInput.text = string.Empty;
                } else
                {
                    consoleInput.text = previousInputs[viewingIndex];
                }
            }
        }
    }
}