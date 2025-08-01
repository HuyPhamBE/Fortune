# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy only project files and restore — this layer will be cached
COPY ./Fortune/Fortune/Fortune.csproj ./Fortune/Fortune/
RUN dotnet restore Fortune/Fortune/Fortune.csproj

COPY ./Fortune ./Fortune
# Build & publish
WORKDIR /app/Fortune/Fortune
RUN dotnet publish -c Release -o /out /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Fortune.dll"]
