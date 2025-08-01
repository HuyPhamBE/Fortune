# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy entire project folder structure
COPY ./Fortune ./Fortune

# Restore
RUN dotnet restore Fortune/Fortune/Fortune.csproj

# Build & publish
WORKDIR /app/Fortune/Fortune
RUN dotnet publish -c Release -o /out /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Fortune.dll"]
