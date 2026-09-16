# Raycynix.Extensions.Secrets

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Secrets` provides a unified secret-resolution layer for Raycynix applications.

It allows application code to ask for secrets through `ISecretResolver` while the actual values can come from the standard configuration pipeline or environment-specific fallback providers.

## What it contains

- `AddRaycynixSecrets(...)`
- `SecretOptions`
- `ISecretProvider` registrations
- `ISecretResolver`
- `ISecretDiagnosticsResolver`
- `GetRequiredSecretAsync(...)`
- `ResolveWithSourceAsync(...)`
- `ExplainSecretResolutionAsync(...)`
- configuration, environment, GitHub Actions, and TeamCity-style secret providers

## What it does not contain

- cloud-specific secret storage integrations
- UI or interactive secret management
- a custom secret file format

## Why use it

`IConfiguration` is still the place where configuration is assembled, but `Raycynix.Extensions.Secrets` gives applications a separate API for secret access.

That separation is useful when you want to:

- consume secrets through a dedicated abstraction instead of injecting raw `IConfiguration`
- support multiple secret sources without changing application code
- keep CI/CD-oriented environment variable resolution as a fallback when configuration does not contain a value

## Usage

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.UseRaycynixConfigurationSources(options =>
{
    options.BaseFileName = "appsettings";
    options.EnvironmentName = builder.Environment.EnvironmentName;
    options.IncludeUserSecrets = builder.Environment.IsDevelopment();
});

builder.Services.AddRaycynixSecrets(builder.Configuration);

public sealed class GitHubTokenLoader(ISecretResolver secrets)
{
    public async Task<string?> LoadAsync(CancellationToken cancellationToken)
    {
        return await secrets.GetSecretAsync("GitHub:Token", cancellationToken);
    }
}
```

See the runnable example in [examples/Raycynix.Extensions.Secrets.Example/Program.cs](https://github.com/Raycynix/Extensions/blob/main/examples/Raycynix.Extensions.Secrets.Example/Program.cs) for a complete walkthrough of:

- default provider precedence
- custom provider precedence through `SecretOptions`
- required-secret resolution
- provider-aware resolution results
- explain output that shows the evaluated provider chain

The package resolves secrets through a provider chain and returns the first available value.

By default, the provider chain checks sources in this order:

1. `IConfiguration`
2. exact environment variable key
3. GitHub Actions-style normalized environment variable
4. TeamCity-style normalized environment variable

This allows applications to keep using the standard configuration pipeline, including `.NET User Secrets`, while still consuming secrets through a dedicated `ISecretResolver`.

You can customize the provider order when the default precedence is not appropriate:

```json
{
  "SecretOptions": {
    "ProviderOrder": [
      "GitHub",
      "Configuration",
      "Environment",
      "TeamCity"
    ]
  }
}
```

```csharp
builder.Services.AddRaycynixSecrets(builder.Configuration, options =>
{
    options.ProviderOrder.Clear();
    options.ProviderOrder.Add(SecretProviderNames.GitHub);
    options.ProviderOrder.Add(SecretProviderNames.Configuration);
    options.ProviderOrder.Add(SecretProviderNames.Environment);
    options.ProviderOrder.Add(SecretProviderNames.TeamCity);
});
```

Providers not listed in `SecretOptions.ProviderOrder` are still evaluated afterward in their registration order.
Provider failures fall through to the next provider by default. Set `SecretOptions:ContinueOnProviderError` to `false` when fail-fast resolution is required.
Custom providers can be referenced by their short name, CLR type name, or fully qualified type name. Unknown and duplicate configured names are rejected.

For environment-based configuration, use indexed keys:

```dotenv
SecretOptions__ProviderOrder__0=GitHub
SecretOptions__ProviderOrder__1=Configuration
SecretOptions__ProviderOrder__2=Environment
SecretOptions__ProviderOrder__3=TeamCity
```

## Logging

The package uses optional Microsoft `ILogger<T>` diagnostics when logging is registered in the application. No Raycynix logging provider is required.

Diagnostics cover provider chain initialization, provider attempts, successful provider selection, and missing-secret outcomes. Secret keys, normalized keys, secret values, configuration values, and environment variable values are not logged.

Examples:

- `ConnectionStrings:Main` can be resolved from `IConfiguration["ConnectionStrings:Main"]`
- or from the exact environment variable `ConnectionStrings:Main`
- or from `CONNECTIONSTRINGS_MAIN` in GitHub Actions-style environments
- or from `ConnectionStrings.Main` in the process environment when the TeamCity parameter is named `env.ConnectionStrings.Main`

You can also use the convenience APIs for required secrets and diagnostics:

```csharp
var token = await secrets.GetRequiredSecretAsync("Api:Token", cancellationToken);

var resolved = await secrets.ResolveWithSourceAsync("ConnectionStrings:Main", cancellationToken);
Console.WriteLine(resolved.ProviderName);

var attempts = await secrets.ExplainSecretResolutionAsync("ConnectionStrings:Main", cancellationToken);
foreach (var attempt in attempts)
{
    Console.WriteLine($"{attempt.ProviderName}: {attempt.Succeeded}");
}
```

Typical output looks like this:

```text
Default provider order
Configuration -> Environment -> GitHub -> TeamCity
GetSecretAsync: Server=config;Database=main;
ResolveWithSourceAsync.ProviderName: ConfigurationSecretProvider
GetRequiredSecretAsync(Api:Token): config-token
ExplainSecretResolutionAsync:
- ConfigurationSecretProvider: True

Custom provider order
GitHub -> Configuration -> Environment -> TeamCity
ResolveWithSourceAsync.Value: Server=github;Database=main;
ResolveWithSourceAsync.ProviderName: GitHubSecretProvider
ExplainSecretResolutionAsync:
- GitHubSecretProvider: True
```

## Migrating From 2.x

- Pass `IConfiguration` to `AddRaycynixSecrets(builder.Configuration)`.
- Import `SecretOptions` from `Raycynix.Extensions.Secrets.Options`.
- Replace `typeof(GitHubSecretProvider)` entries in `ProviderOrder` with stable names such as `SecretProviderNames.GitHub`.
- Provider order can now be bound from `SecretOptions:ProviderOrder` in any registered configuration source.
- TeamCity provider lookup now correctly removes the TeamCity parameter prefix: `env.ConnectionStrings.Main` is read from the process variable `ConnectionStrings.Main`.
