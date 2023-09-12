# Get Base Image for SDK stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy everything and build
COPY . .
RUN dotnet publish src/Bootstrapper/Edux.Bootstrapper -c Release -o out

# Generate runtime image stage (multi stage)
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URL http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT [ "dotnet", "Edux.Bootstrapper.dll" ]
