# Developer Store - Sales API

Welcome to the **DeveloperStore Sales API**, a reference implementation built as part of the Developer Evaluation Project. This repository hosts a robust, production-ready REST API featuring a complete CRUD for managing sales records, adhering to strict Domain-Driven Design (DDD) principles and Clean Architecture.

---

## 🚀 Key Features

* **Complete Sales CRUD**: Manage sale lifecycle (Creation, Retrieval, Update, and Cancellation).
* **JWT Authentication Security**: All sales endpoints are secured using `[Authorize]` attributes, requiring valid JWT Bearer tokens obtained via the auth endpoint.
* **DDD External Identities**: References to external domains (`Customer`, `Branch`, and `Product`) are modeled using UUIDs/Guids with denormalized entity descriptions for maximum performance and decoupling.
* **Smart Discount Engine**: Encapsulates core business rules directly within the domain entities:
  * 📦 **Quantity < 4**: No discount.
  * 📦 **Quantity between 4 and 9**: 10% discount.
  * 📦 **Quantity between 10 and 20**: 20% discount.
  * 🚫 **Quantity > 20**: Prohibited (domain validation error).
* **Automated Total Calculations**: Instant item-level discount and total value computations aggregated automatically to the sale's total.
* **Domain & Application Events**: Publishes events in-process via MediatR Notifications (`SaleCreatedEvent`, `SaleModifiedEvent`, `SaleCancelledEvent`, and `ItemCancelledEvent`) for decoupled application logging.

---

## 📂 Documentation Index

To explore detailed evaluations of the core system design, architecture, and technology selections, consult the specialized documents below:

* **[Overview](./.doc/overview.md)**: High-level overview of the evaluation and competencies.
* **[Tech Stack](./.doc/tech-stack.md)**: Technical decisions, language, and tool choices.
* **[Frameworks](./.doc/frameworks.md)**: Productive frameworks utilized in this project.
* **[Project Structure](./.doc/project-structure.md)**: Folder organization and project layouts.
* **[Bugs Found](./template/doc/bugs_found.md)**: Details and corrections of baseline template bugs (AutoMapper and Serilog).
* **[Integration Tests Report](./template/doc/integration_tests.md)**: E2E HTTP CRUD validation payloads, JWT headers, and event logging evidence.
* **[Walkthrough Verification](./template/doc/walkthrough.md)**: Details of running the application, user creation, authentication, and full Sales CRUD verification.

---

## 🛠️ How to Configure and Run the Project

Follow this guide to configure the database, apply migrations, and run the API locally.

### 1. Prerequisites
Ensure you have the following installed:
* **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
* **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** (or local PostgreSQL 13 instance)
* **[Entity Framework Core CLI Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)** (`dotnet-ef`)
  * To install EF CLI globally, run: 
    ```bash
    dotnet tool install --global dotnet-ef
    ```

### 2. Database Configuration
1. Open a terminal in the `/backend` folder.
2. Spin up the PostgreSQL database container in the background:
   ```bash
   docker compose up -d ambev.developerevaluation.database
   ```
3. Apply the EF Core database migrations to generate the database schema:
   ```bash
   dotnet ef database update --project src/Ambev.DeveloperEvaluation.ORM --startup-project src/Ambev.DeveloperEvaluation.WebApi
   ```

### 3. Running the Web API
1. Navigate to the `/backend` directory.
2. Launch the WebApi project:
   ```bash
   dotnet run --project src/Ambev.DeveloperEvaluation.WebApi --launch-profile http
   ```
3. Once running, open your web browser and access the interactive Swagger documentation page to test the endpoints:
   ```
   http://localhost:5119/swagger/index.html
   ```

---

## 🧪 Testing and Coverage

The project is backed by a rich test suite verifying both domain rules and application handler behavior.

### Running Automated Tests
To run all test cases (Domain, Application, and Infrastructure):
1. Navigate to the `/backend` directory.
2. Execute the dotnet test runner:
   ```bash
   dotnet test Ambev.DeveloperEvaluation.sln
   ```

### Code Coverage Report
To execute tests and generate rich HTML coverage reports locally:
* **Windows (PowerShell/CMD):** Run `coverage-report.bat`
* **Linux/macOS:** Run `chmod +x coverage-report.sh && ./coverage-report.sh`
* The final HTML report will be generated at `./TestResults/CoverageReport/index.html`.
