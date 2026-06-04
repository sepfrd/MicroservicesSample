# Build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /source

COPY . .

RUN dotnet restore "./ToDoListManager.Web/ToDoListManager.Web.csproj" --disable-parallel

RUN dotnet publish "./ToDoListManager.Web/ToDoListManager.Web.csproj" -o /app --no-restore

# Serve

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["dotnet", "ToDoListManager.Web.dll"]
