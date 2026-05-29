# Resume & Vacancy Management System

A REST API for managing resumes and job vacancies, built with ASP.NET Core 8 following a strict multi-layer architecture (DAL → BLL → PL).

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Solution Structure](#2-solution-structure)
3. [Architecture](#3-architecture)
4. [Prerequisites](#4-prerequisites)
5. [Getting Started](#5-getting-started)
6. [API Reference](#6-api-reference)
7. [Running Tests](#7-running-tests)
8. [Configuration Reference](#8-configuration-reference)

---

## 1. Project Overview

The system connects job seekers with employers through three functional areas:

- **Resume & Vacancy management** — create, edit, delete, and browse resumes and vacancies with sorting, filtering, and keyword search.
- **Matching** — find vacancies that match a resume's skills, or find resumes that match a vacancy's required skills.
- **Applications** — employees can apply their resume to a vacancy; employers can propose a vacancy to a resume owner. Both sides can view all linked records.

### User Roles

| Role | Permissions |
|---|---|
| **Unauthorized** | No access to resumes or vacancies |
| **Employee** | Create/edit their own resumes; search matching vacancies; apply to vacancies |
| **Employer** | Create/edit their own vacancies; search matching resumes; propose vacancies to resume owners |
| **Administrator** | Full access to all resources |

Role IDs used during registration: `1` = Administrator, `2` = Employer, `3` = Employee.

---

## 2. Solution Structure

```
Solution/
├── DAL/        Data Access Layer — EF Core entities, repositories, Unit of Work
├── BLL/        Business Logic Layer — services, DTOs, domain validation
├── PL/         Presentation Layer — ASP.NET Core WebAPI controllers
└── Tests/      Unit tests — covers all BLL services
```

### Project responsibilities

| Project | Type | Responsibility |
|---|---|---|
| **DAL** | Class library | Entity definitions, EF Core `DbContext`, migrations, Repository and UoW implementations |
| **BLL** | Class library | All business rules, input validation, role-based authorization, DTO mapping, custom exceptions |
| **PL** | ASP.NET WebAPI | HTTP routing, request/response models, JWT authentication middleware, DI wiring, Swagger |
| **Tests** | xUnit test project | Unit tests for every BLL service; no DAL or PL dependencies |

---

## 3. Architecture

### Layer interaction

```
HTTP Request
    ↓
PL (Controller)           — validates HTTP, maps to BLL DTOs
    ↓
BLL (Service)             — applies business rules, authorisation, maps to/from DAL entities
    ↓
DAL (Repository / UoW)   — EF Core queries against the database
    ↓
Database (SQLite / SQL Server)
```

Each layer only depends on the layer directly below it. DAL is never referenced by PL directly.

### Patterns used

| Pattern | Where |
|---|---|
| **Repository** | `DAL/Interfaces/IRepository<T>` — generic CRUD; extended by entity-specific repositories |
| **Unit of Work** | `DAL/Interfaces/IUnitOfWork` — single access point to all repositories; wraps `SaveChangesAsync` |
| **Dependency Injection** | ASP.NET Core built-in container; services registered as `Scoped` |
| **DTO / Mapper** | BLL DTOs (records) separate domain state from transport; PL ViewModels separate HTTP contracts from BLL DTOs |
| **OperationResult\<T\>** | Standard return envelope for BLL mutations; carries `IsSuccess`, `Data`, and `ErrorMessage` |
| **JWT Bearer** | Stateless authentication; token issued on login, validated on every protected endpoint |
| **Custom Exceptions** | `EntityNotFoundException`, `AuthorizationException`, `DuplicateEntityException`, `ValidationException` — caught by global middleware and mapped to HTTP status codes |

---

## 4. Prerequisites

| Requirement | Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0 or later |
| Database | SQLite (default, zero-config) **or** MS SQL Server 2019+ |
| EF Core CLI (optional, for migrations) | `dotnet tool install -g dotnet-ef` |

---

## 5. Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd <repository-folder>
```

### 2. Configure the connection string

Open `PL/appsettings.json` and set the `DefaultConnection` string:

**SQLite (default — no installation required):**
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=ResumeVacancyDb.sqlite"
}
```

**MS SQL Server:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ResumeVacancyDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

If switching to SQL Server also update `ServiceCollectionExtensions.cs` to call `UseSqlServer` instead of `UseSqlite`.

### 3. Apply EF Core migrations

Run from the solution root. The migrations live in the `DAL` project; the startup project is `PL`.

```bash
dotnet ef database update --project DAL --startup-project PL
```

This creates the database and seeds the `Roles` table (`Administrator`, `Employer`, `Employee`).

### 4. Build and run

```bash
dotnet build
dotnet run --project PL
```

The API starts on `https://localhost:5001` (HTTPS) and `http://localhost:5000` (HTTP) by default.

### 5. Explore with Swagger

Open `http://localhost:5000/swagger` in a browser. All endpoints are documented there. To authenticate, call `POST /api/auth/login`, copy the returned token, and paste it into the **Authorize** dialog (without the `Bearer ` prefix).

---

## 6. API Reference

All endpoints except `/api/auth/*` require a valid JWT token in the `Authorization: Bearer <token>` header.

---

### AuthController — `/api/auth`

No authentication required.

#### `POST /api/auth/register`

Register a new user.

**Request body:**
```json
{
  "email": "jane@example.com",
  "password": "SecurePass1!",
  "firstName": "Jane",
  "lastName": "Doe",
  "roleId": 3
}
```
`roleId`: `1` = Administrator, `2` = Employer, `3` = Employee.

**Response `200 OK`:**
```json
{
  "id": 1,
  "email": "jane@example.com",
  "firstName": "Jane",
  "lastName": "Doe",
  "roleName": "Employee"
}
```

---

#### `POST /api/auth/login`

Authenticate and receive a JWT token.

**Request body:**
```json
{
  "email": "jane@example.com",
  "password": "SecurePass1!"
}
```

**Response `200 OK`:**
```json
{
  "token": "<jwt>",
  "user": {
    "id": 1,
    "email": "jane@example.com",
    "firstName": "Jane",
    "lastName": "Doe",
    "roleName": "Employee"
  }
}
```

**Response `401 Unauthorized`** — wrong credentials.

---

### ResumesController — `/api/resumes`

All endpoints require authentication.

#### `GET /api/resumes`

List resumes. Supports optional query parameters for filtering and sorting.

| Query param | Type | Description |
|---|---|---|
| `keyword` | string | Search in title, description, and skills |
| `sortBy` | string | Field to sort by (e.g. `title`, `expectedSalary`, `createdAt`) |
| `ascending` | bool | Sort direction; default `true` |
| `minSalary` | decimal | Minimum expected salary filter |
| `maxSalary` | decimal | Maximum expected salary filter |

**Response `200 OK`:** array of resume objects (see response shape below).

---

#### `GET /api/resumes/{id}`

Get a single resume by ID.

**Response `200 OK`:**
```json
{
  "id": 1,
  "title": "Senior C# Developer",
  "description": "5 years of backend experience",
  "skills": "C#,SQL,Docker",
  "expectedSalary": 5000.00,
  "createdAt": "2024-06-01T10:00:00Z",
  "updatedAt": "2024-06-01T10:00:00Z",
  "userId": 3,
  "userFullName": "Jane Doe"
}
```

**Response `404 Not Found`** — resume does not exist.

---

#### `POST /api/resumes`

Create a resume. Roles: **Employee**, **Administrator**.

**Request body:**
```json
{
  "title": "Senior C# Developer",
  "description": "5 years of backend experience",
  "skills": "C#,SQL,Docker",
  "expectedSalary": 5000.00
}
```

**Response `201 Created`:** the created resume object.

---

#### `PUT /api/resumes/{id}`

Update a resume. Roles: **Employee**, **Administrator**.

**Request body:** same shape as `POST /api/resumes`.

**Response `200 OK`:** the updated resume object.

---

#### `DELETE /api/resumes/{id}`

Delete a resume. Roles: all authenticated users (ownership enforced by the service).

**Response `204 No Content`.**

---

#### `GET /api/resumes/{id}/matching-vacancies`

Find vacancies whose required skills overlap with this resume's skills.

**Response `200 OK`:** array of vacancy objects.

---

### VacanciesController — `/api/vacancies`

All endpoints require authentication.

#### `GET /api/vacancies`

List vacancies. Supports optional query parameters:

| Query param | Type | Description |
|---|---|---|
| `keyword` | string | Search in title, description, and required skills |
| `sortBy` | string | Field to sort by (e.g. `title`, `salary`, `createdAt`) |
| `ascending` | bool | Sort direction; default `true` |
| `minSalary` | decimal | Minimum salary filter |
| `maxSalary` | decimal | Maximum salary filter |
| `company` | string | Filter by company name |

**Response `200 OK`:** array of vacancy objects (see response shape below).

---

#### `GET /api/vacancies/{id}`

Get a single vacancy by ID.

**Response `200 OK`:**
```json
{
  "id": 2,
  "title": "Backend Developer",
  "description": "Join our engineering team",
  "company": "Acme Corp",
  "requiredSkills": "C#,SQL,REST",
  "salary": 4000.00,
  "createdAt": "2024-06-01T09:00:00Z",
  "updatedAt": "2024-06-01T09:00:00Z",
  "userId": 5,
  "userFullName": "Alice Smith"
}
```

---

#### `POST /api/vacancies`

Create a vacancy. Roles: **Employer**, **Administrator**.

**Request body:**
```json
{
  "title": "Backend Developer",
  "description": "Join our engineering team",
  "company": "Acme Corp",
  "requiredSkills": "C#,SQL,REST",
  "salary": 4000.00
}
```

**Response `201 Created`:** the created vacancy object.

---

#### `PUT /api/vacancies/{id}`

Update a vacancy. Roles: **Employer**, **Administrator**.

**Request body:** same shape as `POST /api/vacancies`.

**Response `200 OK`:** the updated vacancy object.

---

#### `DELETE /api/vacancies/{id}`

Delete a vacancy. Roles: all authenticated users (ownership enforced by the service).

**Response `204 No Content`.**

---

#### `GET /api/vacancies/{id}/matching-resumes`

Find resumes whose skills overlap with this vacancy's required skills.

**Response `200 OK`:** array of resume objects.

---

### ApplicationsController — `/api/applications`

All endpoints require authentication.

#### `POST /api/applications/apply`

Apply a resume to a vacancy (employee initiates). Roles: **Employee**, **Administrator**.

**Request body:**
```json
{
  "resumeId": 1,
  "vacancyId": 2
}
```

**Response `200 OK`:**
```json
{
  "id": 10,
  "resumeId": 1,
  "resumeTitle": "Senior C# Developer",
  "vacancyId": 2,
  "vacancyTitle": "Backend Developer",
  "type": "Apply",
  "status": "Pending",
  "appliedAt": "2024-06-15T14:30:00Z"
}
```

---

#### `POST /api/applications/propose`

Propose a vacancy to a resume owner (employer initiates). Roles: **Employer**, **Administrator**.

**Request body:**
```json
{
  "resumeId": 1,
  "vacancyId": 2
}
```

**Response `200 OK`:** application object (same shape, `type` = `"Propose"`).

---

#### `GET /api/applications/resume/{id}`

List all vacancies linked to a resume (both applied and proposed).

**Response `200 OK`:** array of vacancy objects.

---

#### `GET /api/applications/vacancy/{id}`

List all resumes linked to a vacancy (both applied and proposed).

**Response `200 OK`:** array of resume objects.

---

## 7. Running Tests

The `Tests` project uses **xUnit**, **Moq**, and **FluentAssertions**. All tests are pure unit tests — repositories are mocked, no database is required.

### Run all tests

```bash
dotnet test
```

### Run with detailed output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run with coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### What is tested

| Test class | Service under test | Key scenarios covered |
|---|---|---|
| `ResumeServiceTests` | `ResumeService` | Create (allowed/forbidden roles), matching vacancies (skill overlap, no overlap, resume not found) |
| `VacancyServiceTests` | `VacancyService` | Create (allowed/forbidden roles), matching resumes, delete cascades applications |
| `ApplicationServiceTests` | `ApplicationService` | Apply, propose, duplicate detection, linked vacancy/resume retrieval |
| `UserServiceTests` | `UserService` | Registration (success, duplicate email), authentication (correct/wrong password) |

All test methods follow the `MethodName_Scenario_ExpectedResult` naming convention and the AAA (Arrange / Act / Assert) structure.

---

## 8. Configuration Reference

All configuration lives in `PL/appsettings.json`.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ResumeVacancyDb.sqlite"
  },
  "Jwt": {
    "Key": "change-this-to-a-secret-key-at-least-32-characters-long!",
    "Issuer": "ResumeVacancyApi",
    "Audience": "ResumeVacancyClient",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | ADO.NET / EF Core connection string. Use `Data Source=<file>.sqlite` for SQLite or a standard SQL Server connection string. |
| `Jwt:Key` | HMAC-SHA256 signing key for JWT tokens. **Must be at least 32 characters. Change before deploying to production.** |
| `Jwt:Issuer` | Expected `iss` claim in the JWT. Must match between token generation and validation. |
| `Jwt:Audience` | Expected `aud` claim in the JWT. Must match between token generation and validation. |
| `Jwt:ExpirationMinutes` | Token lifetime in minutes. After expiry the client must log in again. |
| `Logging:LogLevel:Default` | Minimum log level for all categories (`Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`). |
| `Logging:LogLevel:Microsoft.AspNetCore` | Overrides the log level for ASP.NET Core framework messages. |
| `AllowedHosts` | Comma-separated list of allowed `Host` header values. `*` allows any host. |

### Environment-specific overrides

`PL/appsettings.Development.json` is loaded automatically when `ASPNETCORE_ENVIRONMENT=Development` (the default for `dotnet run`). Override any key there without touching the base file.

```bash
# Example: run in production mode
ASPNETCORE_ENVIRONMENT=Production dotnet run --project PL
```
