FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY src/MVDC.Shared/MVDC.Shared.csproj src/MVDC.Shared/
COPY src/MVDC.FullTime/MVDC.FullTime.csproj src/MVDC.FullTime/
COPY src/MVDC.Api/MVDC.Api.csproj src/MVDC.Api/
RUN dotnet restore src/MVDC.Api/MVDC.Api.csproj

# Copy source and publish
COPY src/MVDC.Shared/ src/MVDC.Shared/
COPY src/MVDC.FullTime/ src/MVDC.FullTime/
COPY src/MVDC.Api/ src/MVDC.Api/
RUN dotnet publish src/MVDC.Api/MVDC.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MVDC.Api.dll"]
