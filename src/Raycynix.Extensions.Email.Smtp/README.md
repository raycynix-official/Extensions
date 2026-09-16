# Raycynix.Extensions.Email.Smtp

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

SMTP provider integration for `Raycynix.Extensions.Email`.

## What It Provides

- `AddSmtp(...)`
- `SmtpOptions`
- SMTP provider-specific validation
- `IEmailSender` implementation backed by MailKit
- optional Microsoft `ILogger<T>` diagnostics for SMTP delivery phases

The provider is selected by calling `.AddSmtp(...)`.

`SmtpOptions` is bound from `EmailOptions:SmtpOptions` and validated during application startup.

## Usage

```csharp
builder.Services
    .AddRaycynixEmail(builder.Configuration, email =>
    {
        email.DefaultFromAddress = "no-reply@example.com";
        email.DefaultFromDisplayName = "Example App";
    })
    .AddSmtp(smtp =>
    {
        smtp.Host = "smtp.example.com";
        smtp.Port = 465;
        smtp.SecureSocketOptions = SmtpSecureSocketOptions.SslOnConnect;
        smtp.Username = "smtp-user";
        smtp.Password = "smtp-password";
    });
```

## Configuration

```json
{
  "EmailOptions": {
    "DefaultFromAddress": "no-reply@example.com",
    "DefaultFromDisplayName": "Example App",
    "SmtpOptions": {
      "Host": "smtp.example.com",
      "Port": 465,
      "SecureSocketOptions": "SslOnConnect",
      "Username": "smtp-user",
      "Password": "smtp-password",
      "TimeoutMilliseconds": 100000
    }
  }
}
```

## Migrating From 2.x

- Replace `SmtpConfiguration` with `SmtpOptions`.
- Replace the `Raycynix.Extensions.Email.Smtp.Configurations` namespace with `Raycynix.Extensions.Email.Smtp.Options`.
- Rename the configuration path from `EmailConfiguration:SmtpConfiguration` to `EmailOptions:SmtpOptions`.

## Logging

The SMTP provider uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Diagnostics cover configuration validation, MIME message composition counts, connection, authentication, send, disconnect, and provider failures. Message bodies, subjects, recipient addresses, attachment file names, usernames, passwords, and other credential values are not logged.
