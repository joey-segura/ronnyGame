using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Console
{
    public class CommandInstantiatePrefab : ConsoleCommand
    {
        public override string Description { get; set; }
        public override string Help { get; set; }
        public override string Name { get; set; }
        public override string Command { get; set; }

        public CommandInstantiatePrefab()
        {
            Description = "This is used to instantaite gameobjects";
            Help = "This command requires one parameter 'prefabname'";
            Name = "CreateBeing";
            Command = "createbeing";


            AddThisToList();
        }
        public override string ExecuteCommand(string[] args)
        {
            if (args[1] != null && Resources.Load($"Prefabs/{args[1]}") != null)
            {
                GameMaster gameMaster = Kami.GetComponent<GameMaster>();
                BeingData being = new BeingData();
                being.prefabName = args[1];
                being.objectID = -1;
                being.location = gameMaster.GetPlayerGameObject().transform.position;
                being.angle = new Quaternion(0, 0, 0, 0);
                gameMaster.InstantiateObject(JsonUtility.ToJson(being));
                
                return "Object instantiated sucessfully";
                
                
            } else if (Resources.Load($"Prefabs/{args[1]}") == null)
            {
                return $"Could not find prefab name {args[1]}";
            }
            {
                return "Missing parameter";
            }
        }
        public static CommandInstantiatePrefab CreateCommand()
        {
            return new CommandInstantiatePrefab();
        }

    }
}
