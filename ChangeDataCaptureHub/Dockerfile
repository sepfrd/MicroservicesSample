# Build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /source

COPY . .

RUN dotnet restore ./ChangeDataCaptureHub.Web/ChangeDataCaptureHub.Web.csproj --disable-parallel

RUN dotnet publish ./ChangeDataCaptureHub.Web/ChangeDataCaptureHub.Web.csproj -o /publish --no-restore

# Serve

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /publish ./

ENTRYPOINT ["dotnet", "ChangeDataCaptureHub.Web.dll"]
