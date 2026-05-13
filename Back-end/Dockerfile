# build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY src/UrlShortener.API/UrlShortener.API.csproj src/UrlShortener.API/
COPY src/App/App.csproj src/App/
COPY src/Modules/Shortening/Shortening.csproj src/Modules/Shortening/
COPY src/Modules/Analytics/Analytics.csproj src/Modules/Analytics/
COPY src/Modules/Identity/Identity.csproj src/Modules/Identity/

RUN dotnet restore src/UrlShortener.API/UrlShortener.API.csproj

# copy everything else and publish
COPY . ./
RUN dotnet publish src/UrlShortener.API/UrlShortener.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# non-root user
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY  --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT [ "dotnet", "UrlShortener.API.dll" ]