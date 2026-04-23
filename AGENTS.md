# IdentityPoc Agent Guide

This document defines the system purpose, architecture, domain rules, module responsibilities, and testing rules for agents working on this repository.

## Purpose

IdentityPoc is a bounded context for authentication and authorization.

Other company systems should use this API to:

- Authenticate users.
- Check whether authenticated users are authorized for an action.
- Let administrators manage users, groups, and permissions.

The project is intentionally simple right now. It uses an in-memory store and plain passwords because it is a proof of concept. Do not treat those choices as production-ready security decisions.

## Current Architecture

The API is an ASP.NET Core Minimal API.

Main project:

```text
src/
  IdentityPoc.Api/
    Contracts/
    Domain/
    Endpoints/
    Infrastructure/
```

Test project:

```text
tests/
  IdentityPoc.Api.Tests/
    Domain/
    Integration/
```

### Composition Root

`src/IdentityPoc.Api/Program.cs`

Responsibilities:

- Register application services.
- Configure JSON serialization.
- Configure middleware.
- Map endpoint modules.

Rules:

- Keep `Program.cs` small.
- Do not put domain rules in `Program.cs`.
- Do not put endpoint business logic in `Program.cs`.

### Domain

`src/IdentityPoc.Api/Domain`

Responsibilities:

- Represent core business concepts.
- Define roles, permissions, and domain behavior.
- Keep rules independent from HTTP, persistence, and framework concerns.

Current modules:

- `Authorization`
  - `Role`
  - `Permission`
  - `RolePermissionPolicy`
- `Users`
  - `UserAccount`
- `Groups`
  - `AccessGroup`

Rules:

- Domain code must not depend on endpoints, contracts, or infrastructure.
- Domain behavior should be testable as unit tests.
- Prefer explicit methods for behavior, such as `Grant` and `AddToGroup`.
- Keep invariants close to the entity or policy that owns them.

### Contracts

`src/IdentityPoc.Api/Contracts`

Responsibilities:

- Define request and response DTOs.
- Shape the public API boundary.

Rules:

- Contracts should be simple records.
- Contracts may reference domain enums such as `Role` and `Permission`.
- Do not put behavior in contracts.
- Do not expose mutable domain entities directly from endpoints.

### Infrastructure

`src/IdentityPoc.Api/Infrastructure`

Responsibilities:

- Hold the current in-memory persistence implementation.
- Provide application service behavior that coordinates domain objects.

Current modules:

- `IdentityStore`
- `IdentityService`

Rules:

- Services are tested as integration tests with real collaborators.
- Do not use mocks for service tests.
- Keep persistence details behind infrastructure classes.
- Future database implementations should preserve the behavior covered by integration tests.

### Endpoints

`src/IdentityPoc.Api/Endpoints`

Responsibilities:

- Define HTTP routes.
- Bind requests.
- Return appropriate HTTP results.
- Delegate behavior to services.

Current modules:

- `AuthenticationEndpoints`
- `AuthorizationEndpoints`
- `AdminEndpoints`

Rules:

- Keep endpoint handlers thin.
- Do not duplicate domain rules in endpoint modules.
- Do not access domain collections directly from endpoints.
- Use services for application behavior.
- Admin endpoints currently require `X-Actor-User-Id` with an admin user id.

## Domain Rules

### Roles

The system has exactly these roles today:

- `User`
- `Mod`
- `Admin`

### Permissions

The system has these permissions today:

- `Read`
- `Write`
- `Delete`
- `Update`
- `ManagePermissions`
- `ManageGroups`
- `ManageUsers`

### Role Permission Policy

Role permissions are defined by `RolePermissionPolicy`.

Rules:

- `User` has `Read` and `Write`.
- `Mod` has `Read`, `Write`, `Delete`, and `Update`.
- `Admin` has every permission.

If a role rule changes, update:

- `RolePermissionPolicy`
- Domain unit tests
- Relevant service integration tests
- This document

### Effective Permissions

A user's effective permissions are the union of:

- Permissions from the user's role.
- Direct permissions granted to the user.
- Permissions granted to groups that contain the user.

Rules:

