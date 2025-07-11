# User Management API (.NET 8)

A RESTful Web API built with ASP.NET Core 9 for user registration, authentication, and role-based authorization.

---

## Tech Stack

- ASP.NET Core 9 Web API  
- Entity Framework Core with SQL Server  
- DDD (Domain-Driven Design)
- SOA (Service-Oriented Architecture)
- JWT Authentication  
- FluentValidation  
- Generic Repository and Unit of Work Pattern  
- Dependency Injection  
- Inversion Of Control

---

## Project Architecture

This project is organized into five main layers to separate concerns and promote maintainability:

- **Domain**: Contains core business entities, domain-specific exceptions, and abstractions such as repository and unit of work interfaces.
- **Infrastructure.Data**: Implements data access, Entity Framework Core DbContext, repositories, and database migrations.
- **Infrastructure.IoC**: Handles dependency injection and service registrations.
- **Main**: Contains application-level services, validations (FluentValidation), DTOs and business workflows.
- **WebAPI**: The presentation layer, exposing RESTful endpoints via ASP.NET Core controllers, middleware, and API configuration.


## How the Database Works

- The project uses **Entity Framework Core** as the ORM to interact with a SQL Server database.
- The database schema is managed through EF Core **migrations**, ensuring version control of the database structure.
- An initial **admin** is seeded into the database during startup using a configurable password from the application settings.
- Data access follows the **Repository** and **Unit of Work** patterns to encapsulate database operations and maintain transaction integrity.


## API Documentation

- The API exposes RESTful endpoints for user registration, authentication, and user management.
- Endpoints follow REST conventions with appropriate HTTP methods and status codes.
- Swagger for interactive API documentation
- You can explore and test the API using tools like Postman or curl with the following endpoints:

  - `POST /auth/register` — Register a new user (requires authorization).
  - `POST /auth/login` — Authenticate a user and receive a JWT token.
  - `GET /user` — Retrieve a list of users (requires authorization).
  - `GET /user/{id}` — Retrieve user details by ID (requires authorization).
  - `PUT /user/{id}` — Update user details (requires authorization).
  - `DELETE /user/{id}` — Delete a user (requires authorization).


## Authentication & Authorization

- The API uses **JWT (JSON Web Tokens)** for stateless authentication.
- Users register with a username and password via the `/auth/register` endpoint.
- Upon successful login at `/auth/login`, the API returns a JWT token.
- This token must be included in the `Authorization` header as a Bearer token for protected endpoints.
- Role-based authorization is implemented to restrict access:
  - Users have roles such as `Admin` and `User`.
  - Certain endpoints (like user management) require an `Admin` role.
- Unauthorized requests or requests with invalid tokens receive appropriate HTTP status codes (`401 Unauthorized` or `403 Forbidden`).


## Configuration

### Local Development Configuration

- `appsettings.Development.json` is **excluded from source control** via `.gitignore`.
- This file should be created manually on each developer’s machine.
- It contains **sensitive values** like actual connection strings, JWT secrets, and admin passwords that should not be shared or committed.

#### Example `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=UserManagementDB;Trusted_Connection=True;Encrypt=False"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyOfAtLeast32Characters!"
  },
  "SeedAdmin": {
    "Password": "Admin@123"
  }
}
```

## Unit Testing

This project includes comprehensive unit tests covering core services and domain models to ensure correctness and maintainability.

### Testing Frameworks & Tools

- **xUnit**: Primary testing framework.
- **Moq**: Mocking dependencies like repositories and unit of work.


