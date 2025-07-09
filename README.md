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

- **Domain**: Contains core business entities, domain logic, and domain services implementing the business rules and invariants.
- **Infrastructure.Data**: Implements data access, Entity Framework Core DbContext, repositories, and database migrations.
- **Infrastructure.IoC**: Handles dependency injection and service registrations.
- **Main**: Contains application-level services, validations (FluentValidation), and business workflows.
- **WebAPI**: The presentation layer, exposing RESTful endpoints via ASP.NET Core controllers, middleware, and API configuration.
