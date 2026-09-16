# Raycynix.Extensions.Contracts

This package follows the shared version in [Directory.Build.props](../../Directory.Build.props). See [CHANGELOG.md](CHANGELOG.md) for release history.

`Raycynix.Extensions.Contracts` contains reusable contract models and versioning conventions for shared .NET APIs.

## Package

- Target framework: `net10.0`
- Transport-neutral and independent of ASP.NET Core

## What it contains

- `Money`
- `Quantity`
- `UoM`
- `PagingRequest`
- `PageInfo`
- `PagedResult<TItem>`
- `ErrorContract`
- `ValidationError`
- `ContractVersion`
- `ContractMetadata`
- `VersionedContract<TContract>`
- `ContractHeaders`
- `ContractIntroducedAttribute`
- `ContractDeprecatedAttribute`
- DTO and contract versioning conventions for cross-service APIs
- validation-friendly annotations for common contracts
- a shared transport error model for reusable APIs

## What it does not contain

- `master data` sources
- reference data synchronization
- service-specific DTOs
- API gateway contracts
- transport-specific ASP.NET Core middleware
- logging or runtime diagnostics

## Contract Rules

- shared DTOs must be backward compatible within the same major contract version
- new fields must be additive and optional for existing consumers
- existing public fields must not be removed or renamed inside the same major contract version
- breaking changes require a new contract version
- `Major` changes are breaking changes
- `Minor` changes are additive, backward-compatible changes
- `Patch` changes are non-breaking fixes that do not alter the contract shape
- contract models must stay serialization-friendly and avoid behavior-heavy logic
- cross-service reusable types belong here, service-local DTOs do not
- contract identifiers and versions should be explicit at transport boundaries when contracts are shared across services
- public contracts should expose validation metadata that common frameworks can consume consistently

## Change Rules

- do not remove or rename public contract fields inside the same major version
- deprecate old fields before removal instead of deleting them immediately
- introduce replacement fields as additive optional members first
- use a new major contract version only when compatibility cannot be preserved
- keep deprecated members readable long enough for existing consumers to migrate
- document when a field was introduced and when it became deprecated

## Usage

```csharp
var price = new Money
{
    Amount = 149.99m,
    Currency = "USD"
};

var quantity = new Quantity
{
    Value = 12.5m,
    UoM = new UoM
    {
        Code = "kg",
        Name = "Kilogram"
    }
};

var result = new PagedResult<Money>
{
    Items = new[] { price },
    PageInfo = new PageInfo
    {
        Page = 1,
        PageSize = 20,
        TotalCount = 1,
        TotalPages = 1,
        HasPreviousPage = false,
        HasNextPage = false
    }
};

var versioned = new VersionedContract<PagedResult<Money>>
{
    Metadata = new ContractMetadata
    {
        Name = "catalog.prices",
        Version = new ContractVersion
        {
            Major = 1,
            Minor = 0,
            Patch = 0
        }
    },
    Payload = result
};
```

## Contract Headers

Use the shared header names when contracts cross transport boundaries explicitly:

```csharp
var headers = new Dictionary<string, string?>
{
    [ContractHeaders.ContractName] = "catalog.prices",
    [ContractHeaders.ContractVersion] = "1.2.0"
};
```

## Versioning

Use `ContractVersion` to express the current shared contract version:

```csharp
var version = new ContractVersion
{
    Major = 1,
    Minor = 2,
    Patch = 0
};

var parsed = ContractVersion.Parse("1.2.0");
var isCompatibleLine = parsed >= version;
```

When contract metadata must cross process boundaries explicitly, use:

- `ContractHeaders.ContractName`
- `ContractHeaders.ContractVersion`

This package only defines the common contract model and conventions. It does not enforce transport-specific version negotiation by itself.

`ContractVersion` also supports parsing, comparison, and equality to help consumers implement consistent compatibility checks in their own services.

`ContractVersion.Initial` returns a fresh `1.0.0` value on every access. Mutating one returned instance
does not change the initial version observed by other consumers.

## Error Contracts

Use `ErrorContract` as the shared transport shape for failures:

```csharp
var error = new ErrorContract
{
    Code = "validation_failed",
    Message = "One or more validation errors occurred.",
    TraceId = "00-7d9f6f8f53fd8a8ce6d4cfd21483ca5f-b9d0f6f6bd2f5f61-01",
    ValidationErrors =
    [
        new ValidationError
        {
            Field = "pageSize",
            Code = "out_of_range",
            Message = "Page size must be greater than zero."
        }
    ]
};
```

This package intentionally keeps the error contract generic so it can be reused in HTTP APIs, messaging, and internal service boundaries.

## Validation Metadata

The built-in contract models expose `System.ComponentModel.DataAnnotations` attributes and lightweight `IsValid()` checks so consumers can use them with ASP.NET Core, manual validation flows, or custom guards without introducing transport-specific behavior into the contracts themselves.

Runtime `IsValid()` checks also safely reject required members that were set to `null` by malformed
deserialization input. Contract evolution attributes parse their version arguments immediately, so invalid
values such as `1.0` or `v1.0.0` fail during contract declaration.

## ASP.NET Core

If you want HTTP header integration, endpoint metadata, controller/minimal API helpers, or `ModelState` conversion helpers, use the companion package `Raycynix.Extensions.Contracts.AspNetCore`.

In that package:

- declare a contract on a Minimal API endpoint with `.WithContract(...)`
- declare a contract on MVC actions/controllers with `[Contract(...)]`
- enable `UseRaycynixContracts()` so the declared contract is written to HTTP headers
- return `httpContext.VersionedContract(...)` or `this.VersionedContract(...)` when you want the response body wrapped into `VersionedContract<T>` without duplicating metadata inside the handler

## Marking Contract Evolution

Use the attributes in this package to mark contract evolution directly on shared DTOs:

```csharp
public class CatalogPriceDto
{
    [ContractIntroduced("1.0.0")]
    public string ProductId { get; set; } = string.Empty;

    [ContractIntroduced("1.2.0")]
    public Money? DiscountPrice { get; set; }

    [ContractDeprecated("1.3.0", RemovalVersion = "2.0.0", Reason = "Use DiscountPrice instead.")]
    public decimal? DiscountAmount { get; set; }
}
```

Recommended evolution flow:

1. add a new optional field
2. keep the old field for compatibility
3. mark the old field as deprecated
4. remove it only in the next breaking contract version
