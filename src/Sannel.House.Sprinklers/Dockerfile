#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG Major=0
ARG Minor=1
ARG Patch=0
ARG BuildNumber=0
WORKDIR /src
COPY ["calculateRID.sh", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/Sannel.House.Sprinklers/Sannel.House.Sprinklers.csproj", "src/Sannel.House.Sprinklers/"]
COPY ["src/Sannel.House.Sprinklers.Core/Sannel.House.Sprinklers.Core.csproj", "src/Sannel.House.Sprinklers.Core/"]
COPY ["src/Sannel.House.Sprinklers.Shared/Sannel.House.Sprinklers.Shared.csproj", "src/Sannel.House.Sprinklers.Shared/"]
COPY ["src/Sannel.House.Sprinklers.Infrastructure/Sannel.House.Sprinklers.Infrastructure.csproj", "src/Sannel.House.Sprinklers.Infrastructure/"]
RUN dotnet restore "./src/Sannel.House.Sprinklers/Sannel.House.Sprinklers.csproj" -r "`/src/calculateRID.sh`"
COPY . .
WORKDIR "/src/src/Sannel.House.Sprinklers"
RUN dotnet build "./Sannel.House.Sprinklers.csproj" -r "`/src/calculateRID.sh`" --no-restore -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG Major=0
ARG Minor=1
ARG Patch=0
ARG BuildNumber=0
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Sannel.House.Sprinklers.csproj" --no-restore -r "`/src/calculateRID.sh`" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sannel.House.Sprinklers.dll"]