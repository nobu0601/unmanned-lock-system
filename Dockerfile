# Stage 1: Build frontend apps
FROM node:18-alpine AS frontend-build

# Build LIFF app
WORKDIR /frontend/liff-app
COPY frontend/liff-app/package*.json ./
RUN npm ci
COPY frontend/liff-app/ ./
RUN npm run build && ls -la dist/

# Build Device app
WORKDIR /frontend/device-app
COPY frontend/device-app/package*.json ./
RUN npm ci
COPY frontend/device-app/ ./
RUN npm run build && ls -la dist/

# Build Admin app
WORKDIR /frontend/admin-app
COPY frontend/admin-app/package*.json ./
RUN npm ci
COPY frontend/admin-app/ ./
RUN npm run build && ls -la dist/

# Stage 2: Build backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /app
COPY backend/src/UnmannedLockSystem.Api/*.csproj ./
RUN dotnet restore
COPY backend/src/UnmannedLockSystem.Api/ ./
RUN dotnet publish -c Release -o /out

# Stage 3: Final runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=backend-build /out .

# Copy frontend builds to wwwroot subdirectories
COPY --from=frontend-build /frontend/liff-app/dist ./wwwroot/liff/
COPY --from=frontend-build /frontend/device-app/dist ./wwwroot/device/
COPY --from=frontend-build /frontend/admin-app/dist ./wwwroot/admin/

# Create a root index.html
RUN echo '<!DOCTYPE html><html><head><meta charset="utf-8"><title>Unmanned Lock System</title></head><body style="font-family:sans-serif;text-align:center;padding:60px"><h1>Unmanned Lock System</h1><ul style="list-style:none;padding:0"><li style="margin:16px"><a href="/liff/">Customer (LIFF)</a></li><li style="margin:16px"><a href="/admin/">Admin Dashboard</a></li><li style="margin:16px"><a href="/device/">Device Scanner</a></li><li style="margin:16px"><a href="/swagger/">API Docs</a></li></ul></body></html>' > ./wwwroot/index.html

# List wwwroot to verify files are there
RUN ls -laR ./wwwroot/ | head -40

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENTRYPOINT ["dotnet", "UnmannedLockSystem.Api.dll"]
