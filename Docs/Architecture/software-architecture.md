# BirdTaxonomy — Software Architecture

## Overview

BirdTaxonomy is implemented as a backend-driven system based on
ASP.NET Core Web API using layered architecture principles and
Clean Architecture boundaries.

The system exposes REST endpoints that communicate with an existing
SQL Server database and serve both browser-based clients and
development tools.

---

## High-Level Architecture

The solution is composed of the following major components:

### 1. Clients

External consumers of the API.

Includes:

- Browser application (HTML/CSS/JavaScript)
- Swagger UI
- API testing tools

Responsibilities:

- Send HTTP/HTTPS requests
- Consume JSON responses
- Execute CRUD operations

---

### 2. API Layer

Entry point of the application.

Technology:

- ASP.NET Core 8 Web API

Responsibilities:

- Receive HTTP requests
- Route endpoints
- Validate input
- Return HTTP responses

Main component:

- TaxonomiaController

---

### 3. Business Layer

Contains application use cases.

Responsibilities:

- Execute business workflows
- Coordinate domain entities
- Validate business rules
- Orchestrate repositories

Main component:

- TaxonomiaService

---

### 4. Persistence Layer

Responsible for database communication.

Technology:

- Entity Framework Core

Responsibilities:

- ORM mapping
- Query execution
- Entity tracking
- Transaction handling

Main components:

- BirdTaxonomyDbContext
- Repositories

---

### 5. Database Layer

Production and development storage.

Technology:

- SQL Server

Current physical schema:

- rank
- taxon
- species_info

---

## Communication Flow

The system follows this flow:

Client
→ HTTP Request
→ Controller
→ Service
→ Repository
→ DbContext
→ SQL Server

Response flow:

SQL Server
→ DbContext
→ Repository
→ Service
→ Controller
→ JSON Response

---

## Deployment Architecture

Current hosting:

- MonsterASP.NET

Current deployment model:

- Monolithic deployment
- Single API project
- Shared database

---

## Future Evolution

Planned improvements:

- Physical multi-project split
- Docker containers
- CI/CD pipelines
- Cloud deployment
- Observability
- AI-assisted workflows