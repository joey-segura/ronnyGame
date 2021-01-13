using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console
{
    public class CommandHelp : ConsoleCommand
    {
        public override string Description { get; set; }
        public override string Help { get; set; }
        public override string Name { get; set; }
        public override string Command { get; set; }

        public CommandHelp()
        {
            Description = "This is to help users understand how to use this console";
            Help = "Use this command to get help on how to use the console";
            Name = "Help";
            Command = "help";

            AddThisToList();
        }
        public override string ExecuteCommand(string[] args)
        {
            string output = null;
            foreach (KeyValuePair<string, ConsoleCommand> entry in ConsoleMaster.Commands) 
            {
                output += "Name: " + entry.Value.Name + "\n";
                output += "Description: " +  entry.Value.Description + "\n";
                output += "Help: " +  entry.Value.Help + "\n";
                output += "Command: " +  entry.Value.Command + "\n";
                if (entry.Value.Name != "Help")
                {
                    output += "\n";
                }
            }
            return output;
        }
        public static CommandHelp CreateCommand()
        {
            return new CommandHelp();
        }

    }
}
