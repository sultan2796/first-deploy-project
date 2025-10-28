## Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer caching
COPY CSharpFirstApi.sln ./
COPY CSharpFirstApi/CSharpFirstApi.csproj CSharpFirstApi/
RUN dotnet restore "CSharpFirstApi/CSharpFirstApi.csproj"

# Copy the remaining source
COPY . .

# Build and publish
WORKDIR /src/CSharpFirstApi
RUN dotnet publish -c Release -o /app/publish --no-restore

## Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published output
COPY --from=build /app/publish .

# Set ASP.NET Core default ports (matches EXPOSE)
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "CSharpFirstApi.dll"]


