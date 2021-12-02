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
                { "QUIT", new Command("QUIT", new string[] { "QUIT", "Q", "BYE" }, Quit) },
                { "LOOK", new Command("LOOK", new string[] { "LOOK", "L" }, Look) },
                { "NORTH", new Command("NORTH", new string[] { "NORTH", "N" }, game => Move(game, Directions.North)) },
                { "SOUTH", new Command("SOUTH", new string[] { "SOUTH", "S" }, game => Move(game, Directions.South)) },
                { "EAST", new Command("EAST", new string[] { "EAST", "E"}, game => Move(game, Directions.East)) },
                { "WEST", new Command("WEST", new string[] { "WEST", "W" }, game => Move(game, Directions.West)) },
                { "REWARD", new Command("REWARD", new string[] { "REWARD", "R"}, game => Reward(game,5)) },
                { "SCORE", new Command("SCORE", new string[] { "SCORE"}, Score) },
                { "INVENTORY", new Command("INVENTORY", new string[] { "INVENTORY","I"}, Inventory) },
                { "EXAMINE X", new Command("EXAMINE X", new string[] { "EXAMINE X","E X"},game => Examine(game,"X")) },
                { "GET X", new Command("GET X", new string[] { "GET X","G X"}, game => Get(game,"X")) },
                { "DROP X", new Command("DROP X", new string[] { "DROP X","D X"}, game => Drop(game,"X")) },
                { "EQUIP X", new Command("EQUIP X", new string[] { "EQUIP X"}, game => Equip(game,"X")) }, 
                { "UNEQUIP X", new Command("EXAMINE X", new string[] { "UNEQUIP X"}, game => Unequip(game,"X")) },
                { "USE X ON Y", new Command("USE X ON Y", new string[] { "USE X ON Y"}, game => Use(game,"X","Y")) }
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
            Command foundCommand = null;
            foreach (Command command in Commands.Values)
            {
                if (command.Verbs.Contains(commandString))
                {
                    foundCommand = command;
                    break;
                }
            }

            if (foundCommand != null)
            {
                foundCommand.Action(this);
                Player.Moves++;
            }
            else
            {
                Output.WriteLine("Unknown command.");
            }
        }

        private static void Move(Game game, Directions direction)
        {
            if (game.Player.Move(direction) == false)
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


        public static void Look(Game game) => game.Output.WriteLine(game.Player.Location.Description);

        private static void Quit(Game game)
        {
            game.Player.HasQuit = true;
            game.IsRunning = false;
        }

        public static void Reward(Game game, int scoreAmt) => game.Player.Score = game.Player.Score + scoreAmt;

        public static void Score(Game game) => game.Output.WriteLine($"Your current score is: {game.Player.Score}");

        public static void Inventory(Game game)
        {
            game.Output.WriteLine("Your inventory contains:");
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

        public static void Get(Game game, string objName)
        {
            WorldObject obj = new WorldObject(objName);
            foreach(WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name == objName)
                {
                    obj = wObj;
                }
            }

            if (game.World.WorldObjects.Contains(obj))
            {
                if (obj.LocationInWorld != game.Player.Location.Name)
                {
                    game.Output.WriteLine($"A {obj.LocationInWorld} doesn't appear to be here in {game.Player.Location.Name}");
                }
                else
                {
                    game.Player.Inventory.Add(obj);
                    obj.LocationInWorld = "PlayerInventory";
                    Reward(game, obj.ScoreValue);
                    obj.ScoreValue = 0; //Remove score value so the player cannot farm score from picking up and dropping an item repeatedly

                }
            }
            else
            {
                game.Output.WriteLine($"Whatever a {obj.Name} is, there isn't one here, or anywhere in this world, for that matter.");
            }
            
        }

        public static void Drop(Game game, string objName)
        {
            WorldObject obj = new WorldObject(objName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name == objName)
                {
                    obj = wObj;
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
                    game.Player.Inventory.Remove(obj);
                    obj.LocationInWorld = "game.Player.Location.Name";
                }
            }
            else
            {
                game.Output.WriteLine($"Whatever a {obj.Name} is, there isn't one here, or anywhere in this world, for that matter.");
            }

        }

        public static void Equip(Game game, string objName)
        {
            WorldObject obj = new WorldObject(objName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name == objName)
                {
                    obj = wObj;
                }
            }

            if (game.World.WorldObjects.Contains(obj))
            {
                if (obj.LocationInWorld != "PlayerInventory")
                {
                    game.Output.WriteLine($"A {obj.LocationInWorld} doesn't appear to be in your inventory");
                }
                else
                {
                    if (obj.IsEquippable)
                    {
                        if (obj.IsEquipped)
                        {
                            obj.IsEquipped = true;
                            //Change object in inventory description?
                        }
                        else
                        {
                            game.Output.WriteLine($"Your {obj.Name} is already on your {obj.EquipLocation}!");
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
                game.Output.WriteLine($"Whatever a {obj.Name} is, there isn't in your inventory, or anywhere in this world, for that matter.");
            }

        }

        public static void Unequip(Game game, string objName)
        {
            WorldObject obj = new WorldObject(objName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name == objName)
                {
                    obj = wObj;
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
                        obj.IsEquipped = false;
                        //Change object in inventory description?
                    }
                    else
                    {
                        game.Output.WriteLine($"Your {obj.Name} isn't equipped in the first place!");
                    }

                }
            }
            else
            {
                game.Output.WriteLine($"Whatever a {obj.Name} is, there isn't in your inventory, or anywhere in this world, for that matter.");
            }

        }

        public static void Examine(Game game, string objName)
        {
            WorldObject obj = new WorldObject(objName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name == objName)
                {
                    obj = wObj;
                }
            }

            if (game.World.WorldObjects.Contains(obj))
            {
                if (obj.LocationInWorld == "PlayerInventory" || obj.LocationInWorld == game.Player.Location.Name)
                {
                    //Give inventory desc
                    game.Output.WriteLine($"{obj.Name} - {obj.ExamineDescription}");
                }
                else
                {
                    game.Output.WriteLine($"There isn't a {obj.Name} nearby, or in your inventory, for that matter.");
                }
            }
            else
            {
                game.Output.WriteLine($"Whatever a {obj.Name} is, there isn't in your inventory, or anywhere in this world, for that matter.");
            }

        }

        public static void Use(Game game, string objOneName, string objTwoName)
        {
            WorldObject objOne = new WorldObject(objOneName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name == objOneName)
                {
                    objOne = wObj;
                }
            }
            WorldObject objTwo = new WorldObject(objTwoName);
            foreach (WorldObject wObj in game.World.WorldObjects)
            {
                if (wObj.Name == objTwoName)
                {
                    objTwo = wObj;
                }
            }

            if(game.World.WorldObjects.Contains(objOne) && game.World.WorldObjects.Contains(objTwo))
            {
                if (objOne.LocationInWorld == "PlayerInventory" && objTwo.LocationInWorld == game.Player.Location.Name)
                {
                    //If objOne is the correct object to use on objTwo, make something happen
                    /*
                    if (objOne)
                    {

                    }
                    else
                    {
                        game.Output.WriteLine($"{objOne.Name} isn't the object to use on {objTwo.Name}. Try examining {objTwo.Name} again.");
                    }
                    */
                }
                else
                {
                    game.Output.WriteLine("You must use an object from your inventory on an object in the room.");
                }
            }
            else
            {
                game.Output.WriteLine("One of those objects isn't even in the world, much less this area or your inventory.");
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) => Player = new Player(World, StartingLocation);
    }
}