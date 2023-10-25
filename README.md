# EduX

## Platform making learning easy. Which is still in progress and another modules will be added in the near future.

## Functionality
### Users Module
- Sign-Up
- Sign-In
- Check User Profile
- Get All Users
- Modify User
- Refresh Access Token
- Revoke Refresh Token

## Run the App
### Build App's Image
From the root directory:
`docker build -t edux .`

### Run/Stop App with Whole Infrastructure
From the ./edux/compose directory:
- `docker compose -f infrastructure.yml up -d`
- `docker compose -f infrastructure.yml down`

## Manual Testing
To test the REST API you can use the `EduX.http` file from the solution - `REST Client` in Visual Studio Code is required.

## Unit, Integration and Performance Testing
We can use NBomber to start performance tests.
This steps are required:
- `dotnet run -c Release (Bootstrapper)`
- `dotnet test`

The tests' report is available under this path - `EduX\tests\Modules\Users\Edux.Modules.Users.Tests.Performance\bin\Debug\net7.0\reports`

## OpenAPI Documentation
The OpenAPI specification is available under this endpoint - `http://localhost:4000/swagger/v1/swagger.json`

Swagger documentation is available under this endpoint - `http://localhost:4000/swagger/index.html`

ReDoc documentation is available under this endpoint - `http://localhost:4000/docs/index.html`

## App's Infrastructure
OpenTracing Metrics are available under this endpoint - `http://localhost:4000/metrics`

Logs in Seq are available under this endpoint - `http://localhost:5341`

Metrics in Prometheus are available under this endpoint - `http://localhost:9090`

Dashboards with Metrics from Prometheus are available in Grafana under this endpoint - `http://localhost:3000`

Vault Dashboards as a Secret Manager is available under this endpoint - `http://localhost:8200/ui/vault`

## Additional Endpoint
Bootstrapper Entry Point is available under this endpoint - `http://localhost:4000`

List of Enabled Modules is available under this endpoint - `http://localhost:4000/modules`

Changing Logging Level can be done via this endpoint - `POST http://localhost:4000/logging/level?level=information`

Redis Connection - `http://localhost:6379`

RabbitMQ UI - `http://localhost:15672`

MSSQLServer Connection - `http://localhost:1433`

## Adding New DB Migrations
From module's directory level

- Creating Migration
`dotnet ef migrations add Users_Module_Init --startup-project ..\..\..\Bootstrapper\Edux.Bootstrapper\ -o ./EF/Migrations --context UsersWriteDbContext`

- Update Database
`dotnet ef database update --startup-project ..\..\..\Bootstrapper\Edux.Bootstrapper\ --context UsersWriteDbContext`