FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 443
EXPOSE 80
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
COPY ["src/Services/Certificate/O2.Certificate.API/O2.Certificate.API.csproj", "src/Services/Certificate/O2.Certificate.API/O2.Certificate.API.csproj"]

# Publish the application
COPY . .
RUN dotnet restore "O2.sln"
WORKDIR /app/src/Services/Certificate/O2.Certificate.API
RUN dotnet publish --no-restore -c Release -o /app

# Build runtime image
FROM build AS publish
FROM base AS final

WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "O2.Certificate.API.dll"]