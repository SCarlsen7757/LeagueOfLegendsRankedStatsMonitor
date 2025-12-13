using InfluxDB.Client.Writes;
using LeagueOfLegendsInFluxTelegrafAgent.Dto.RiotGames;
using LeagueOfLegendsInFluxTelegrafAgent.Services;
using LeagueOfLegendsInFluxTelegrafAgent.Services.Interfaces;
using LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames;
using LeagueOfLegendsInFluxTelegrafAgent.Services.RiotGames.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Options from appsettings.json
builder.Services.Configure<InfluxDBServiceOptions>(builder.Configuration.GetSection("InfluxDb"));
builder.Services.Configure<ApiServiceOptions>(builder.Configuration.GetSection("RiotGamesApi"));
builder.Services.Configure<AccountServiceOptions>(builder.Configuration.GetSection("RiotGamesAccount"));
builder.Services.Configure<LeagueEntryServiceOptions>(builder.Configuration.GetSection("RiotGamesLeagueEntry"));

builder.Services.Configure<MySQLServiceOptions>(builder.Configuration.GetSection("MySQL"));
var connectionString = builder.Configuration.GetSection("MySQL")["ConnectionString"];

// Register DbContext as a factory for singleton services
builder.Services.AddDbContextFactory<AccountDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddSingleton<MySQLService>();
builder.Services.AddSingleton<IAccountStorageService>(sp => sp.GetRequiredService<MySQLService>());

// Register Services
builder.Services.AddSingleton<InfluxDBService>();
builder.Services.AddSingleton<ApiService>();
builder.Services.AddSingleton<IApiService>(sp => sp.GetRequiredService<ApiService>());

builder.Services.AddSingleton<AccountService>();
builder.Services.AddSingleton<IAccountService>(sp => sp.GetRequiredService<AccountService>());

builder.Services.AddSingleton<LeagueEntryService>();
builder.Services.AddSingleton<ILeagueEntryService>(sp => sp.GetRequiredService<LeagueEntryService>());

// Register BackgroundServices
builder.Services.AddHostedService(sp => sp.GetRequiredService<AccountService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<LeagueEntryService>());

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Set up event subscriptions for InfluxDB
var influxDbService = app.Services.GetRequiredService<InfluxDBService>();
var accountService = app.Services.GetRequiredService<AccountService>();
var leagueEntryService = app.Services.GetRequiredService<LeagueEntryService>();

var influxDbConfig = builder.Configuration.GetSection("InfluxDb");
var bucket = influxDbConfig["Bucket"] ?? "RankedStats";
var organization = influxDbConfig["Organization"] ?? "LeagueOfLegends";

// Subscribe to league entry data and write to InfluxDB
leagueEntryService.OnNewLeagueEntryData += async (player, leagueEntries) =>
{
    var point = PointData
        .Measurement("PlayerStats")
        .Tag(nameof(LeagueEntryDTO.PuuId), leagueEntries.PuuId)
        .Tag("Player", player)
        .Field(nameof(LeagueEntryDTO.QueueType), leagueEntries.QueueType)
        .Field(nameof(LeagueEntryDTO.LeaguePoints), leagueEntries.TotalLeaguePoints)
        .Field(nameof(LeagueEntryDTO.Wins), leagueEntries.Wins)
        .Field(nameof(LeagueEntryDTO.Losses), leagueEntries.Losses)
        .Field(nameof(LeagueEntryDTO.TotalGames), leagueEntries.TotalGames)
        .Field(nameof(LeagueEntryDTO.WinRate), leagueEntries.WinRate)
        .Field(nameof(LeagueEntryDTO.HotStreak), leagueEntries.HotStreak)
        .Field(nameof(LeagueEntryDTO.Tier), leagueEntries.Tier.ToString())
        .Field(nameof(LeagueEntryDTO.Rank), leagueEntries.Rank)
        .Timestamp(DateTime.UtcNow, InfluxDB.Client.Api.Domain.WritePrecision.Ns);

    await influxDbService.WriteDataPointAsync(point, bucket, organization);
};

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
