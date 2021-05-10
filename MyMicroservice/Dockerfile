# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

# Restoring projects before copying source code to take advantage of caching
# If source code would change (but project file wouldn't) results from previous execution of restore steps will be used
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS restore
WORKDIR /src
COPY ["MyMicroservice.Common/MyMicroservice.Common.csproj", "MyMicroservice.Common/"]
RUN dotnet restore "MyMicroservice.Common/MyMicroservice.Common.csproj"
COPY ["MyMicroservice/MyMicroservice.csproj", "MyMicroservice/"]
RUN dotnet restore "MyMicroservice/MyMicroservice.csproj"
COPY ["MyMicroservice.Tests/MyMicroservice.Tests.csproj", "MyMicroservice.Tests/"]
RUN dotnet restore "MyMicroservice.Tests/MyMicroservice.Tests.csproj"
COPY . .

# You can run Unit Test when image is being built.
# This won't affect debugging speed since Visual Studio only executes "base" stage in Debug mode
FROM restore AS test
WORKDIR /src/MyMicroservice.Tests
RUN dotnet test

# It could be "restore AS publish", but then "test" stage wouldn't be executed
FROM test AS publish
WORKDIR /src/MyMicroservice
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyMicroservice.dll"]
