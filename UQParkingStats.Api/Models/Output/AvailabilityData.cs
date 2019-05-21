using System;
using Newtonsoft.Json;

namespace UQParkingStats.Api.Models.Output
{
    public class AvailabilityData
    {
        public int AvailableParks { get; set; }
        public string Timestamp { get; set; }
    }
}
