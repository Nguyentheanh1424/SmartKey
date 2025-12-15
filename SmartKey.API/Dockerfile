FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SmartKey.sln ./
COPY SmartKey.API/SmartKey.API.csproj SmartKey.API/
COPY SmartKey.Application/SmartKey.Application.csproj SmartKey.Application/
COPY SmartKey.Infrastructure/SmartKey.Infrastructure.csproj SmartKey.Infrastructure/
COPY SmartKey.Domain/SmartKey.Domain.csproj SmartKey.Domain/

RUN dotnet restore SmartKey.API/SmartKey.API.csproj

COPY . .

RUN dotnet publish SmartKey.API/SmartKey.API.csproj -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT
