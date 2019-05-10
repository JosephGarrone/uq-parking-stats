using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace UQParkingStats.Api.Models.Output
{
    public class DateData
    {
        [DynamoDBHashKey]
        public string Date { get; set; }
        public List<CarparkData> Carparks { get; set; }
    }
}
