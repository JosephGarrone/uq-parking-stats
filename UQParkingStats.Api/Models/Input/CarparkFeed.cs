using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace UQParkingStats.Api.Models.Input
{
    class CarparkFeed
    {
        [JsonProperty("CarParks")]
        public Carpark[] Carparks { get; set; }

        [JsonProperty("LastUpdated")]
        public LastUpdated LastUpdated { get; set; }
    }
}
