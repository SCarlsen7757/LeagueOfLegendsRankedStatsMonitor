using InfluxDB.Client.Writes;
using InfluxDbDataInsert.Dto;
using LeagueOfLegendsInFluxTelegrafAgent.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Options from appsettings.json
builder.Services.Configure<InfluxDBServiceOptions>(builder.Configuration.GetSection("InfluxDb"));
builder.Services.Configure<RiotGamesApiServiceOptions>(builder.Configuration.GetSection("RiotGamesApi"));
builder.Services.Configure<RiotGamesAccountServiceOptions>(builder.Configuration.GetSection("RiotGamesAccount"));
builder.Services.Configure<RiotGamesLeagueEntryServiceOptions>(builder.Configuration.GetSection("RiotGamesLeagueEntry"));

// Register Services
builder.Services.AddSingleton<InfluxDBService>();
builder.Services.AddSingleton<IRiotGamesApiService, RiotGamesApiService>();
builder.Services.AddSingleton<IRiotGamesAccountService, RiotGamesAccountService>();
builder.Services.AddSingleton<RiotGamesAccountService>();
builder.Services.AddSingleton<RiotGamesLeagueEntryService>();

// Register BackgroundServices
builder.Services.AddHostedService(sp => sp.GetRequiredService<RiotGamesAccountService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<RiotGamesLeagueEntryService>());

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Set up event subscriptions for InfluxDB
var influxDbService = app.Services.GetRequiredService<InfluxDBService>();
var accountService = app.Services.GetRequiredService<RiotGamesAccountService>();
var leagueEntryService = app.Services.GetRequiredService<RiotGamesLeagueEntryService>();

var influxDbConfig = builder.Configuration.GetSection("InfluxDb");
var bucket = influxDbConfig["Bucket"] ?? "LolStats";
var organization = influxDbConfig["Organization"] ?? "Lol";

// Subscribe to account data changes and write to InfluxDB
accountService.OnNewAccountData += async (accounts) =>
{
    foreach (var account in accounts.Values)
    {
        var point = PointData
            .Measurement("PlayerInformation")
            .Tag("puuid", account.PuuId)
            .Field("GameName", account.GameName)
            .Field("TagLine", account.TagLine)
            .Timestamp(DateTime.UtcNow, InfluxDB.Client.Api.Domain.WritePrecision.Ns);

        await influxDbService.WriteDataPointAsync(point, bucket, organization);
    }
};

// Subscribe to league entry data changes and write to InfluxDB
leagueEntryService.OnNewLeagueEntryData += async (leagueEntries) =>
{
    foreach (var entry in leagueEntries)
    {
        var point = PointData
            .Measurement("PlayerStats")
            .Tag("puuid", entry.PuuId)
            .Field("LeaguePoints", entry.TotalLeaguePoints)
            .Field("Wins", entry.Wins)
            .Field("Losses", entry.Losses)
            .Field("HotStreak", entry.HotStreak)
            .Timestamp(DateTime.UtcNow, InfluxDB.Client.Api.Domain.WritePrecision.Ns);

        await influxDbService.WriteDataPointAsync(point, bucket, organization);
    }
};

// Initialize accounts from InfluxDB on startup
var puuIds = await influxDbService.ReadPuuIdsAsync(bucket, organization);
foreach (var puuId in puuIds)
{
    accountService.Accounts.TryAdd(puuId, new AccountDto
    {
        PuuId = puuId,
        GameName = string.Empty,
        TagLine = string.Empty
    });
}

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
