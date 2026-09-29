using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database;
using Raycynix.Extensions.Database.AspNetCore;
using Raycynix.Extensions.Database.AspNetCore.Example;
using Raycynix.Extensions.Database.Sqlite;
using Raycynix.Extensions.Serilog;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var builder = WebApplication.CreateBuilder(args);

builder.AddRaycynixSerilog(options =>
    options.Options.DefaultConsoleOutputTemplate =
        "[{Timestamp:HH:mm:ss}] [{Level:u3}] [{ServiceName}] [{ServiceVersion}] [Env:{Environment}] {Message:lj}{NewLine}{Exception}");

builder.Services
    .AddRaycynixDatabase(builder.Configuration, "Application")
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
    async ([FromServices] RaycynixDatabaseContext databaseContext, CancellationToken cancellationToken) =>
    {
        var orders = await databaseContext.Set<ExampleOrder>()
            .AsNoTracking()
            .OrderBy(current => current.CreatedAt)
            .Select(current => new ExampleOrderResponse(
                current.Number,
                current.CustomerName,
                current.TotalAmount,
                current.Status,
                current.CreatedAt))
            .ToListAsync(cancellationToken);

        return Results.Ok(orders);
    });

app.MapGet("/orders/{number}", async (
    string number,
    [FromServices] RaycynixDatabaseContext databaseContext,
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
            current.CustomerName,
            current.TotalAmount,
            current.Status,
            current.CreatedAt))
        .FirstOrDefaultAsync(cancellationToken);

    return order is null ? Results.NotFound() : Results.Ok(order);
});

app.MapPost("/orders", async (
    CreateExampleOrderRequest request,
    [FromServices] RaycynixDatabaseContext databaseContext,
    ILogger<OrderEndpoints> logger,
    CancellationToken cancellationToken) =>
{
    var order = new ExampleOrder
    {
        Id = Guid.NewGuid(),
        Number = $"ORD-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(1000, 9999)}",
        CustomerName = request.CustomerName,
        TotalAmount = request.TotalAmount,
        Status = "Pending",
        CreatedAt = DateTime.UtcNow
    };

    databaseContext.Set<ExampleOrder>().Add(order);
    await databaseContext.SaveChangesAsync(cancellationToken);

    logger.LogInformation("Created order through HTTP endpoint {@Endpoint}", new
    {
        order.Number,
        order.CustomerName,
        order.TotalAmount
    });

    var response = new ExampleOrderResponse(
        order.Number,
        order.CustomerName,
        order.TotalAmount,
        order.Status,
        order.CreatedAt);

    return Results.Created($"/orders/{order.Number}", response);
});

app.Run();

internal abstract record CreateExampleOrderRequest(string CustomerName, decimal TotalAmount);

internal sealed record ExampleOrderResponse(
    string Number,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTimeOffset CreatedAtUtc);

internal abstract class OrderEndpoints;