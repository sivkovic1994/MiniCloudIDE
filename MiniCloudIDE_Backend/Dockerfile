# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY MiniCloudIDE_Backend.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
RUN apt-get update && \
    apt-get install -y --no-install-recommends python3 && \
    ln -s /usr/bin/python3 /usr/bin/python && \
    rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
COPY worker.py .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "MiniCloudIDE_Backend.dll"]
