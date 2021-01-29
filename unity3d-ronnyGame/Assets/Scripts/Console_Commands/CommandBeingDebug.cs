using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console
{
    public class CommandBeingDebug : ConsoleCommand
    {
        public override string Description { get; set; }
        public override string Help { get; set; }
        public override string Name { get; set; }
        public override string Command { get; set; }

        public CommandBeingDebug()
        {
            Description = "This will display the beings colider as well as display information about that being";
            Help = "This command toggles all beings into debug mode or just a specific ID if ID is supplied as the first parameter";
            Name = "BeingDebug";
            Command = "debug";
            
            AddThisToList();
        }
        public override string ExecuteCommand(string[] args)
        {
            if (args.Length >= 2)
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
                        being.ToggleDebug();
                        return "Debug";
                    }
                    else
                    {
                        return "Could not find being with ID " + args[1];
                    }
                } else
                {
                    return "could not parse string to int";
                }
            } else
            {
                GameObject Kami = GameObject.Find("Kami");
                GameMaster gameMaster = Kami.GetComponent<GameMaster>();
                ListBeingData data = gameMaster.GameMasterBeingDataList;
                foreach (BeingData beingData in data.BeingDatas)
                {
                    Being being = beingData.gameObject.GetComponent<Being>();
                    being.ToggleDebug();
                }
                return "All beings toggled";
            }
        }
        public static CommandBeingDebug CreateCommand()
        {
            return new CommandBeingDebug();
        }
    }
}
