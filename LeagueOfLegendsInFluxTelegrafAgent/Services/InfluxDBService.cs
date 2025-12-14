using InfluxDB3.Client;
using InfluxDB3.Client.Write;
using Microsoft.Extensions.Options;

namespace LeagueOfLegendsInFluxTelegrafAgent.Services
{
    public class InfluxDBServiceOptions
    {
        public required string Url { get; set; }
        public required string Token { get; set; }
        public required string Database { get; set; }
    }

    public class InfluxDBService
    {
        private readonly InfluxDBServiceOptions options;
        private readonly ILogger<InfluxDBService> logger;
        private bool isInitialized = false;
        private readonly SemaphoreSlim initLock = new(1, 1);
        private InfluxDBClient Client { get; set; }

        public InfluxDBService(IOptions<InfluxDBServiceOptions> options,
                               ILogger<InfluxDBService> logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }

        public async Task InitializeAsync()
        {
            if (isInitialized)
                return;

            await initLock.WaitAsync();
            try
            {
                if (isInitialized)
                    return;

                // Retry logic for initial connection
                var maxRetries = 5;
                var retryDelay = TimeSpan.FromSeconds(2);

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                            logger.LogInformation("Attempting to connect to InfluxDB (attempt {Attempt}/{MaxRetries})...", attempt, maxRetries);

                        Client = new InfluxDBClient(options.Url, options.Token);

                        isInitialized = true;

                        if (logger.IsEnabled(LogLevel.Information))
                            logger.LogInformation("InfluxDB initialization completed successfully");
                        break;
                    }
                    catch (Exception ex) when (attempt < maxRetries)
                    {
                        if (logger.IsEnabled(LogLevel.Warning))
                            logger.LogWarning(ex, "Failed to connect to InfluxDB (attempt {Attempt}/{MaxRetries}). Retrying in {Delay} seconds...",
                                attempt, maxRetries, retryDelay.TotalSeconds);
                        await Task.Delay(retryDelay);
                        retryDelay = TimeSpan.FromSeconds(retryDelay.TotalSeconds * 2); // Exponential backoff
                    }
                    catch (Exception ex)
                    {
                        if (logger.IsEnabled(LogLevel.Error))
                            logger.LogError(ex, "Failed to initialize InfluxDB after {MaxRetries} attempts", maxRetries);
                        throw;
                    }
                }
            }
            finally
            {
                initLock.Release();
            }
        }

        public async Task WriteDataPointAsync(PointData data, string database)
        {
            await InitializeAsync();


            await Client.WritePointAsync(data, database);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Wrote data point to InfluxDB: {Data}", data.ToLineProtocol());
        }
    }
}
