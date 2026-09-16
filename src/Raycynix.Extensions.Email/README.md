# Raycynix.Extensions.Email

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

Shared email registration infrastructure for Raycynix applications.

## What It Provides

- `AddRaycynixEmail(...)`
- `EmailOptions`
- email provider resolution
- email builder support for provider packages
- optional Microsoft `ILogger<T>` diagnostics for provider resolution

This package registers the shared email infrastructure. A provider package, such as `Raycynix.Extensions.Email.Smtp`, must be added to provide an `IEmailSender` implementation.

`EmailOptions` is bound from the conventional `EmailOptions` section and validated during application startup.

## Usage

```csharp
builder.Services
    .AddRaycynixEmail(builder.Configuration, email =>
    {
        email.DefaultFromAddress = "no-reply@example.com";
        email.DefaultFromDisplayName = "Example App";
    })
    .AddSmtp();
```

## Configuration

```json
{
  "EmailOptions": {
    "DefaultFromAddress": "no-reply@example.com",
    "DefaultFromDisplayName": "Example App",
    "DefaultReplyToAddress": "support@example.com",
    "DefaultReplyToDisplayName": "Support"
  }
}
```

## Migrating From 2.x

- Replace `EmailConfiguration` with `EmailOptions`.
- Replace the `Raycynix.Extensions.Email.Configurations` namespace with `Raycynix.Extensions.Email.Options`.
- Rename the configuration section from `EmailConfiguration` to `EmailOptions`.

## Logging

The package uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Provider resolution diagnostics include registration counts and provider selection flow only. Email addresses, subjects, bodies, and credentials are not logged.
