using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Zork.Common
{
    public class Game : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public World World { get; private set; }

        public string StartingLocation { get; set; }
        
        public string WelcomeMessage { get; set; }
        
        public string ExitMessage { get; set; }
        
        public IOutputService Output { get; set; }

        public IInputService Input { get; set; }

        [JsonIgnore]
        public Player Player { get; private set; }

        public bool IsRunning { get; set; }

        [JsonIgnore]
        public Dictionary<string, Command> Commands { get; private set; }

        public Game(World world, Player player)
        {
            World = world;
            Player = player;

            Commands = new Dictionary<string, Command>()
            {
                { "QUIT", new Command("QUIT", new string[] { "QUIT", "Q", "BYE" }, Quit, "Exit the game.") },
                { "LOOK", new Command("LOOK", new string[] { "LOOK", "L" }, Look, "Examine the current room.") },
                { "NORTH", new Command("NORTH", new string[] { "NORTH", "N" }, CommandContext => Move(CommandContext, Directions.North)) },
                { "SOUTH", new Command("SOUTH", new string[] { "SOUTH", "S" }, CommandContext => Move(CommandContext, Directions.South)) },
                { "EAST", new Command("EAST", new string[] { "EAST", "E"}, CommandContext => Move(CommandContext, Directions.East)) },
                { "WEST", new Command("WEST", new string[] { "WEST", "W" }, CommandContext => Move(CommandContext, Directions.West)) },
                { "REWARD", new Command("REWARD", new string[] { "REWARD", "R"}, CommandContext => Reward(CommandContext,5), "Just straight-up cheat and increase your score by 5.") },
                { "SCORE", new Command("SCORE", new string[] { "SCORE"}, Score, "Check your score. Gain score by collection treasures.") },
                { "INVENTORY", new Command("INVENTORY", new string[] { "INVENTORY","I"}, Inventory, "Learn the contents of your inventory.") },
                { "GET", new Command("GET", new string[] { "GET", "TAKE", "G"}, CommandContext => Get(CommandContext), "Take an object in the room, if it can be taken.") },
                { "EXAMINE", new Command("EXAMINE X", new string[] { "EXAMINE","EX"},CommandContext => Examine(CommandContext), "Examine an object in the room or in your inventory.") },
                { "DROP", new Command("DROP", new string[] { "DROP","D"}, CommandContext => Drop(CommandContext), "Leave on object in this room.") },
                { "EQUIP", new Command("EQUIP", new string[] { "EQUIP","EQ"}, CommandContext => Equip(CommandContext), "Equip an object, if it can be. Certain objects must be equipped to enter certain areas.") }, 
                { "UNEQUIP", new Command("EXAMINE", new string[] { "UNEQUIP","UNEQ", "DEQUIP","DEQ"}, CommandContext => Unequip(CommandContext), "Unequip an object.") },
                { "USE", new Command("USE", new string[] { "USE"}, CommandContext => Use(CommandContext), "Use an object from your inventory, equipped or not, on an object in the current room.") },
                { "HELP", new Command("HELP", new string[] { "HELP", "\"HELP\"","\'HELP\'", "H"}, CommandContext => Help(CommandContext), "Access this help menu again.") }
            };
        }

        public void Start(IInputService input, IOutputService output)
        {
            Assert.IsNotNull(input);
            Input = input;
            Input.InputReceived += InputReceivedHandler;

            Assert.IsNotNull(output);
            Output = output;

            Output.WriteLine(string.IsNullOrWhiteSpace(WelcomeMessage) ? "Welcome to Zork!" : WelcomeMessage);
            IsRunning = true;            
        }

        private void InputReceivedHandler(object sender, string commandString)
        {
            String[] splitCommand = commandString.Split(' ');
            CommandContext newContext = new CommandContext(this, commandString);
            {
                if (splitCommand.Length == 1)
                {
                    newContext = new CommandContext(this, commandString, splitCommand[0]);
                }
                else if (splitCommand.Length == 2)
                {
                    newContext = new CommandContext(this, commandString, splitCommand[0], splitCommand[1]);
                }
                else
                {
                    newContext = new CommandContext(this, commandString, splitCommand[0], splitCommand[1], splitCommand[splitCommand.Count() - 1]);
                }
            }
            
            Command foundCommand = null;
            foreach (Command command in Commands.Values)
            {
                if (command.Verbs.Contains(newContext.Verb))
                {
                    foundCommand = command;
                    break;
                }
            }

            if (foundCommand != null)
            {
                foundCommand.Action(newContext);
                Player.Moves++;
            }
            else
            {
                Output.WriteLine("Unknown command. Try entering: \"Help\" to recieve a list of all commands.");
            }
        }

        private static void Help(CommandContext commandContext)
        {
            Game game = commandContext.Game;
            foreach (KeyValuePair<string,Command> entry in commandContext.Game.Commands)
            {
                if (entry.Value.Description != null)
                {
                    game.Output.WriteLine($"{entry.Key} ({entry.Value.VerbAliasList}) - {entry.Value.Description}");
                }
                else
                {
                    game.Output.WriteLine($"{entry.Key} ({entry.Value.VerbAliasList})");
                }
                
            }
            game.Output.WriteLine("");
        }

        private static void Move(CommandContext commandContext, Directions direction)
        {
            Game game = commandContext.Game;
            if (game.Player.Move(commandContext,direction) == false)
            {
                game.Output.WriteLine("The way is shut!");                
            }
        }

        public static Game Load(string filename)
        {
            Game game = JsonConvert.DeserializeObject<Game>(File.ReadAllText(filename));
            game.Player = new Player(game.World, game.StartingLocation);
            return game;
        }

        public static void Look(CommandContext commandContext)
        {
            Game game = commandContext.Game;
            game.Output.WriteLine(game.Player.Location.Description);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.LocationInWorld.ToUpper() == game.Player.Location.Name.ToUpper())
                {
                    game.Output.WriteLine(wObj.WorldDescription);
                }
            }
        }

        private static void Quit(CommandContext commandContext)
        {
            Game game = commandContext.Game;
            game.Player.HasQuit = true;
            game.IsRunning = false;
        }

        public static void Reward(CommandContext commandContext, int scoreAmt)
        {
            Game game = commandContext.Game;
            game.Player.Score = game.Player.Score + scoreAmt;
        }

        public static void Score(CommandContext commandContext) 
        {
            Game game = commandContext.Game;
            game.Output.WriteLine($"Your current score is: {game.Player.Score}");
        } 

        public static void Inventory(CommandContext commandContext)
        {
            Game game = commandContext.Game;
            game.Output.WriteLine("Your inventory contains:");
            if (game.Player.Inventory.Count == 0)
            {
                game.Output.WriteLine("Nothing, currently.");
            }
            else
            {
                foreach (WorldObject wObject in game.Player.Inventory)
                {
                    if (wObject.IsEquipped)
                    {
                        game.Output.WriteLine($"{wObject.Name} - Equipped to {wObject.EquipLocation}");
                    }
                    else
                    {
                        game.Output.WriteLine(wObject.Name);
                    }
                }
            }
            game.Output.WriteLine("");

            game.Output.WriteLine("The world contains:");
            foreach(WorldObject wo in game.World.WorldObjects)
            {
                game.Output.WriteLine($"> \"{wo.Name}\" in \"{wo.LocationInWorld}\", ");
            }

            game.Output.WriteLine("");
        }

        public static void Get(CommandContext commandContext)
        {
            Game game = commandContext.Game;
            string objName;
            if (commandContext.Subject != null)
            {
                objName = commandContext.Subject;
            }
            else
            {
                objName = "Nothing";
            }
            WorldObject obj = new WorldObject(objName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name.ToUpper() == obj.Name.ToUpper())
                {
                    obj = wObj;
                    break;
                }
            }

            if (game.World.WorldObjects.Contains(obj))
            {
                if (obj.LocationInWorld.ToUpper() != game.Player.Location.Name.ToUpper())
                {
                    game.Output.WriteLine($"A {obj.Name} doesn't appear to be here in {game.Player.Location.Name}");
                }
                else
                {

                    if (!obj.IsTakeable) 
                    {
                        game.Output.WriteLine($"You cannot carry the {obj.Name}.");
                    }
                    else
                    {
                        game.Player.Inventory.Add(obj);
                        obj.LocationInWorld = "PlayerInventory";
                        game.Output.WriteLine($"You take the {obj.Name}.");
                        Reward(commandContext, obj.ScoreValue);
                        obj.ScoreValue = 0; //Remove score value so the player cannot farm score from picking up and dropping an item repeatedly
                        
                    }

                    
                }
            }
            else
            {
                if (obj.Name == "Nothing")
                {
                    game.Output.WriteLine($"You can't pick up nothing.");
                }
                else
                {
                    game.Output.WriteLine($"Whatever a \"{obj.Name}\" is, there isn't one here, or anywhere in this world, for that matter.");
                }
            }
            
        }

        public static void Drop(CommandContext commandContext)
        {
            Game game = commandContext.Game;
            string objName;
            if(commandContext.Subject != null)
            {
                objName = commandContext.Subject;
            }
            else
            {
                objName = "Nothing";
            }
            WorldObject obj = new WorldObject(objName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name.ToUpper() == obj.Name.ToUpper())
                {
                    obj = wObj;
                    break;
                }
            }

            if (game.World.WorldObjects.Contains(obj))
            {
                if (obj.LocationInWorld != "PlayerInventory")
                {
                    game.Output.WriteLine($"You don't appear to have a {obj.Name} to dispose of here in {game.Player.Location.Name}");
                }
                else
                {
                    if (obj.IsEquipped)
                    {
                        game.Output.WriteLine($"You'll need to unequip the {obj.Name} before you drop it here.");
                    }
                    else
                    {
                        game.Player.Inventory.Remove(obj);
                        obj.LocationInWorld = game.Player.Location.Name;
                        game.Output.WriteLine($"The {obj.Name} is now sitting here on the ground in {game.Player.Location.Name}.");
                    }
                    
                }
            }
            else
            {
                if (obj.Name == "Nothing")
                {
                    game.Output.WriteLine($"You can't drop nothing.");
                }
                else
                {
                    game.Output.WriteLine($"Whatever a \"{obj.Name}\" is, there isn't one here, or anywhere in this world, for that matter.");
                }
                
            }

        }

        public static void Equip(CommandContext commandContext)
        {
            Game game = commandContext.Game;
            string objName;
            if (commandContext.Subject != null)
            {
                objName = commandContext.Subject;
            }
            else
            {
                objName = "Nothing";
            }
            WorldObject obj = new WorldObject(objName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name.ToUpper() == obj.Name.ToUpper())
                {
                    obj = wObj;
                    break;
                }
            }

            if (game.World.WorldObjects.Contains(obj))
            {
                if (obj.LocationInWorld != "PlayerInventory")
                {
                    game.Output.WriteLine($"A {obj.Name} doesn't appear to be in your inventory");
                }
                else
                {
                    if (obj.IsEquippable)
                    {
                        if (!obj.IsEquipped)
                        {
                            obj.IsEquipped = true;
                            game.Output.WriteLine($"You place your {obj.Name} on your {obj.EquipLocation}.");
                        }
                        else
                        {
                            game.Output.WriteLine($"You've already got your {obj.Name} on your {obj.EquipLocation}!");
                        }
                    }
                    else
                    {
                        game.Output.WriteLine($"You can't just strap a {obj.Name} to your body!");
                    }

                }
            }
            else
            {
                if (obj.Name == "Nothing")
                {
                    game.Output.WriteLine($"You can't equip nothing.");
                }
                else
                {
                    game.Output.WriteLine($"Whatever a \"{obj.Name}\" is, there isn't one here, or anywhere in this world, for that matter.");
                }
            }

        }

        public static void Unequip(CommandContext commandContext)
        {
            Game game = commandContext.Game;
            string objName;
            if (commandContext.Subject != null)
            {
                objName = commandContext.Subject;
            }
            else
            {
                objName = "Nothing";
            }
            WorldObject obj = new WorldObject(objName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name.ToUpper() == obj.Name.ToUpper())
                {
                    obj = wObj;
                    break;
                }
            }

            if (game.World.WorldObjects.Contains(obj))
            {
                if (obj.LocationInWorld != "PlayerInventory")
                {
                    game.Output.WriteLine($"A {obj.LocationInWorld} doesn't appear to be in your inventory, much less on your person!");
                }
                else
                {
                    if (obj.IsEquipped)
                    {
                        if (game.Player.Location.EquipmentToEnter == obj.Name)
                        {
                            game.Output.WriteLine($"You can't remove your {obj.Name} here! You needed it to get here, and might get stuck if you lost it.");
                        }
                        else
                        {
                            obj.IsEquipped = false;
                            game.Output.WriteLine($"You return your {obj.Name} to your bag.");
                        }
                    }
                    else
                    {
                        game.Output.WriteLine($"Your {obj.Name} isn't equipped in the first place!");
                    }

                }
            }
            else
            {
                if (obj.Name == "Nothing")
                {
                    game.Output.WriteLine($"You can't unequip nothing.");
                }
                else
                {
                    game.Output.WriteLine($"Whatever a \"{obj.Name}\" is, there isn't one here, or anywhere in this world, for that matter.");
                }
            }

        }

        public static void Examine(CommandContext commandContext)
        {
            Game game = commandContext.Game;
            string objName;
            if (commandContext.Subject != null)
            {
                objName = commandContext.Subject;
            }
            else
            {
                objName = "Nothing";
            }
            WorldObject obj = new WorldObject(objName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name.ToUpper() == obj.Name.ToUpper())
                {
                    obj = wObj;
                    break;
                }
            }
            
            if (game.World.WorldObjects.Contains(obj))
            {
                if (obj.LocationInWorld == "PlayerInventory" || obj.LocationInWorld.ToUpper() == game.Player.Location.Name.ToUpper())
                {
                    //Give inventory desc
                    game.Output.WriteLine($"{obj.Name} - {obj.ExamineDescription}");
                }
                else
                {
                    game.Output.WriteLine($"There isn't a {obj.Name} here in {obj.LocationInWorld}, or in your inventory, for that matter.");
                }
            }
            else
            {
                if (obj.Name == "Nothing")
                {
                    game.Output.WriteLine($"You can't examine nothing.");
                }
                else
                {
                    game.Output.WriteLine($"Whatever a \"{obj.Name}\" is, there isn't one here, or anywhere in this world, for that matter.");
                }
            }

        }

        public static void Use(CommandContext commandContext)
        {
            Game game = commandContext.Game;
            string objOneName;
            if (commandContext.Subject != null)
            {
                objOneName = commandContext.Subject;
            }
            else
            {
                objOneName = "Nothing";
            }
            WorldObject objOne = new WorldObject(objOneName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name.ToUpper() == objOne.Name.ToUpper())
                {
                    objOne = wObj;
                    break;
                }
            }

            string objTwoName;
            if (commandContext.SubjectTwo != null)
            {
                objTwoName = commandContext.SubjectTwo;
            }
            else
            {
                objTwoName = "Nothing";
            }
            WorldObject objTwo = new WorldObject(objTwoName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name.ToUpper() == objTwo.Name.ToUpper())
                {
                    objTwo = wObj;
                    break;
                }
            }

            if (game.World.WorldObjects.Contains(objOne) && game.World.WorldObjects.Contains(objTwo))
            {
                if (objOne.LocationInWorld == "PlayerInventory" && objTwo.LocationInWorld.ToUpper() == game.Player.Location.Name.ToUpper())
                {
                    //If objOne is the correct object to use on objTwo, make something happen
                    if (objTwo.CorrectUseObjectName == null || objTwo.CorrectUseObjectName == "")
                    {
                        game.Output.WriteLine($"The {objTwo.Name} doesn't need anything used on it. Why don't you just take it?");
                    }
                    else
                    {
                        if (objOne.Name.ToUpper() == objTwo.CorrectUseObjectName.ToUpper())
                        {
                            WorldObject lootObj = new WorldObject(objTwo.RewardObjectName);
                            foreach (WorldObject wObj in game.World.WorldObjects)
                            {
                                if (wObj.Name.ToUpper() == lootObj.Name.ToUpper())
                                {
                                    lootObj = wObj;
                                    break;
                                }
                            }
                            game.Output.WriteLine($"You successfully use the {objOne.Name} on the {objTwo.Name}, revealing a {lootObj.Name} within.");
                            lootObj.LocationInWorld = game.Player.Location.Name;
                            Get(new CommandContext(game,$"get {lootObj.Name}", "GET",lootObj.Name));
                        }
                        else
                        {
                            game.Output.WriteLine($"The {objOne.Name} isn't the object to use on the {objTwo.Name}. Try examining the {objTwo.Name} again.");
                        }
                    }
                    
                }
                else
                {
                    game.Output.WriteLine($"You must use an object from your inventory on an object in the room.");

                }
            }
            else
            {
                if (objOne.Name == "Nothing" )
                {
                    game.Output.WriteLine($"You can't use nothing on something.");
                }
                else if (objTwo.Name == "Nothing")
                {
                    game.Output.WriteLine($"You can't use something on nothing.");
                }
                else
                {
                    game.Output.WriteLine("One of those objects isn't even in the world, much less this area or your inventory.");
                }
                
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) => Player = new Player(World, StartingLocation);
    }
}