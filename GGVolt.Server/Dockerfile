# GGVolt.Server/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем только csproj для кеширования слоёв NuGet
COPY ["GGVolt.Server/GGVolt.Server.csproj", "GGVolt.Server/"]
COPY ["GGVolt.Core/GGVolt.Core.csproj", "GGVolt.Core/"]
COPY ["GGVolt.Infrastructure/GGVolt.Infrastructure.csproj", "GGVolt.Infrastructure/"]

RUN dotnet restore "GGVolt.Server/GGVolt.Server.csproj"

# Копируем весь исходный код
COPY . .
WORKDIR "/src/GGVolt.Server"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "GGVolt.Server.dll"]