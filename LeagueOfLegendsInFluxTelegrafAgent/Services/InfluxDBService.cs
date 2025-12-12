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
    }
}
