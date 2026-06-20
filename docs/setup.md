# Backend Setup

Fabric backend config lives in `src/backend/Fabric.Server/appsettings.json` and environment-specific overrides such as `appsettings.Development.json`.

Secrets should not be committed. Use environment-specific config, user secrets, environment variables, or a secret store for production values.

## Minimal Configuration

```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Username=user;Password=password;Database=fabric"
  },
  "Cors": {
    "Origins": ["http://localhost:5173"]
  },
  "Tenancy": {
    "Mode": "SingleTenant",
    "DefaultTenant": {
      "Id": "main-tenant",
      "Oidc": {
        "MetadataUrl": "http://localhost:7080/realms/dev/.well-known/openid-configuration",
        "ClientId": "portal",
        "RequireHttpsMetadata": false
      }
    }
  },
  "AllowedHosts": "*",
  "EnableSwagger": true
}
```

## Tenancy Modes

`Tenancy:Mode` controls how tenant config is resolved.

Allowed values:

- `SingleTenant`
- `MultiTenant`

### SingleTenant

Use `SingleTenant` for local dev, demos, or deployments where one backend serves one tenant.

The backend uses `Tenancy:DefaultTenant` to seed the default tenant when migrations run.

```json
{
  "Tenancy": {
    "Mode": "SingleTenant",
    "DefaultTenant": {
      "Id": "main-tenant",
      "Oidc": {
        "MetadataUrl": "https://login.example.com/.well-known/openid-configuration",
        "ClientId": "fabric-portal",
        "RequireHttpsMetadata": true
      }
    }
  }
}
```

Required `DefaultTenant` fields in `SingleTenant` mode:

- `Id`: Fabric tenant id.
- `Oidc:MetadataUrl`: OIDC discovery document URL.
- `Oidc:ClientId`: Portal client id.
- `Oidc:RequireHttpsMetadata`: Set `false` only for local HTTP identity providers.

### MultiTenant

Use `MultiTenant` when one backend serves multiple tenants.

```json
{
  "Tenancy": {
    "Mode": "MultiTenant"
  },
  "AdminOidc": {
    "MetadataUrl": "https://login.example.com/.well-known/openid-configuration",
    "ClientId": "fabric-admin",
    "RequireHttpsMetadata": true
  }
}
```

In `MultiTenant` mode, tenant-specific settings are stored in the tenancy database and loaded at runtime. `AdminOidc` config is required for admin authentication.

Required `AdminOidc` fields in `MultiTenant` mode:

- `MetadataUrl`: OIDC discovery document URL.
- `ClientId`: Admin client id.
- `RequireHttpsMetadata`: Set `false` only for local HTTP identity providers.

## Email Configuration

Email is optional. If no email config exists, email send attempts return `NotConfigured`.

Platform email config is set through appsettings under `Email:Graph`. It acts as the default mail configuration for the platform.

```json
{
  "Email": {
    "Graph": {
      "FromEmail": "noreply@example.com",
      "FromName": "Fabric",
      "AzureTenantId": "00000000-0000-0000-0000-000000000000",
      "ApplicationId": "00000000-0000-0000-0000-000000000000",
      "Secret": "replace-with-secret",
      "SaveSentItems": false
    }
  }
}
```

`Email:Graph` is all-or-nothing. If `Graph` is present, these fields are required:

- `FromEmail`
- `FromName`
- `AzureTenantId`
- `ApplicationId`
- `Secret`

`SaveSentItems` is optional and defaults to `false`.

### Tenant Email Override

Tenants can override platform email config at runtime through the portal.

Resolution order:

1. Tenant-specific Graph email config.
2. Platform `Email:Graph` config from appsettings.
3. `NotConfigured` if neither exists.

Tenant overrides are also all-or-nothing. A tenant either uses a complete Graph email config or falls back to the platform default. Individual tenant fields do not fall back to individual platform fields.

In `SingleTenant` mode, the initial seeded tenant can also receive an email override through `Tenancy:DefaultTenant:GraphEmail`.

```json
{
  "Tenancy": {
    "Mode": "SingleTenant",
    "DefaultTenant": {
      "Id": "main-tenant",
      "Oidc": {
        "MetadataUrl": "https://login.example.com/.well-known/openid-configuration",
        "ClientId": "fabric-portal",
        "RequireHttpsMetadata": true
      },
      "GraphEmail": {
        "FromEmail": "tenant@example.com",
        "FromName": "Tenant Name",
        "AzureTenantId": "00000000-0000-0000-0000-000000000000",
        "ApplicationId": "00000000-0000-0000-0000-000000000000",
        "Secret": "replace-with-secret",
        "SaveSentItems": false
      }
    }
  }
}
```

## Environment Variables

ASP.NET Core configuration supports environment variable overrides with `__` as a section separator.

Examples:

```bash
ConnectionStrings__Database="Host=localhost;Username=user;Password=password;Database=fabric"
Tenancy__Mode="SingleTenant"
Tenancy__DefaultTenant__Id="main-tenant"
Tenancy__DefaultTenant__Oidc__MetadataUrl="https://login.example.com/.well-known/openid-configuration"
Tenancy__DefaultTenant__Oidc__ClientId="fabric-portal"
Email__Graph__FromEmail="noreply@example.com"
Email__Graph__FromName="Fabric"
Email__Graph__AzureTenantId="00000000-0000-0000-0000-000000000000"
Email__Graph__ApplicationId="00000000-0000-0000-0000-000000000000"
Email__Graph__Secret="replace-with-secret"
```