- Direct permissions add capabilities; they do not currently remove capabilities.
- Group permissions add capabilities; they do not currently remove capabilities.
- There is no deny rule yet.
- Permission calculation lives in `IdentityService` today.

### Users

A user has:

- Id
- Display name
- Email
- Password
- Role
- Direct permissions
- Group memberships

Rules:

- `UserAccount` owns direct permission assignment.
- `UserAccount` owns group membership assignment from the user side.
- The current password field is plain text only for the proof of concept.
- Do not add production authentication assumptions without explicitly changing the design.

### Groups

A group has:

- Id
- Name
- Description
- Permissions
- User memberships

Rules:

- `AccessGroup` owns group permission assignment.
- `AccessGroup` owns user membership assignment from the group side.
- Adding a user to a group must keep both user and group state consistent.

### Administration

An admin can:

- List users.
- Create users.
- List groups.
- Create groups.
- Add users to groups.
- Grant permissions to users.
- Grant permissions to groups.

Current admin authorization rule:

- A caller is considered admin if their effective permissions include `ManagePermissions`.

Current HTTP mechanism:

- Admin endpoints read `X-Actor-User-Id`.
- Missing or invalid header returns `401`.
- Non-admin actor returns `403`.

## API Surface

Current endpoints:

- `GET /health`
- `POST /api/auth/login`
- `GET /api/auth/users/{userId}/profile`
- `POST /api/authorization/check`
- `GET /api/admin/users`
- `POST /api/admin/users`
- `GET /api/admin/groups`
- `POST /api/admin/groups`
- `POST /api/admin/groups/{groupId}/users`
- `POST /api/admin/groups/{groupId}/permissions`
- `POST /api/admin/users/{userId}/permissions`

## Seed Data

`IdentityStore` currently seeds:

| Email | Password | Role |
| --- | --- | --- |
| `admin@company.local` | `admin123` | `Admin` |
| `mod@company.local` | `mod123` | `Mod` |
| `user@company.local` | `user123` | `User` |

Seed groups:

- `Support`
- `InternalApps`

Rules:

- Tests may rely on seed behavior when testing the current service integration behavior.
- If seed data changes, update integration tests and README/agent docs.

## Testing Rules

Run tests with:

```powershell
dotnet test IdentityPoc.slnx
```

### Unit Tests

Location:

```text
tests/IdentityPoc.Api.Tests/Domain
```

Use unit tests for:

- Domain policies.
- Domain entities.
- Domain invariants.

Rules:

- Unit tests must not use mocks.
- Unit tests should not spin up HTTP or infrastructure.
- Unit tests should exercise domain behavior directly.

### Integration Tests

Location:

```text
tests/IdentityPoc.Api.Tests/Integration
```

Use integration tests for:

- Services.
- Behavior that coordinates store, domain entities, policies, and contracts.

Rules:

- Do not use mocks.
- Use real `IdentityStore`.
- Use real `IdentityService`.
- Prefer testing externally visible service behavior instead of private implementation details.

### Endpoint Tests

Endpoint tests do not exist yet.

When introduced, they should be integration-style tests using the real API composition. Do not mock services unless the project explicitly changes the testing policy.

## Change Guidelines

When adding a feature:

1. Put business concepts and rules in `Domain`.
2. Put API request/response shapes in `Contracts`.
3. Put coordination and persistence-facing behavior in `Infrastructure`.
4. Put HTTP route mapping in `Endpoints`.
5. Add domain unit tests for domain behavior.
6. Add service integration tests for service behavior.
7. Update `README.md` and this file when public behavior or rules change.

When modifying authorization:

1. Update role/permission rules in the domain policy.
2. Update effective permission behavior in the service only if aggregation rules change.
3. Update integration tests for service-visible outcomes.
4. Update admin endpoint behavior if HTTP access rules change.

## Current Limitations

These are known proof-of-concept limitations:

- Passwords are stored in plain text.
- There is no JWT or token issuing yet.
- There is no database.
- There is no refresh token flow.
- There is no permission deny or revoke behavior.
- There is no endpoint-level test suite yet.
- Admin HTTP authorization uses `X-Actor-User-Id`, not a production authentication scheme.

Do not silently treat these limitations as solved. If changing one, update tests and documentation.
