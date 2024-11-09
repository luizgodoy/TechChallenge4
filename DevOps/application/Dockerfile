#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
ENV ASPNETCORE_ENVIRONMENT=Development
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TechChallenge.API/TechChallenge.API.csproj", "TechChallenge.API/"]
COPY ["TechChallenge.Application/TechChallenge.Application.csproj", "TechChallenge.Application/"]
COPY ["TechChallenge.Infrastructure/TechChallenge.Data.csproj", "TechChallenge.Infrastructure/"]
COPY ["TechChallenge.Core/TechChallenge.Core.csproj", "TechChallenge.Core/"]
COPY ["TechChallenge.Domain/TechChallenge.Domain.csproj", "TechChallenge.Domain/"]
RUN dotnet restore "./TechChallenge.Application/TechChallenge.Application.csproj"
COPY . .
WORKDIR "/src/TechChallenge.Application"
RUN dotnet build "./TechChallenge.Application.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TechChallenge.Application.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TechChallenge.Application.dll"]