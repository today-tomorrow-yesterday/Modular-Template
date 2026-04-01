# Application Layer

The **Application Layer** orchestrates the flow of data and coordinates domain objects to perform use cases. It contains application-specific business rules but no enterprise business logic.

## What Belongs Here

- **Use Cases / Application Services** - Orchestrate domain objects to fulfill user requests
- **Commands & Queries** - CQRS pattern implementations
- **DTOs** - Data transfer objects for crossing boundaries
- **Interfaces** - Abstractions for external services (email, storage, etc.)
- **Validation** - Input validation for commands and queries

## Key Principle

> "The Application Layer directs the expressive domain objects to work out problems. The tasks this layer is responsible for are meaningful to the business or necessary for interaction with the application layers of other systems."
> — Eric Evans, *Domain-Driven Design*

## Dependency Rule

The Application Layer depends only on the Domain Layer. It knows nothing about databases, web frameworks, or external services—only their abstractions.

## References

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - Robert C. Martin
- [CQRS Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs) - Microsoft Azure Architecture
- [Application Layer](https://www.domainlanguage.com/ddd/reference/) - Eric Evans, DDD Reference
