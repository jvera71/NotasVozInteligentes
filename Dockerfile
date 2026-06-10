# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/NotasVozInteligentes/NotasVozInteligentes.csproj src/NotasVozInteligentes/
COPY src/NotasVozInteligentes.Client/NotasVozInteligentes.Client.csproj src/NotasVozInteligentes.Client/
RUN dotnet restore src/NotasVozInteligentes/NotasVozInteligentes.csproj

COPY src/. src/
RUN dotnet publish src/NotasVozInteligentes/NotasVozInteligentes.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

VOLUME ["/app/App_Data"]

ENTRYPOINT ["dotnet", "NotasVozInteligentes.dll"]
