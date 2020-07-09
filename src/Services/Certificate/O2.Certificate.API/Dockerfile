#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Services/Certificate/O2.Certificate.API/O2.Certificate.API.csproj", "Services/Certificate/O2.Certificate.API/"]
COPY ["Services/Certificate/O2.Certificate.Repositories/O2.Certificate.Repositories.csproj", "Services/Certificate/O2.Certificate.Repositories/"]
COPY ["Services/Certificate/Toolkit/O2 Black Toolkit/O2.Black.Toolkit.Core.Repositories/O2.Black.Toolkit.Core.Repositories.csproj", "Services/Certificate/Toolkit/O2 Black Toolkit/O2.Black.Toolkit.Core.Repositories/"]
COPY ["Services/Certificate/Toolkit/O2 Black Toolkit/O2.Black.Toolkit.Core.Data/O2.Black.Toolkit.Core.Data.csproj", "Services/Certificate/Toolkit/O2 Black Toolkit/O2.Black.Toolkit.Core.Data/"]
COPY ["Services/Certificate/Toolkit/O2 Black Toolkit/O2.Black.Toolkit.Core/O2.Black.Toolkit.Core.csproj", "Services/Certificate/Toolkit/O2 Black Toolkit/O2.Black.Toolkit.Core/"]
COPY ["Services/Certificate/O2.Certificate.Data/O2.Certificate.Data.csproj", "Services/Certificate/O2.Certificate.Data/"]
RUN dotnet restore "Services/Certificate/O2.Certificate.API/O2.Certificate.API.csproj"
COPY . .
WORKDIR "/Services/Certificate/O2.Certificate.API"
RUN dotnet build "O2.Certificate.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "O2.Certificate.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "O2.Certificate.API.dll"]