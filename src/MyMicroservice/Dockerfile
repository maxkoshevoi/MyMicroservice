# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

# Restoring projects before copying source code to take advantage of caching
# If source code would change (but project file wouldn't) results from previous execution of restore steps will be used
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS restore
WORKDIR /src
COPY ["src/MyMicroservice.Common/MyMicroservice.Common.csproj", "src/MyMicroservice.Common/"]
RUN dotnet restore "src/MyMicroservice.Common/MyMicroservice.Common.csproj"
COPY ["src/MyMicroservice/MyMicroservice.csproj", "src/MyMicroservice/"]
RUN dotnet restore "src/MyMicroservice/MyMicroservice.csproj"
COPY ["tests/MyMicroservice.Tests/MyMicroservice.Tests.csproj", "tests/MyMicroservice.Tests/"]
RUN dotnet restore "tests/MyMicroservice.Tests/MyMicroservice.Tests.csproj"
COPY . .

# You can run Unit Test when image is being built.
# This won't affect debugging speed since Visual Studio only executes "base" stage in Debug mode
FROM restore AS test
WORKDIR /src/tests/MyMicroservice.Tests
RUN dotnet test

# It could be "restore AS publish", but then "test" stage wouldn't be executed
FROM test AS publish
WORKDIR /src/src/MyMicroservice
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyMicroservice.dll"]
