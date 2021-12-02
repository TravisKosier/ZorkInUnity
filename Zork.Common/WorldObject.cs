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

        [JsonProperty(PropertyName = "LocationInWorldName", Order = 4)]
        public string LocationInWorld { get; set; }

        [JsonProperty(PropertyName = "IsTakeable", Order = 5)]
        public bool IsTakeable { get; set; }

        [JsonProperty(PropertyName = "IsEquippable", Order = 6)]
        public bool IsEquippable { get; set; }

        [JsonIgnore]
        public bool IsEquipped { get; set; }

        [JsonProperty(PropertyName = "EquipLocation", Order = 7)]
        public string EquipLocation { get; set; }

        [JsonProperty(PropertyName = "ScoreValue", Order = 8)]
        public int ScoreValue { get; set; }

        [JsonProperty(PropertyName = "CorrectUseObject", Order = 9)]
        public string CorrectUseObjectName { get; set; }

        [JsonProperty(PropertyName = "RewardObject", Order = 10)]
        public string RewardObjectName { get; set; }

        public WorldObject(string name = null)
        {
            Name = name;
            IsEquipped = false;
        }

        
    }
}
