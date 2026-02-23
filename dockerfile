# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/Elisal.WasteManagement.sln", "./"]
COPY ["src/Elisal.WasteManagement.Api/Elisal.WasteManagement.Api.csproj", "Elisal.WasteManagement.Api/"]
COPY ["src/Elisal.WasteManagement.Application/Elisal.WasteManagement.Application.csproj", "Elisal.WasteManagement.Application/"]
COPY ["src/Elisal.WasteManagement.Domain/Elisal.WasteManagement.Domain.csproj", "Elisal.WasteManagement.Domain/"]
COPY ["src/Elisal.WasteManagement.Infrastructure/Elisal.WasteManagement.Infrastructure.csproj", "Elisal.WasteManagement.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Elisal.WasteManagement.Api/Elisal.WasteManagement.Api.csproj"

# Copy remaining source code
COPY ["src/Elisal.WasteManagement.Api/", "Elisal.WasteManagement.Api/"]
COPY ["src/Elisal.WasteManagement.Application/", "Elisal.WasteManagement.Application/"]
COPY ["src/Elisal.WasteManagement.Common/", "Elisal.WasteManagement.Domain/"]
COPY ["src/Elisal.WasteManagement.Infrastructure/", "Elisal.WasteManagement.Infrastructure/"]

# Build and publish
WORKDIR "/src/Elisal.WasteManagement.Api"
RUN dotnet build "Elisal.WasteManagement.Api.csproj" -c Release -o /app/build
RUN dotnet publish "Elisal.WasteManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser:appuser /app
USER appuser
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Elisal.WasteManagement.Api.dll"]
