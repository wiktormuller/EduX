# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish src/Bootstrapper/Edux/Bootstrapper -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URL http://*:5000
ENTRYPOINT [ "dotnet", "Edux.Bootstrapper.dll" ]
