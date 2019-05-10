using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Lambda.Core;

namespace UQParkingStats.Api.Models
{
    public class Message
    {
        public bool Success { get; set; }
        public string Content { get; set; }
    }
}
