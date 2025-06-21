# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj and restore only its dependencies first
COPY Sharply.Shared/*.csproj ./Sharply.Shared/
COPY Sharply.Server/*.csproj ./Sharply.Server/

RUN dotnet restore Sharply.Server/Sharply.Server.csproj

# copy everything else and publish
COPY . .
RUN dotnet publish Sharply.Server/Sharply.Server.csproj \
    -c Release -o /app/publish

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# copy the published output
COPY --from=build /app/publish .

# tell Kestrel to listen on 80 inside the container
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "Sharply.Server.dll"]

