# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY Chinese_sale_api.csproj ./
RUN dotnet restore Chinese_sale_api.csproj

# Copy all source files and publish
COPY . ./
RUN dotnet publish Chinese_sale_api.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create logs directory
RUN mkdir -p /app/Logs

# Copy published files from build stage
COPY --from=build /app/publish .

# Expose port 5000
EXPOSE 5000

# Set entry point
ENTRYPOINT ["dotnet", "Chinese_sale_api.dll"]