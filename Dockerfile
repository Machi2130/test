# Use the official .NET 9.0 runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 9.0 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution file and restore dependencies
COPY ["testapp.sln", "."]
COPY ["testapp.Server/testapp.Server.csproj", "testapp.Server/"]
COPY ["testapp.Domain/testapp.Domain.csproj", "testapp.Domain/"]
COPY ["testapp.DAL/testapp.DAL.csproj", "testapp.DAL/"]
COPY ["testapp.client/testapp.client.esproj", "testapp.client/"]

RUN dotnet restore "testapp.sln"

# Copy everything else and build
COPY . .
WORKDIR "/src/testapp.Server"
RUN dotnet build "testapp.Server.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "testapp.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build the Angular application
FROM node:18 AS node-build
WORKDIR /src
COPY testapp.client/package*.json ./
RUN npm ci
COPY testapp.client/ ./
RUN npm run build --prod

# Final stage: copy published .NET app and built Angular app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=node-build /src/dist/testapp.client ./wwwroot

# Set the entry point
ENTRYPOINT ["dotnet", "testapp.Server.dll"]
