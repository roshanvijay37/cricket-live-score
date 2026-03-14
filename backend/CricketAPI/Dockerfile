# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CricketAPI.csproj", "."]
RUN dotnet restore "./CricketAPI.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "CricketAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CricketAPI.csproj" -c Release -o /app/publish

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CricketAPI.dll"]