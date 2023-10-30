#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MagPie-Ewelink-Proxy/MagPie-Ewelink-Proxy.csproj", "MagPie-Ewelink-Proxy/"]
RUN dotnet restore "MagPie-Ewelink-Proxy/MagPie-Ewelink-Proxy.csproj"
COPY . .
WORKDIR "/src/MagPie-Ewelink-Proxy"
RUN dotnet build "MagPie-Ewelink-Proxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MagPie-Ewelink-Proxy.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MagPie-Ewelink-Proxy.dll"]