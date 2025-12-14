# League of Legends Statistics Monitor

A .NET 10 application that monitors League of Legends ranked statistics using the Riot Games API, stores data in InfluxDB and MySQL, and visualizes it with Grafana.

## Overview

This project tracks League of Legends player ranked statistics over time, including:
- League Points (LP)
- Win/Loss records
- Win rate
- Tier and rank
- Hot streak status

The application periodically fetches data from the Riot Games API, stores it in InfluxDB for time-series data and MySQL for account information, and provides a REST API for managing tracked accounts.

## Architecture

- **Backend**: .NET 10 ASP.NET Core Web API
- **Time-Series Database**: InfluxDB 3 (for player statistics)
- **Relational Database**: MySQL (for account storage)
- **Visualization**: Grafana
- **Containerization**: Docker & Docker Compose

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or Docker Engine with Docker Compose
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (for local development)
- [Riot Games API Key](https://developer.riotgames.com/) (free tier available)

## Project Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd LeagueOfLegendsStatMonitor
```

### 2. Configure Environment Variables

Create or modify the `.env` file in the root directory with your configuration:

```env
# MySQL Configuration
MYSQL_USERNAME=lolstats
MYSQL_PASSWORD=lolstats123
MYSQL_ROOT_PASSWORD=rootpassword123
MYSQL_DATABASE=RiotGames

# InfluxDB Configuration
# Note: InfluxDB runs without authentication (START_WITHOUT_AUTH enabled)
INFLUXDB_DATABASE=RankedStats

# Grafana Configuration
GRAFANA_USERNAME=admin
GRAFANA_PASSWORD=admin

# Riot Games API Configuration
RIOT_API_TOKEN=<your-riot-api-key>

# Refresh Intervals (optional)
RIOT_ACCOUNT_REFRESH_INTERVAL=24:00:00
RIOT_LEAGUE_ENTRY_REFRESH_INTERVAL=00:10:00
```

**Important**: 
- Replace `<your-riot-api-key>` with your actual Riot Games API key from https://developer.riotgames.com/
- **InfluxDB Authentication**: InfluxDB runs without authentication. The `INFLUXDB3_START_WITHOUT_AUTH=true` flag is set in `docker-compose.yml`. No token is required for local development.

### 3. Start the Application

Using Docker Compose (recommended):

```bash
docker-compose up -d
```

This will start all services:
- **LeagueOfLegendsInFluxTelegrafAgent**: Available at http://localhost:5000 (API)
- **MySQL**: Available at http://localhost:8088
- **InfluxDB**: Available at http://localhost:8086
- **Grafana**: Available at http://localhost:8087

### 4. Verify Services are Running

```bash
docker-compose ps
```

All services should show as "Up" or "running".

### 5. Access the Application

- **Swagger API Documentation**: http://localhost:5000/swagger
- **InfluxDB UI**: http://localhost:8086
- **Grafana**: http://localhost:8087
  - Username: `admin` (from GRAFANA_USERNAME)
  - Password: `admin` (from GRAFANA_PASSWORD)

## MySQL Database Setup

### Initial Database Connection

The application automatically creates the necessary database schema when it starts. However, you may want to create additional users for Grafana or other tools.

### Grafana User Configuration

To allow Grafana to query MySQL data, create a read-only user:

1. Connect to the MySQL container:

```bash
docker exec -it <mysql-container-name> mysql -u root -p
```

Enter the root password (default: `rootpassword123` from `.env`)

2. Create the Grafana user and grant permissions:

```sql
CREATE USER 'grafana_user'@'%' IDENTIFIED BY 'grafana_password';
GRANT SELECT ON RiotGames.* TO 'grafana_user'@'%';
FLUSH PRIVILEGES;
```

3. Exit MySQL:

```sql
EXIT;
```

## InfluxDB Configuration in Grafana

### Add InfluxDB as a Data Source

1. Open Grafana at http://localhost:8087
2. Navigate to **Connections** > **Data sources** (or **Configuration** > **Data Sources** in older versions)
3. Click **+ Add new data source**
4. Select **InfluxDB** from the list
5. Configure with the following settings:
   - **Name**: `InfluxDB` (or your preferred name)
   - **Query Language**: `InfluxQL`
   - **URL**: `http://influxdb:8181`
   - **Database**: `RankedStats`
6. Click **Save & Test**

You should see a green "datasource is working" message.

## MySQL Configuration in Grafana

### Add MySQL as a Data Source (optional)

1. Open Grafana at http://localhost:8087
2. Navigate to **Connections** > **Data sources**
3. Click **+ Add new data source**
4. Select **MySQL**
5. Configure with the following settings:
   - **Host**: `mysql:3306`
   - **Database**: `RiotGames`
   - **User**: `grafana_user` (or `root` for quick testing)
   - **Password**: `grafana_password` (or the root password)
6. Click **Save & Test**

## Usage

### Adding Players to Track

Use the API to add League of Legends accounts to track:

**POST** `/api/accounts`

```json
{
  "gameName": "PlayerName",
  "tagLine": "1234", // e.g., "EUW", "NA", etc.
  "region": "EUW1"
}
```

Example using curl:

```bash
curl -X POST "http://localhost:5000/api/accounts" \
  -H "Content-Type: application/json" \
  -d '{
    "gameName": "PlayerName",
    "tagLine": "EUW",
    "region": "2"
  }'
```

### Available Regions

- `Americas`
- `Asia`
- `Europe`
- `Sea`

### Data Refresh Intervals

The application automatically refreshes data at configurable intervals:

- **Account Data**: Default every 24 hours (configurable via `RIOT_ACCOUNT_REFRESH_INTERVAL`)
- **League Entry Data**: Default every 15 minutes (configurable via `RIOT_LEAGUE_ENTRY_REFRESH_INTERVAL`)

Adjust these in your `.env` file using the format `HH:MM:SS`.

## Configuration Files

### appsettings.json

For local development without Docker, configure `LeagueOfLegendsInFluxTelegrafAgent/appsettings.json`:

```json
{
  "MySQL": {
    "ConnectionString": "Server=localhost;Port=3306;Database=RiotGames;User=root;Password=yourpassword;"
  },
  "InfluxDb": {
    "Url": "http://localhost:8181/",
    "Database": "RankedStats",
    "Token": ""
  },
  "RiotGamesApi": {
    "Token": "YourRiotApiTokenHere",
    "Region": "Europe"
  },
  "RiotGamesAccount": {
    "RefreshInterval": "24:00:00"
  },
  "RiotGamesLeagueEntry": {
    "RefreshInterval": "00:30:00"
  }
}
```

## Development

### Running Locally

1. Ensure MySQL and InfluxDB are running (via Docker or locally)
2. Update `appsettings.json` with your local configuration
3. Run the application:

```bash
cd LeagueOfLegendsInFluxTelegrafAgent
dotnet run
```

### Building the Docker Image

```bash
docker build -t leagueoflegendsinfluxtelegrafagent ./LeagueOfLegendsInFluxTelegrafAgent
```

## License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details.