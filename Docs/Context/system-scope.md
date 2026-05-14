# BirdTaxonomy — System Scope

## In Scope

The current version includes:

### Backend

- REST API development
- DTO validation
- Business service layer
- Repository abstraction
- ORM mapping

### Database

Integration with an existing SQL Server database:

- rank
- taxon
- species_info

Supported relationships:

- rank 1:N taxon
- taxon 1:0..1 species_info

### Frontend

Browser-based client for:

- listing taxonomy records
- creating records
- editing records
- deleting records

### Deployment

Production hosting on:

- MonsterASP.NET

### Documentation

Project documentation includes:

- Swagger
- Markdown documentation
- UML diagrams
- ER diagrams
- Architecture documentation

---

## Out of Scope (Current Version)

The current version does NOT include:

- Authentication
- Authorization
- Role-based access
- Cloud services
- Distributed microservices
- Real-time messaging
- Machine learning pipelines

---

## Future Scope

Planned expansion:

- Azure deployment
- Containerization with Docker
- CI/CD pipelines
- AI agents integration
- Data engineering workflows
- Scientific analytics