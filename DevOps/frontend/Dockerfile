#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
FROM nginx AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TechChallenge.Web/TechChallenge.Web.csproj", "TechChallenge.Web/"]
COPY ["TechChallenge.Core/TechChallenge.Core.csproj", "TechChallenge.Core/"]
RUN dotnet restore "TechChallenge.Web/TechChallenge.Web.csproj"

COPY . .
RUN dotnet publish "TechChallenge.Web/TechChallenge.Web.csproj" -c Release -o /app/publish


FROM  base AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY  ./nginx.conf /etc/nginx/nginx.conf