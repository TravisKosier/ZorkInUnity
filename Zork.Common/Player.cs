﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Zork.Common
{
    public class Player
    {

        public event EventHandler<Room> LocationChanged;
        public event EventHandler<int> MovesChanged;
        public event EventHandler<int> ScoreChanged;
        public event EventHandler<bool> HasQuitChanged;

        public World World { get; }

        [JsonIgnore]
        public Room Location
        {
            get
            {
                return _location;
            }
            private set
            {
                if (_location != value)
                {
                    _location = value;
                    LocationChanged?.Invoke(this, _location);
                }
            }
        }

        public int Score 
        {
            get
            {
                return _score;
            }
            set
            {
                if (_score != value)
                {
                    _score = value;
                    ScoreChanged?.Invoke(this, _score);
                }
            }
        }

        public int Moves 
        {
            get
            {
                return _moves;
            }
            set
            {
                if (_moves != value)
                {
                    _moves = value;
                    MovesChanged?.Invoke(this, _moves);
                }
            }
        }

        public bool HasQuit
        {
            get
            {
                return _hasQuit;
            }
            set
            {
                if (_hasQuit != value)
                {
                    _hasQuit = value;
                    HasQuitChanged?.Invoke(this, _hasQuit);
                }
            }
        }

        public List<WorldObject> Inventory
        {
            get
            {
                return _inventory;
            }
            set
            {
                if (_inventory != value)
                {
                    _inventory = value;
                }
            }
        }

        public Player(World world, string startingLocation)
        {
            Assert.IsTrue(world != null);
            Assert.IsTrue(world.RoomsByName.ContainsKey(startingLocation));

            World = world;
            Location = world.RoomsByName[startingLocation];
            Inventory = new List<WorldObject>();
        }

        public bool Move(CommandContext commandContext, Directions direction)
        {
            Game game = commandContext.Game;
            bool isValidMove = Location.Neighbors.TryGetValue(direction, out Room destination);
            if (isValidMove)
            {
                if (destination?.EquipmentToEnter != "")
                {
                    foreach (WorldObject wObj in game.World.WorldObjects)
                    {
                        if (wObj.Name == destination.EquipmentToEnter && wObj.IsEquipped == true)
                        {
                            isValidMove = true;
                            break;
                        }
                        else
                        {
                            isValidMove = false;
                        }
                    }
                    if (!isValidMove)
                    {
                        game.Output.WriteLine(destination.NoEquipmentMessage);
                    }
                    else
                    {
                        game.Output.WriteLine($"With the aid of your {destination.EquipmentToEnter}, you are able to proceed into {destination.Name}");
                        Location = destination;
                    }
                }
                else
                {
                    Location = destination;
                }
                
            }

            return isValidMove;
        }

        private Room _location;
        private int _score;
        private int _moves;
        private bool _hasQuit = false;
        private List<WorldObject> _inventory;
    }
}