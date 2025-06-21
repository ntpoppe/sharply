# Sharply

Sharply is a cross-platform chat application inspired by popular real-time communication platforms. It enables users to join servers and message others within channels.

---

## Getting Started

### Prerequisites
Ensure the following are installed on your system:
- [.NET SDK](https://dotnet.microsoft.com/) (9.0 or later)
- [.NET 9 Runtime for ASP.NET Core](https://dotnet.microsoft.com/)
- [SQLite](https://www.sqlite.org/) (Sharply uses SQLite for the database)

---

### Setting Up the Environment
#### Install dependencies:
```bash
dotnet tool install --global dotnet-ef

cd sharply/Sharply.Server
dotnet restore

cd ../Sharply.Client
dotnet restore
```

Sharply uses environment variables to manage configuration. Create the following `.env` files in the respective directories:

#### Backend (`Sharply.Server/.env`)
```env
ConnectionString__DefaultConnection=Data Source=sharply.db
ServerSettings__ServerUri=http://localhost:8000
ASPNETCORE_ENVIRONMENT=Development
JWT__Key=override_this_key_here
```
- **ConnectionString_DefaultConnection**: Defines the database connection string. Sharply uses SQLIte. To change the connection string, set this value to, for example, `Data Source=exampleDatabase.db`
- **ServerSettings_ServerUri**: The URI of the server backend. This should point where your server is hosted.
- **ASPNETCORE_ENVIRONMENT**: Environment of the server backend (development/production). The Docker Compose sets this to production.
- **JWT__Key**: A key used for signing and validating tokens. Must be at least 128 bits. Replace this with a randomly generated string.

#### Frontend (`Sharply.Client/.env`)
```env
ServerSettings__ServerUri=http://localhost:8000
```
- **ServerSettings__ServerUri**: Specifies the URI of the server that the client will connect to.
---

#### Creating the Database File
Navigate to the backend directory:
```bash
cd Sharply.Server
```
Run the following command to ensure the database file is created and initialized:
```bash
dotnet ef migrations add InitialCreate
```
Apply the migration to generate the database
```bash
dotnet ef database update
```

---

### Running the Application

#### Backend (Server)
Navigate to the backend directory and start the server:
```bash
cd Sharply.Server
dotnet run
```

#### Frontend (Client)
Navigate to the client directory and run the application:
```bash
cd Sharply.Client
dotnet run
```

---

### Docker

Sharply can be deployed using Docker for production environments. The application includes a multi-stage Dockerfile and docker-compose configuration for easy deployment.

#### Prerequisites
- [Docker](https://docs.docker.com/get-docker/)
- [Docker Compose](https://docs.docker.com/compose/install/)

#### Building and Running with Docker

##### Using Docker Compose (Recommended)
The easiest way to run Sharply in production is using the provided docker-compose.yml:

```bash
# Build and start the application
docker-compose up -d

# View logs
docker-compose logs -f

# Stop the application
docker-compose down
```

> **Important**: The `ServerSettings__ServerUri` in the docker-compose.yml file points to an environment variable called `DOCKER_SERVER_URI`. You should set this to match your own domain or server address.

#### Database Persistence

The SQLite database is stored in a Docker volume (`sharply-db`) to ensure data persistence across container restarts. The database file is located at `/data/sharply.db` inside the container.

Sharply uses the following libraries and frameworks:
- [Avalonia UI](https://avaloniaui.net/)
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)
- [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)










