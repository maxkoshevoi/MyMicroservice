# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

# Restoring projects before copying source code to take advantage of caching
# If source code would change (but project file wouldn't) results from previous execution of restore steps will be used
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS publish
WORKDIR /src
COPY ["src/MyMicroservice.Common/MyMicroservice.Common.csproj", "src/MyMicroservice.Common/"]
RUN dotnet restore "src/MyMicroservice.Common/MyMicroservice.Common.csproj"
COPY ["src/HealthCheckUI/HealthCheckUI.csproj", "src/HealthCheckUI/"]
RUN dotnet restore "src/HealthCheckUI/HealthCheckUI.csproj"
COPY . .
RUN dotnet publish "src/HealthCheckUI/HealthCheckUI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HealthCheckUI.dll"]