# BookShop

A bookshop application with a .NET API, Entity Framework Core persistence, and a React frontend.

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [API Overview](#api-overview)
- [Testing](#testing)
- [Roadmap](#roadmap)
- [Author](#author)

## Features

- Book listings with title, author, publication year, ISBN, price, and availability
- Layered architecture with clear separation of concerns
- Persistence via Entity Framework Core
- Web frontend consuming the API
- Customer registration, orders, and payments
- Swagger / OpenAPI documentation

## Tech Stack

| Layer     | Technology                                      |
| --------- | ----------------------------------------------- |
| Backend   | C#, ASP.NET Core Web API (.NET 10)               |
| Data      | Entity Framework Core, SQLite                    |
| Frontend  | React                                           |
| Tooling   | Swagger/OpenAPI, Git                            |

## Architecture

The solution follows a clean / layered architecture, so business logic stays independent of frameworks and infrastructure.

```
BookShop/
├── BookShop.API             # Controllers, DTOs, middleware, DI setup (entry point)
├── BookShop.Core            # Domain entities, interfaces, business logic (no external dependencies)
├── BookShop.Infrastructure  # EF Core DbContext, repositories, migrations, external services
├── BookShop.Web             # Frontend application
└── BookShop.slnx            # Solution file
```

**Dependency direction:** `API → Infrastructure → Core`

- **Core** defines the domain and abstractions (e.g. repository interfaces) and depends on nothing.
- **Infrastructure** implements those abstractions (data access, etc.).
- **API** wires everything together and exposes HTTP endpoints.

This makes the business logic easy to test and the data layer easy to swap.

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (9.0 or later)
- [Node.js](https://nodejs.org/) (LTS) for the frontend
- SQLite (configured by default)

### 1. Clone the repository

```bash
git clone https://github.com/TomasMike/BookShop.git
cd BookShop
```

### 2. Run the API

```bash
cd BookShop.API
dotnet restore
dotnet run
```

The API will start on the URL shown in the console. Swagger UI is typically available at `/swagger`.

### 3. Run the frontend

```bash
cd BookShop.Web/bookshop-web
npm install
npm run dev
```

### Configuration

Connection strings and settings live in `BookShop.API/appsettings.json`. For local secrets, use [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) rather than committing credentials:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>"
```

## API Overview

<!-- Adjust to match your real endpoints. -->

| Method | Endpoint          | Description            |
| ------ | ----------------- | ---------------------- |
| GET    | `/Book`       | List all books          |
| GET    | `/Book/{id}`  | Get a book by ID        |
| POST   | `/Book`       | Create a new book       |
| POST   | `/api/orders` | Create an order         |

Example request:

```http
POST /Book
Content-Type: application/json

{
  "title": "Dune",
  "author": { "id": 1, "name": "Frank Herbert" },
  "publishedYear": 1965,
  "isbn": "9780441172719",
  "price": 12.99,
  "isAvailable": true
}
```

Full interactive documentation is available via Swagger when the API is running.

## Testing

```bash
dotnet test
```

<!-- TODO: describe your unit / integration tests once added. -->

## Roadmap

- [ ] Unit and integration tests
- [ ] CI pipeline with GitHub Actions
- [ ] Docker / docker-compose setup
- [ ] Authentication and role-based authorization (JWT)
- [ ] Pagination, filtering and sorting
- [ ] Global error handling and request validation
- [ ] Structured logging and health checks

## Author

**Tomas Mike**
GitHub: [@TomasMike](https://github.com/TomasMike)

<!-- Optional: add LinkedIn / portfolio links. -->
