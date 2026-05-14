# BirdTaxonomy.API

BirdTaxonomy.API is a scientific backend system designed to expose bird taxonomy data through REST APIs using ASP.NET Core.

---

## Project Overview

This project provides:

- taxonomy catalog management
- species information retrieval
- legacy database integration
- REST API communication
- agent-assisted software engineering

---

## Technology Stack

- C#
- ASP.NET Core 8 Web API
- Entity Framework Core
- SQL Server
- Swagger
- Docker

---

## Architecture

Implemented architectural approaches:

- Clean Architecture
- Layered Architecture
- Data Driven Architecture

---

## Database

Current physical tables:

- rank
- taxon
- species_info

Relationships:

- rank 1:N taxon
- taxon 1:0..1 species_info

---

## API Endpoints

Examples:

- GET /api/taxonomia/rangos
- GET /api/taxonomia/taxones
- POST /api/taxonomia/taxones
- PUT /api/taxonomia/taxones/{id}
- DELETE /api/taxonomia/taxones/{id}

Swagger documentation available during runtime.

---

## Documentation

Project documentation includes:

- software architecture
- ER diagrams
- UML diagrams
- architecture decisions
- AI-assisted engineering workflow

---

## AI Workflow

Agents used during development:

- OpenAI Codex → implementation and refactoring
- GitHub Copilot → validation and consistency review
- NotebookLM → documentation and knowledge synthesis

---

## References and Inspiration

This project was inspired by publicly available educational
and scientific resources.

References include:

- Wikipedia taxonomy articles
- Microsoft .NET documentation
- Entity Framework Core documentation

Wikipedia content was used only as conceptual inspiration.

---

## License

This project is licensed under the MIT License.

See LICENSE for details.