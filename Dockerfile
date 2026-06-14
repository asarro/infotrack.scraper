FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Infotrack.Scraper/Infotrack.Scraper.csproj Infotrack.Scraper/
RUN dotnet restore Infotrack.Scraper/Infotrack.Scraper.csproj
COPY . .
WORKDIR /src/Infotrack.Scraper
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Infotrack.Scraper.dll"]
