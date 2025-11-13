FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["JuriIQ.Scheduler/JuriIQ.Scheduler.csproj", "JuriIQ.Scheduler/"]
COPY ["JuriIQ.Core/JuriIQ.Core.csproj", "JuriIQ.Core/"]
COPY ["JuriIQ.Infrastructure/JuriIQ.Infrastructure.csproj", "JuriIQ.Infrastructure/"]
COPY ["JuriIQ.AI/JuriIQ.AI.csproj", "JuriIQ.AI/"]

# Restore dependencies
RUN dotnet restore "JuriIQ.Scheduler/JuriIQ.Scheduler.csproj"

# Copy all source files
COPY . .

# Build
WORKDIR "/src/JuriIQ.Scheduler"
RUN dotnet build "JuriIQ.Scheduler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JuriIQ.Scheduler.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create folders for documents
RUN mkdir -p /app/documents_to_process /app/documents_done /app/documents_failed /app/logs

ENTRYPOINT ["dotnet", "JuriIQ.Scheduler.dll"]
