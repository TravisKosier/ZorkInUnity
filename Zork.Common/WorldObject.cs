using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zork.Common
{
    public class WorldObject
    {
        [JsonProperty(Order = 1)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "WorldDescription",Order = 2)]
        public string WorldDescription { get; set; }

        [JsonProperty(PropertyName = "ExamineDescription", Order = 3)]
        public string ExamineDescription { get; set; }

        [JsonProperty(PropertyName = "LocationInWorld", Order = 4)]
        //private Dictionary<Room, string> LocationInWorldName { get; set; } = new Dictionary<Room, string>();
        public string LocationInWorld { get; set; }

        [JsonProperty(PropertyName = "IsTakeable", Order = 5)]
        public bool IsTakeable { get; set; }

        [JsonProperty(PropertyName = "IsEquippable", Order = 6)]
        public string IsEquippable { get; set; }

        [JsonIgnore]
        public bool IsEquipped { get; set; }

        [JsonProperty(PropertyName = "EquipLocation", Order = 7)]
        public string EquipLocation { get; set; }

        public WorldObject(string name = null)
        {
            Name = name;
            IsEquipped = false;
        }

        
    }
}
