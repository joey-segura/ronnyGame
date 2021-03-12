using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console
{
    public class CommandTimeScale : ConsoleCommand
    {
        public override string Description { get; set; }
        public override string Help { get; set; }
        public override string Name { get; set; }
        public override string Command { get; set; }

        public CommandTimeScale()
        {
            Description = "This is used to scale time to a constant value (default is 1)";
            Help = "Use this command to change the rate of time";
            Name = "TimeScale";
            Command = "timescale";

            AddThisToList();
        }
        public override string ExecuteCommand(string[] args)
        {
            int scalar = int.Parse(args[1]);
            if (scalar != 0 && scalar > 0)
            {
                Time.timeScale = scalar;
                return $"Time scaled by {scalar}";
            } else
            {
                return $"failed to scale; input {args[1]} could not be parsed to integer";
            }
        }
        public static CommandTimeScale CreateCommand()
        {
            return new CommandTimeScale();
        }

    }
}