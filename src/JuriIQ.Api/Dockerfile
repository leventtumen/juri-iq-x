FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["JuriIQ.Api/JuriIQ.Api.csproj", "JuriIQ.Api/"]
COPY ["JuriIQ.Core/JuriIQ.Core.csproj", "JuriIQ.Core/"]
COPY ["JuriIQ.Infrastructure/JuriIQ.Infrastructure.csproj", "JuriIQ.Infrastructure/"]
COPY ["JuriIQ.AI/JuriIQ.AI.csproj", "JuriIQ.AI/"]

# Restore dependencies
RUN dotnet restore "JuriIQ.Api/JuriIQ.Api.csproj"

# Copy all source files
COPY . .

# Build
WORKDIR "/src/JuriIQ.Api"
RUN dotnet build "JuriIQ.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JuriIQ.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create folders for documents
RUN mkdir -p /app/documents_to_process /app/documents_done /app/documents_failed /app/logs

ENTRYPOINT ["dotnet", "JuriIQ.Api.dll"]
