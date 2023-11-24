# EduX

![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![Azure](https://img.shields.io/badge/azure-%230072C6.svg?style=for-the-badge&logo=microsoftazure&logoColor=white)
![MicrosoftSQLServer](https://img.shields.io/badge/Microsoft%20SQL%20Server-CC2927?style=for-the-badge&logo=microsoft%20sql%20server&logoColor=white)
![MongoDB](https://img.shields.io/badge/MongoDB-%234ea94b.svg?style=for-the-badge&logo=mongodb&logoColor=white)
![Redis](https://img.shields.io/badge/redis-%23DD0031.svg?style=for-the-badge&logo=redis&logoColor=white)
![Swagger](https://img.shields.io/badge/-Swagger-%23Clojure?style=for-the-badge&logo=swagger&logoColor=white)
![React](https://img.shields.io/badge/react-%2320232a.svg?style=for-the-badge&logo=react&logoColor=%2361DAFB)
![JavaScript](https://img.shields.io/badge/javascript-%23323330.svg?style=for-the-badge&logo=javascript&logoColor=%23F7DF1E)
![TypeScript](https://img.shields.io/badge/typescript-%23007ACC.svg?style=for-the-badge&logo=typescript&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-black?style=for-the-badge&logo=JSON%20web%20tokens)
![GitHub Actions](https://img.shields.io/badge/github%20actions-%232671E5.svg?style=for-the-badge&logo=githubactions&logoColor=white)

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
- `dotnet run -c Release` from Bootstrapper level
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

Health Checks of Liveness Probes - `http://localhost:4000/health-checks/live`

Health Checks of Readiness  Probes - `http://localhost:4000/health-checks/ready`

## Adding New DB Migrations
From module's directory level

- Creating Migration
`dotnet ef migrations add Users_Module_Init --startup-project ..\..\..\Bootstrapper\Edux.Bootstrapper\ -o ./EF/Migrations --context UsersWriteDbContext`

- Update Database
`dotnet ef database update --startup-project ..\..\..\Bootstrapper\Edux.Bootstrapper\ --context UsersWriteDbContext`

## GraphQL
All GraphQL requests go through this endpoint - `http://localhost:4000/graphql`

We can play with GraphQL via Playground UI under this endpoint - `http://localhost:4000/ui/playground`

Example of GraphQL query for Users Module:
`query {
  userMe {
    email
    id
    role
    createdAt
    updatedAt
    claims
  }
}`

Example of GraphQL subscription for Users Module:
`subscription {
  returnedUserMe {
    email
  }
}`

## gRPC
List of available gRPC services - `http://localhost:4000/grpc-endpoints`

List of Users Proto contract files - `http://localhost:4000/users-proto/users.proto`
