FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["UserEventProcessor.csproj", "."]
RUN dotnet restore "./UserEventProcessor.csproj"

COPY . .

RUN dotnet publish "UserEventProcessor.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "UserEventProcessor.dll"]