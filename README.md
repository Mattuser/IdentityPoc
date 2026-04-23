# IdentityPoc

Backend API proof of concept for authentication and authorization flows.

For architecture, domain rules, module responsibilities, and agent guidance, read [AGENTS.md](AGENTS.md).

Passwords are stored as BCrypt hashes by default. Successful login returns a Bearer JWT access token.

## Requirements

- .NET SDK 10

## Run

```powershell
dotnet run --project src\IdentityPoc.Api --urls http://localhost:5055
```

## Endpoints

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

Admin endpoints require `Authorization: Bearer <accessToken>` for a user whose effective permissions include `ManagePermissions`.

## Seed Users

| Email | Password | Role |
| --- | --- | --- |
| `admin@company.local` | `admin123` | `Admin` |
| `mod@company.local` | `mod123` | `Mod` |
| `user@company.local` | `user123` | `User` |

## Domain Rules

- `User`: `Read`, `Write`
- `Mod`: `Read`, `Write`, `Delete`, `Update`
- `Admin`: all permissions, including user, group, and permission management

## Tests

```powershell
dotnet test IdentityPoc.slnx
```

Test strategy:

- Domain behavior is covered by unit tests.
- Service behavior is covered by integration tests using the real in-memory store.
- Mocks are intentionally avoided.

## Project Layout

```text
src/
  IdentityPoc.Api/
    Contracts/
    Domain/
    Endpoints/
    Infrastructure/
tests/
  IdentityPoc.Api.Tests/
    Domain/
    Integration/
```
