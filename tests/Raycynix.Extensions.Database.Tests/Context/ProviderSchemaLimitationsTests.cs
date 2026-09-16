using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Infrastructure;

namespace Raycynix.Extensions.Database.Tests.Context;

public sealed class ProviderSchemaLimitationsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Provider_ShouldUseNativeSchemaSemantics(bool mysql)
    {
        var options = new DbContextOptionsBuilder();
        if (mysql)
            options.UseMySQL("Server=localhost;Database=schema_tests;User ID=test;Password=test");
        else
            options.UseSqlite("Data Source=:memory:");
        using var context = new SchemaContext(options.Options);

        var table = mysql ? "`sales`.`items`" : "\"items\"";
        context.Set<Item>().ToQueryString().Should().Contain($"FROM {table} AS");
        context.Database.GenerateCreateScript().Should().Contain($"CREATE TABLE {table}");
    }

    private sealed class SchemaContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<Item>().EntityName("items").EntitySchema("sales");
    }

    private sealed class Item
    {
        public int Id { get; set; }
    }
}
