#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["src/Services/Certificate/O2.Certificate.API/O2.Certificate.API.csproj", "src/Services/Certificate/O2.Certificate.API/"]
COPY ["src/Services/Certificate/O2. Certificate.Repositories/O2.Certificate.Repositories.csproj", "src/Services/Certificate/O2. Certificate.Repositories/"]
COPY ["src/Services/Certificate/Toolkit/O2 Black Toolkit/src/O2.Black.Toolkit.Core.Repositories/O2.Black.Toolkit.Core.Repositories.csproj", "src/Services/Certificate/Toolkit/O2 Black Toolkit/src/O2.Black.Toolkit.Core.Repositories/"]
COPY ["src/Services/Certificate/Toolkit/O2 Black Toolkit/src/O2.Black.Toolkit.Core.Data/O2.Black.Toolkit.Core.Data.csproj", "src/Services/Certificate/Toolkit/O2 Black Toolkit/src/O2.Black.Toolkit.Core.Data/"]
COPY ["src/Services/Certificate/Toolkit/O2 Black Toolkit/src/O2.Black.Toolkit.Core/O2.Black.Toolkit.Core.csproj", "src/Services/Certificate/Toolkit/O2 Black Toolkit/src/O2.Black.Toolkit.Core/"]
COPY ["src/Services/Certificate/O2. Certificate.Data/O2.Certificate.Data.csproj", "src/Services/Certificate/O2. Certificate.Data/"]
RUN dotnet restore "src/Services/Certificate/O2.Certificate.API/O2.Certificate.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Certificate/O2.Certificate.API"
RUN dotnet build "O2.Certificate.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "O2.Certificate.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "O2.Certificate.API.dll"]