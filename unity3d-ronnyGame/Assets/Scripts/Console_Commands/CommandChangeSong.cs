using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console
{
    public class CommandChangeSong : ConsoleCommand
    {
        public override string Description { get; set; }
        public override string Help { get; set; }
        public override string Name { get; set; }
        public override string Command { get; set; }

        public CommandChangeSong()
        {
            Description = "This is used to change songs";
            Help = "This command requires one parameter (string) of the song name";
            Name = "ChangeSong";
            Command = "song";

            AddThisToList();
        }
        public override string ExecuteCommand(string[] args)
        {
            SoundMaster soundMaster = Kami.GetComponent<SoundMaster>();
            if (soundMaster.ChangeSong(null,args[1]))
            {
                return "Song Changed Successfully!";
            } else
            {
                return $"Song Changed failed, attempted to search for {args[1]}";
            }
        }
        public static CommandChangeSong CreateCommand()
        {
            return new CommandChangeSong();
        }
    }
}
