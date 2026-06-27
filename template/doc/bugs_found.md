# Baseline Template Bugs Discovered Report

This document details the inconsistencies and bugs identified in the project baseline (original template) during the Sales API implementation. These issues were corrected or mitigated to ensure the correct execution of the application.

---

### 1. Complete Break in the Authentication Flow (AutoMapper)

* **Location**: `src/Ambev.DeveloperEvaluation.WebApi/Features/Auth/AuthenticateUserFeature/AuthenticateUserProfile.cs`
* **Type**: Implementation Bug (Omission)
* **Description**: 
  The native authentication controller (`AuthController`) executes object mappings at runtime:
  * From `AuthenticateUserRequest` (Input DTO) to `AuthenticateUserCommand` (Use Case).
  * From `AuthenticateUserResult` (Handler Output) to `AuthenticateUserResponse` (HTTP Response).
  
  However, the `AuthenticateUserProfile` mapping class configured only the `User -> AuthenticateUserResponse` map, omitting the Request/Command and Result/Response relations.
* **Impact**: 
  Any request to the login endpoint (`POST /api/auth`) failed immediately with a `500 Internal Server Error` caused by an `AutoMapperMappingException` ("Missing type map configuration"). This prevented the generation of any JWT Token in the original application.
* **Correction Applied**: 
  Added the missing mappings in `AuthenticateUserProfile.cs`:
  ```csharp
  CreateMap<AuthenticateUserRequest, AuthenticateUserCommand>();
  CreateMap<AuthenticateUserResult, AuthenticateUserResponse>();
  ```

---

### 2. Incorrect Silencing of Warnings and Errors in Console (Serilog Filter)

* **Location**: `src/Ambev.DeveloperEvaluation.Common/Logging/LoggingExtension.cs`
* **Type**: Configuration Logical Bug (Exclusion Filter)
* **Description**: 
  The default logging extension configures an exclusion filter in Serilog using the `_filterPredicate` property:
  ```csharp
  static readonly Func<LogEvent, bool> _filterPredicate = exclusionPredicate =>
  {
      if (exclusionPredicate.Level != LogEventLevel.Information) return true;
      // ...
  };
  ```
  Serilog uses the `.Filter.ByExcluding(predicate)` method where any log returning `true` in the predicate **is discarded**. By returning `true` when `Level != Information`, the filter systematically discarded all logs with a severity level different from `Information` (such as `Warning`, `Error`, and `Fatal`).
* **Impact**: 
  Crucial diagnostic messages such as `LogWarning`, `LogError`, and `LogFatal` (including Exceptions and critical system failures) were **completely silenced** and never printed to the console or recorded in log files.
* **Mitigation**: 
  To avoid invasive changes in the shared library project (`Common`), we adapted the `SaleEventsHandler` to output logs using the `LogInformation` severity level, ensuring they bypass this baseline filter defect and show up in the console.

---

### 3. EF Core Target Assembly Configuration for Migrations

* **Location**: 
  * `src/Ambev.DeveloperEvaluation.ORM/Common/DefaultContextFactory.cs`
  * `src/Ambev.DeveloperEvaluation.WebApi/Program.cs`
* **Type**: Incomplete Configuration
* **Description**: 
  EF Core requires the connection string and migration assembly configuration to be aligned when using separate projects for ORM and WebApi. The `DefaultContextFactory` class initialized the database context without indicating the correct migration assembly target for the ORM project.
* **Impact**: 
  CLI commands such as `dotnet ef database update` failed or searched for migrations in the wrong assembly due to the lack of an explicit target mapping.
* **Correction Applied**: 
  Updated `Program.cs` to explicitly target the ORM assembly when injecting the DB context:
  ```csharp
  builder.Services.AddDbContext<DefaultContext>(options =>
      options.UseNpgsql(
          builder.Configuration.GetConnectionString("DefaultConnection"),
          b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")
      )
  );
  ```
