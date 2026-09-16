# Raycynix.Extensions.Email.Abstractions

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

Contracts and models shared by the Raycynix email packages.

## What It Provides

- `IEmailBuilder`
- `IEmailSender`
- `IEmailProviderRegistration`
- `EmailMessage`
- `EmailAddress`
- `EmailBody`
- `EmailAttachment`
- `EmailSendResult`
- email send and provider configuration exceptions

## Provider Contracts

Provider packages register their own `IEmailSender` implementation and implement `IEmailProviderRegistration` to expose provider identity and validate provider-specific settings.

```csharp
public interface IEmailProviderRegistration
{
    string ProviderName { get; }

    void Validate(IServiceProvider serviceProvider);
}
```

`EmailSendResult` represents expected provider outcomes. Use failures for provider responses such as rejected messages or invalid recipients. Throw `EmailSendException` when no reliable provider result was produced.

`EmailBody` is created through factory methods so it always contains sendable content. Sender implementations should call `EmailMessage.Validate()` before mapping a message to provider-specific APIs.

Subjects and custom headers reject line breaks before provider mapping. Attachment factories return a new stream for each read and honor cancellation before opening content.

`EmailSendResult` copies and validates provider metadata. Metadata keys must be non-empty and values cannot be `null`.

## Logging

This package contains contracts, models, and exceptions only. Runtime diagnostics belong to the provider packages, so this package does not add a logging dependency.

## Usage

Libraries can depend on this package when they only need the sender contract and message models.

```csharp
public sealed class WelcomeEmailService(IEmailSender emailSender)
{
    public Task SendAsync(string address, CancellationToken cancellationToken = default)
    {
        return emailSender.SendAsync(new EmailMessage
        {
            To = [new EmailAddress(address)],
            Subject = "Welcome",
            Body = EmailBody.FromPlainText("Welcome to Raycynix.")
        }, cancellationToken);
    }
}
```
