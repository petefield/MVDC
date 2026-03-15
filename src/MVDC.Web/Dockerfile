FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY src/MVDC.Shared/MVDC.Shared.csproj src/MVDC.Shared/
COPY src/MVDC.Web/MVDC.Web.csproj src/MVDC.Web/
RUN dotnet restore src/MVDC.Web/MVDC.Web.csproj

# Copy source and publish
COPY src/MVDC.Shared/ src/MVDC.Shared/
COPY src/MVDC.Web/ src/MVDC.Web/
RUN dotnet publish src/MVDC.Web/MVDC.Web.csproj -c Release -o /app/publish --no-restore

FROM nginx:alpine AS runtime
# Copy published Blazor WASM static files
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/MVDC.Web/nginx.conf /etc/nginx/nginx.conf
COPY src/MVDC.Web/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

EXPOSE 80
ENTRYPOINT ["/docker-entrypoint.sh"]
