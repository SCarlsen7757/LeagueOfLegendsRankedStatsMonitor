using InfluxDB3.Client.Write;
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

// Initialize InfluxDB (create org and bucket if needed)
var influxDbService = app.Services.GetRequiredService<InfluxDBService>();
await influxDbService.InitializeAsync();

// Set up event subscriptions for InfluxDB
var accountService = app.Services.GetRequiredService<AccountService>();
var leagueEntryService = app.Services.GetRequiredService<LeagueEntryService>();

var influxDbConfig = builder.Configuration.GetSection("InfluxDb");
var database = influxDbConfig["Database"] ?? "RankedStats";

// Subscribe to league entry data and write to InfluxDB
leagueEntryService.OnNewLeagueEntryData += async (player, leagueEntries) =>
{
    var point = PointData
        .Measurement("PlayerStats")
        .SetTag(nameof(LeagueEntryDTO.PuuId), leagueEntries.PuuId)
        .SetField("Player", player)
        .SetTag(nameof(LeagueEntryDTO.QueueType), leagueEntries.QueueType)
        .SetField(nameof(LeagueEntryDTO.LeaguePoints), leagueEntries.TotalLeaguePoints)
        .SetField(nameof(LeagueEntryDTO.Wins), leagueEntries.Wins)
        .SetField(nameof(LeagueEntryDTO.Losses), leagueEntries.Losses)
        .SetField(nameof(LeagueEntryDTO.TotalGames), leagueEntries.TotalGames)
        .SetField(nameof(LeagueEntryDTO.WinRate), leagueEntries.WinRate)
        .SetField(nameof(LeagueEntryDTO.HotStreak), leagueEntries.HotStreak)
        .SetField(nameof(LeagueEntryDTO.Tier), leagueEntries.Tier.ToString())
        .SetField(nameof(LeagueEntryDTO.Rank), leagueEntries.Rank)
        .SetTimestamp(DateTime.UtcNow);

    await influxDbService.WriteDataPointAsync(point, database);
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
