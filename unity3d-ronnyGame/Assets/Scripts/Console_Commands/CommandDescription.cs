using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console
{
    public class CommandDescription : ConsoleCommand
    {
        public override string Description { get; set; }
        public override string Help { get; set; }
        public override string Name { get; set; }
        public override string Command { get; set; }

        public CommandDescription()
        {
            Description = "This is the description of the console";
            Help = "Use this command to gain insight as to what this is";
            Name = "Description";
            Command = "description";

            AddThisToList();
        }
        public override string ExecuteCommand(string[] args)
        {
            return "This is tenshi";
        }
        public static CommandDescription CreateCommand()
        {
            return new CommandDescription();
        }

    }
}
