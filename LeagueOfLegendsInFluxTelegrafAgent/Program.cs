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
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21))));

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
var bucket = influxDbConfig["Bucket"] ?? "LolStats";
var organization = influxDbConfig["Organization"] ?? "Lol";

// Subscribe to league entry data changes and write to InfluxDB
leagueEntryService.OnNewLeagueEntryData += async (leagueEntries) =>
{
    foreach (var entry in leagueEntries)
    {
        var account = accountService.Accounts.ToList().FirstOrDefault(x => x.Key == entry.PuuId);

        var point = PointData
            .Measurement("PlayerStats")
            .Tag(nameof(LeagueEntryDTO.PuuId), entry.PuuId)
            .Tag("Player", $"{account.Value.GameName}#{account.Value.TagLine}")
            .Field(nameof(LeagueEntryDTO.LeaguePoints), entry.TotalLeaguePoints)
            .Field(nameof(LeagueEntryDTO.Wins), entry.Wins)
            .Field(nameof(LeagueEntryDTO.Losses), entry.Losses)
            .Field(nameof(LeagueEntryDTO.TotalGames), entry.TotalGames)
            .Field(nameof(LeagueEntryDTO.WinRate), entry.WinRate)
            .Field(nameof(LeagueEntryDTO.HotStreak), entry.HotStreak)
            .Field(nameof(LeagueEntryDTO.Tier), entry.Tier.ToString())
            .Field(nameof(LeagueEntryDTO.Rank), entry.Rank)
            .Timestamp(DateTime.UtcNow, InfluxDB.Client.Api.Domain.WritePrecision.Ns);

        await influxDbService.WriteDataPointAsync(point, bucket, organization);
    }
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
