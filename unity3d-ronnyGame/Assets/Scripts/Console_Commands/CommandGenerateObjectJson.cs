using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console
{
    public class CommandGenerateObjectJson : ConsoleCommand
    {
        public override string Description { get; set; }
        public override string Help { get; set; }
        public override string Name { get; set; }
        public override string Command { get; set; }

        public CommandGenerateObjectJson()
        {
            Description = "This is used to Destroy beings by their id";
            Help = "This command requires an ID parameter of the object you wish to generate Json";
            Name = "Json";
            Command = "json";

            AddThisToList();
        }
        public override string ExecuteCommand(string[] args)
        {
            int ID = int.Parse(args[1]);
            if (args[1] != null)
            {
                GameObject Kami = GameObject.Find("Kami");
                GameMaster gameMaster = Kami.GetComponent<GameMaster>();
                BeingData beingData = gameMaster.GetBeingDataByID(ID);
                if (beingData != null)
                {
                    Being being = beingData.gameObject.GetComponent<Being>();
                    TextEditor te = new TextEditor();
                    te.text = being.CompactBeingDataIntoJson();
                    te.SelectAll();
                    te.Copy();
                    return $"Copied Data '{te.text}' to clipboard";
                }
                else
                {
                    return "Could not find being with ID " + args[1];
                }

            }
            return "could not parse string to int";
        }
        public static CommandGenerateObjectJson CreateCommand()
        {
            return new CommandGenerateObjectJson();
        }

    }
}
