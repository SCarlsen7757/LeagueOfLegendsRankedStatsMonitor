using InfluxDB.Client.Writes;
using Microsoft.Extensions.Options;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services
{
    public class InfluxDBServiceOptions
    {
        public string? Url { get; set; }
        public string? Token { get; set; }
    }

    public class InfluxDBService
    {
        private readonly InfluxDBServiceOptions options;
        private readonly ILogger<InfluxDBService> logger;

        public InfluxDBService(IOptions<InfluxDBServiceOptions> options,
                               ILogger<InfluxDBService> logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }

        public async Task WriteDataPointAsync(PointData data, string bucket, string organization)
        {
            using var client = new InfluxDB.Client.InfluxDBClient(options.Url, options.Token);
            var writeApi = client.GetWriteApiAsync();

            await writeApi.WritePointAsync(data, bucket, organization);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Wrote data point to InfluxDB: {Data}", data.ToLineProtocol());
        }

        public async Task<IEnumerable<string>> ReadPuuIdsAsync(string bucket, string organization)
        {
            using var client = new InfluxDB.Client.InfluxDBClient(options.Url, options.Token);
            var queryApi = client.GetQueryApi();

            // Query recent account records and return distinct puuId values
            string fluxQuery = $@"
            from(bucket: ""{bucket}"")
            |> range(start: 0) // Adjust based on your time horizon if needed
            |> group(columns: [""puuid""]) 
            |> top(n: 1, columns: [""_time""])";

            // Query the database
            var fluxTables = await client.GetQueryApi().QueryAsync(fluxQuery, organization);

            var uniquePuuIds = new HashSet<string>();

            foreach (var table in fluxTables)
            {
                foreach (var record in table.Records)
                {
                    var puuid = record.Values["puuid"]?.ToString();
                    if (puuid != null && uniquePuuIds.Add(puuid))
                    {
                        // If added successfully (not duplicate), you can process further details here
                        var playerName = record.Values["_value"]?.ToString();
                        Console.WriteLine($"puuid: {puuid}, Player Name: {playerName}");
                    }
                }
            }

            return uniquePuuIds;
        }
    }
}
