using System.Collections.Generic;
using Newtonsoft.Json;

namespace UQParkingStats.Api.Models.Output
{
    public class CarparkData
    {
        public string Name { get; set; }
        public bool IsCasual { get; set; }
        public List<AvailabilityData> Data { get; set; }
    }
}
