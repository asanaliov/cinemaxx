FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Cinemaxx/Cinemaxx.csproj Cinemaxx/
RUN dotnet restore Cinemaxx/Cinemaxx.csproj

COPY Cinemaxx/ Cinemaxx/
RUN dotnet publish Cinemaxx/Cinemaxx.csproj \
    --configuration Release --no-restore --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

RUN mkdir -p /app/data 
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Cinemaxx.dll"]
