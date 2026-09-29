using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raycynix.Extensions.Database;
using Raycynix.Extensions.Database.Example;
using Raycynix.Extensions.Database.Hosting;
using Raycynix.Extensions.Database.Sqlite;
using Raycynix.Extensions.Serilog;

Environment.CurrentDirectory = AppContext.BaseDirectory;

var builder = Host.CreateDefaultBuilder(args);

builder
    .UseRaycynixSerilog()
    .ConfigureServices((context, services) =>
    {
        services.AddRaycynixDatabase(context.Configuration, "Application")
            .AddSqlite();

        services.AddHostedService<DatabaseExampleWorker>();
    });

var host = builder.Build();

await host.InitializeRaycynixDatabaseAsync();
await host.RunAsync();