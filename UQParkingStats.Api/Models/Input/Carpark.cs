using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace UQParkingStats.Api.Models.Input
{
    public class Carpark
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("FriendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("NickName")]
        public string NickName { get; set; }

        [JsonProperty("BuildingNumber")]
        public string BuildingNumber { get; set; }

        [JsonProperty("Location")]
        public string Location { get; set; }

        [JsonProperty("isPermit")]
        public long IsPermit { get; set; }

        [JsonProperty("isCasual")]
        public long IsCasual { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("CurrentDisplay")]
        public string CurrentDisplay { get; set; }
    }
}
