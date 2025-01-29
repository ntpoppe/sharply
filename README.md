# Sharply

Sharply is a cross-platform chat application inspired by popular real-time communication platforms. It enables users to join servers and message others within channels.

---

## Getting Started

### Prerequisites
Ensure the following are installed on your system:
- [.NET SDK](https://dotnet.microsoft.com/) (9.0 or later)
- [SQLite](https://www.sqlite.org/) (Sharply uses SQLite for the database)

---

### Cloning the Repository
```bash
git clone https://github.com/poppent/sharply.git
cd sharply
```

---

### Setting Up the Environment
#### Install dependencies:
```bash
dotnet tool install --global dotnet-ef

cd Sharply.Server
dotnet restore

cd ../Sharply.Client
dotnet restore
```

Sharply uses environment variables to manage configuration. Create the following `.env` files in the respective directories:

#### Backend (`Sharply.Server/.env`)
```env
ConnectionString__DefaultConnection=Data Source=sharply.db
ServerSettings__ServerUri=http://localhost:8000
JWT__Key=override_this_key_here
```
- **ConnectionString_DefaultConnection**: Defines the database connection string. Sharply uses SQLIte. To change the connection string, set this value to, for example, `Data Source=exampleDatabase.db`
- **ServerSettings_ServerUri**: The URI of the server backend. This should point where your server is hosted.
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

-- 

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


Sharply uses the following libraries and frameworks:
- [Avalonia UI](https://avaloniaui.net/)
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)
- [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)










