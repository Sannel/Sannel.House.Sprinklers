#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Sannel.House.Sprinklers/Sannel.House.Sprinklers.csproj", "src/Sannel.House.Sprinklers/"]
COPY ["src/Sannel.House.Sprinklers.Core/Sannel.House.Sprinklers.Core.csproj", "src/Sannel.House.Sprinklers.Core/"]
COPY ["src/Sannel.House.Sprinklers.Shared/Sannel.House.Sprinklers.Shared.csproj", "src/Sannel.House.Sprinklers.Shared/"]
COPY ["src/Sannel.House.Sprinklers.Infrastructure/Sannel.House.Sprinklers.Infrastructure.csproj", "src/Sannel.House.Sprinklers.Infrastructure/"]
COPY "calculateRID.sh" .
RUN chmod +x calculateRID.sh \
	&& ./calculateRID.sh
RUN dotnet restore -r $RID "./src/Sannel.House.Sprinklers/Sannel.House.Sprinklers.csproj"
COPY . .
WORKDIR "/src/src/Sannel.House.Sprinklers"
RUN dotnet build -r $RID "./Sannel.House.Sprinklers.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN ./calculateRID.sh \
	&& rm calculateRID.sh
RUN dotnet publish -r $RID "./Sannel.House.Sprinklers.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sannel.House.Sprinklers.dll"]