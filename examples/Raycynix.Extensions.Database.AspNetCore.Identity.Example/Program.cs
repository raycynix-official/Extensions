using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.AspNetCore;
using Raycynix.Extensions.Database.AspNetCore.Identity;
using Raycynix.Extensions.Database.AspNetCore.Identity.Example.Models;
using Raycynix.Extensions.Database.Sqlite;
using Raycynix.Extensions.Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.AddRaycynixSerilog(options =>
{
    options.Options.DefaultConsoleOutputTemplate =
        "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{ServiceName}] [{ServiceVersion}] [Env:{Environment}] {Message:lj}{NewLine}{Exception}";
});

builder.Services
    .AddRaycynixIdentityDatabase(builder.Configuration, "Identity")
    .AddSqlite();

var app = builder.Build();

await app.InitializeRaycynixDatabaseAsync();

app.MapGet("/", () => Results.Ok(new
{
    Service = "Raycynix.Extensions.Database.AspNetCore.Example",
    Endpoints = new[]
    {
        "GET /orders",
        "GET /orders/{number}",
        "POST /orders"
    }
}));

app.MapGet("/orders",
    async ([FromServices] RaycynixIdentityDatabaseContext databaseContext, CancellationToken cancellationToken) =>
    {
        var orders = await databaseContext.Set<ExampleOrder>()
            .AsNoTracking()
            .OrderBy(current => current.CreatedAt)
            .Select(current => new ExampleOrderResponse(
                current.Number,
                current.CustomerId,
                current.TotalAmount,
                current.Status,
                current.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(orders);
    });

app.MapGet("/orders/{number}", async (
    string number,
    [FromServices] RaycynixIdentityDatabaseContext databaseContext,
    ILoggerFactory loggerFactory,
    CancellationToken cancellationToken) =>
{
    var logger = loggerFactory.CreateLogger("OrdersEndpoint");
    logger.LogInformation("Loading order {OrderNumber}", number);

    var order = await databaseContext.Set<ExampleOrder>()
        .AsNoTracking()
        .Where(current => current.Number == number)
        .Select(current => new ExampleOrderResponse(
            current.Number,
            current.CustomerId,
            current.TotalAmount,
            current.Status,
            current.CreatedAt))
        .FirstOrDefaultAsync(cancellationToken);

    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.MapPost("/orders", async (
    CreateExampleOrderRequest request,
    [FromServices] RaycynixIdentityDatabaseContext databaseContext,
    ILogger<OrderEndpoints> logger,
    CancellationToken cancellationToken) =>
{
    var order = new ExampleOrder
    {
        Id = Guid.NewGuid(),
        Number = $"ORD-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(1000, 9999)}",
        CustomerId = request.CustomerId,
        TotalAmount = request.TotalAmount,
        Status = "Pending",
        CreatedAt = DateTime.UtcNow
    };

    databaseContext.Set<ExampleOrder>().Add(order);
    await databaseContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation("Created order through HTTP endpoint {@Endpoint}", new
    {
        order.Number,
        order.CustomerId,
        order.TotalAmount
    });

    var response = new ExampleOrderResponse(
        order.Number,
        order.CustomerId,
        order.TotalAmount,
        order.Status,
        order.CreatedAt);

    return Results.Created($"/orders/{order.Number}", response);
});

app.Run();