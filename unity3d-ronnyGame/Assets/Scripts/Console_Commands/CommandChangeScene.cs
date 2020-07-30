using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console
{
    public class CommandChangeScene : ConsoleCommand
    {
        public override string Description { get; set; }
        public override string Help { get; set; }
        public override string Name { get; set; }
        public override string Command { get; set; }

        public CommandChangeScene()
        {
            Description = "This is used to have unity change scenes";
            Help = "This command requires one parameter (string) of the scene name";
            Name = "ChangeScene";
            Command = "changescene";

            AddThisToList();
        }
        public override string ExecuteCommand(string[] args)
        {
            SceneMaster sceneMaster = Kami.GetComponent<SceneMaster>();
            if (args[0] != string.Empty && args[1] != null)
            {
                sceneMaster.ChangeScene(args[1]);
            }
            return $"Scene failed or Scene name {args[1]} was invalid";
        }
        public static CommandChangeScene CreateCommand()
        {
            return new CommandChangeScene();
        }

    }
}
