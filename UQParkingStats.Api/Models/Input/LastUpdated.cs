using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace UQParkingStats.Api.Models.Input
{
    class LastUpdated
    {
        [JsonProperty("UnixEpoch")]
        public long UnixEpoch { get; set; }

        [JsonProperty("TimeOnly")]
        public string TimeOnly { get; set; }

        [JsonProperty("Friendly")]
        public string Friendly { get; set; }

        [JsonProperty("Full")]
        public string Full { get; set; }
    }
}
