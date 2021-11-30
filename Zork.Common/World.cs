using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Zork.Common
{
    public class World : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<Room> Rooms { get; set; }
        public List<WorldObject> WorldObjects { get; set; }

        [JsonIgnore]
        public IReadOnlyDictionary<string, Room> RoomsByName => _roomsByName;
        [JsonIgnore]
        public IReadOnlyDictionary<string, WorldObject> WorldObjectsByName => _worldObjectsByName;

        public World()
        {
            Rooms = new List<Room>();
            _roomsByName = new Dictionary<string, Room>();
            WorldObjects = new List<WorldObject>();
            _worldObjectsByName = new Dictionary<string, WorldObject>();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            _roomsByName = Rooms.ToDictionary(room => room.Name, room => room);
            _worldObjectsByName = WorldObjects.ToDictionary(worldObject => worldObject.Name, worldObject => worldObject);

            foreach (Room room in Rooms)
            {
                room.UpdateNeighbors(this);
            }
            //foreach (WorldObject worldObject in WorldObjects)
            //{
            //    worldObject.PlaceInWorld();
            //}
        } 

        private Dictionary<string, Room> _roomsByName;
        private Dictionary<string, WorldObject> _worldObjectsByName;
    }
}