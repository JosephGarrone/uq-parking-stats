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
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public APIGatewayProxyResponse Result(HttpStatusCode code, object data)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)code,
                Body = JsonConvert.SerializeObject(data),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Headers", "Content-Type" },
                    { "Access-Control-Allow-Methods", "OPTIONS,POST,GET" },
                }
            };
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

                    bool converted = int.TryParse(carparkData.CurrentDisplay, out int avail);

                    carpark.Data.Add(new AvailabilityData
                    {
                        AvailableParks = converted ? avail : 0,
                        Timestamp = $"{now:s}"
                    });

                }

                await DbContext.SaveAsync(data);

                return Result(HttpStatusCode.OK, new Message
                {
                    Content = "FetchData request succeeded.",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Fetch Data. Ex={ex}\n");

                return Result(HttpStatusCode.NotFound, new Message
                {
                    Content = $"FetchData request failed internally. Ex={ex}",
                    Success = false
                });
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

            return Result(HttpStatusCode.OK, new[]
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
                new { Id = "P12", Name = "Daycare" }
            }); 
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

                        return Result(HttpStatusCode.OK, new Message
                        {
                            Content = $"GetData request did not specify a {parameter}.",
                            Success = false
                        });
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
                        return Result(HttpStatusCode.OK, data.Carparks
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
                            }));
                    case "casual":
                        return Result(HttpStatusCode.OK, data.Carparks
                            .Where(park => park.IsCasual)
                            .GroupBy(park => 0)
                            .Select(group =>
                            {
                                return new CarparkData
                                {
                                    Name = "Casual",
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
                            }));
                    case "permit":
                        return Result(HttpStatusCode.OK, data.Carparks
                            .Where(park => !park.IsCasual)
                            .GroupBy(park => 0)
                            .Select(group =>
                            {
                                return new CarparkData
                                {
                                    Name = "Permit",
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
                            }));
                    default:
                        return Result(HttpStatusCode.OK, data.Carparks.FirstOrDefault(park => park.Name == carpark));
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Get Data. Ex={ex}\n");
                return Result(HttpStatusCode.NotFound, new Message
                {
                    Content =
                        $"Get Data request ({string.Join(", ", request.PathParameters.Select(param => $"{param.Key}: {param.Value}"))}) failed internally. Ex={ex}",
                    Success = false
                });
            }
        }
    }
}
