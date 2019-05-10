using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Util;
using Newtonsoft.Json;
using UQParkingStats.Api.Models;
using UQParkingStats.Api.Models.Input;
using UQParkingStats.Api.Models.Output;
using DynamoDBContextConfig = Amazon.DynamoDBv2.DataModel.DynamoDBContextConfig;
using JsonSerializer = Amazon.Lambda.Serialization.Json.JsonSerializer;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace UQParkingStats.Api
{
    public class Functions
    {
        private const string CarparkQueryStringName = "Carpark";
        private const string YearQueryStringName = "Year";
        private const string MonthQueryStringName = "Month";
        private const string DayQueryStringName = "Day";
        private const string ParkingDataTableNameEnvironmentKey = "ParkingDataTable";

        IDynamoDBContext DbContext { get; set; }

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            var parkingDataTable = Environment.GetEnvironmentVariable(ParkingDataTableNameEnvironmentKey);
            if (!string.IsNullOrEmpty(parkingDataTable))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(DateData)] = new TypeMapping(typeof(DateData), parkingDataTable);
            }

            DbContext = new DynamoDBContext(new AmazonDynamoDBClient(), new DynamoDBContextConfig
            {
                Conversion = DynamoDBEntryConversion.V2
            });
        }

        /// <summary>
        /// Scrapes the data and stores it
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> FetchData(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                context.Logger.LogLine("Fetch Data\n");

                CarparkFeed feed;
                using (var client = new HttpClient())
                {
                    var content = await client.GetStringAsync("https://pg.pf.uq.edu.au/feed");
                    feed = JsonConvert.DeserializeObject<CarparkFeed>(content);
                }

                DateTime now = DateTime.Now.AddHours(10); // Convert to AEST

                string key = $"{now:yyyy-MM-dd}";
                DateData data = await DbContext.LoadAsync<DateData>(key) ?? new DateData
                {
                    Date = key
                };

                if (data.Carparks == null)
                {
                    data.Carparks = new List<CarparkData>();
                }

                foreach (var carparkData in feed.Carparks)
                {
                    var carpark = data.Carparks.FirstOrDefault(park => park.Name == carparkData.Name.ToLower());

                    if (carpark == null)
                    {
                        carpark = new CarparkData
                        {
                            Name = carparkData.Name.ToLower(),
                            IsCasual = carparkData.IsCasual == 1,
                            Data = new List<AvailabilityData>()
                        };

                        data.Carparks.Add(carpark);
                    }

                    if (carpark.Data == null)
                    {
                        carpark.Data = new List<AvailabilityData>();
                    }

                    carpark.Data.Add(new AvailabilityData
                    {
                        AvailableParks = (int) carparkData.CurrentDisplay,
                        Timestamp = $"{now:s}"
                    });

                }

                await DbContext.SaveAsync(data);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = JsonConvert.SerializeObject(new Message
                    {
                        Content = "FetchData request succeeded.",
                        Success = true
                    }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
            catch (Exception ex)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Body = JsonConvert.SerializeObject(new Message
                    {
                        Content = $"FetchData request failed internally. Ex={ex}",
                        Success = false
                    }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
        }

        /// <summary>
        /// Get a list of all the carparks
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public APIGatewayProxyResponse GetCarparks(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Get Carparks\n");
            
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(new []
                {
                    new { Id = "P1", Name = "Warehouse" },
                    new { Id = "P2", Name = "Multi Level" },
                    new { Id = "P3", Name = "Multi Level B" },
                    new { Id = "P4", Name = "Multi Level A" },
                    new { Id = "P6", Name = "BSL Short Term" },
                    new { Id = "P7", Name = "Dustbowl" },
                    new { Id = "P8 L1", Name = "Boatshep Top" },
                    new { Id = "P8 L2", Name = "Boatshed Bottom" },
                    new { Id = "P9", Name = "Boatshed Open" },
                    new { Id = "P10", Name = "UQ Centre" },
                    new { Id = "P11 L1", Name = "Conifer L1" },
                    new { Id = "P11 L2", Name = "Conifer L2" },
                    new { Id = "P11 L3", Name = "Conifer L3" },
                    new { Id = "P12", Name = "Daycare" },

                }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            return response;
        }

        /// <summary>
        /// Get the data for the specified carpark and the specified date
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> GetData(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                context.Logger.LogLine("Get Data\n");

                foreach (string parameter in new[]
                {
                    CarparkQueryStringName, YearQueryStringName, MonthQueryStringName, DayQueryStringName
                })
                {
                    if (!request.PathParameters.ContainsKey(parameter))
                    {
                        context.Logger.LogLine($"GetData request did not specify a {parameter}.'\n");

                        return new APIGatewayProxyResponse
                        {
                            StatusCode = (int)HttpStatusCode.NotFound,
                            Body = JsonConvert.SerializeObject(new Message
                            {
                                Content = $"GetData request did not specify a {parameter}.",
                                Success = false
                            }),
                            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                        };
                    }
                }

                string carpark = request.PathParameters[CarparkQueryStringName];
                string year = request.PathParameters[YearQueryStringName];
                string month = request.PathParameters[MonthQueryStringName];
                string day = request.PathParameters[DayQueryStringName];

                DateData data = await DbContext.LoadAsync<DateData>($"{year}-{month}-{day}"); 

                switch (carpark.ToLower())
                {
                    case "all":
                        return new APIGatewayProxyResponse
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = JsonConvert.SerializeObject(data.Carparks
                                .GroupBy(park => 0)
                                .Select(group =>
                                {
                                    return new CarparkData
                                    {
                                        Name = "All",
                                        Data = group
                                            .SelectMany(avail => avail.Data)
                                            .GroupBy(avail => avail.Timestamp)
                                            .Select(avail =>
                                            {
                                                return new AvailabilityData
                                                {
                                                    Timestamp = avail.Key,
                                                    AvailableParks = avail.Sum(parks => parks.AvailableParks)
                                                };
                                            })
                                            .ToList()
                                    };  
                                })
                            ),
                            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                        };
                    case "casual":
                        return new APIGatewayProxyResponse
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = JsonConvert.SerializeObject(data.Carparks
                                .Where(park => park.IsCasual)
                                .GroupBy(park => 0)
                                .Select(group =>
                                {
                                    return new CarparkData
                                    {
                                        Name = "All",
                                        Data = group
                                            .SelectMany(avail => avail.Data)
                                            .GroupBy(avail => avail.Timestamp)
                                            .Select(avail =>
                                            {
                                                return new AvailabilityData
                                                {
                                                    Timestamp = avail.Key,
                                                    AvailableParks = avail.Sum(parks => parks.AvailableParks)
                                                };
                                            })
                                            .ToList()
                                    };
                                })
                            ),
                            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                        };
                    case "permit":
                        return new APIGatewayProxyResponse
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = JsonConvert.SerializeObject(data.Carparks
                                .Where(park => !park.IsCasual)
                                .GroupBy(park => 0)
                                .Select(group =>
                                {
                                    return new CarparkData
                                    {
                                        Name = "All",
                                        Data = group
                                            .SelectMany(avail => avail.Data)
                                            .GroupBy(avail => avail.Timestamp)
                                            .Select(avail =>
                                            {
                                                return new AvailabilityData
                                                {
                                                    Timestamp = avail.Key,
                                                    AvailableParks = avail.Sum(parks => parks.AvailableParks)
                                                };
                                            })
                                            .ToList()
                                    };
                                })
                            ),
                            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                        };
                    default:
                        return new APIGatewayProxyResponse
                        {
                            StatusCode = (int)HttpStatusCode.NotFound,
                            Body = JsonConvert.SerializeObject(data.Carparks.FirstOrDefault(park => park.Name == carpark)),
                            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                        };
                }
            }
            catch (Exception ex)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Body = JsonConvert.SerializeObject(new Message
                    {
                        Content = $"GetData request ({string.Join(", ", request.PathParameters.Select(param => $"{param.Key}: {param.Value}"))}) failed internally. Ex={ex}",
                        Success = false
                    }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }
        }
    }
}
