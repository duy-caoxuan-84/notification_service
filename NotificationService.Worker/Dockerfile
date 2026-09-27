FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["NotificationService.Worker/NotificationService.Worker.csproj", "NotificationService.Worker/"]
COPY ["NotificationService.Core/NotificationService.Core.csproj", "NotificationService.Core/"]
COPY ["NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "NotificationService.Infrastructure/"]
RUN dotnet restore "NotificationService.Worker/NotificationService.Worker.csproj"
COPY . .
WORKDIR "/src/NotificationService.Worker"
RUN dotnet build "NotificationService.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.Worker.dll"]
