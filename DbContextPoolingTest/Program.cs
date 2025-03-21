// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using DbContextPoolingTest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


var serviceCollection = new ServiceCollection();
serviceCollection.AddDbContextPool<PoolTestDbContext>(options =>
    options.UseSqlite("Data Source=test.db"));
    
var serviceProvider = serviceCollection.BuildServiceProvider();

using (var scope = serviceProvider.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PoolTestDbContext>();
    await context.Database.EnsureCreatedAsync();

    if (!await context.People.AnyAsync())
    {
        await context.People.AddRangeAsync(new Person { Id = 1, Name = "John Langley" },
            new Person { Id = 2, Name = "Jennifer Langley" });
        await context.SaveChangesAsync();
    }
}

// Track connection IDs to check for reuse
var connectionIds = new ConcurrentBag<int>();

Console.WriteLine("Tests db context pool where each execution gets its own scope");
Console.WriteLine();

// Parallel execution
Parallel.ForEach(Enumerable.Range(1, 10), i =>
{
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PoolTestDbContext>();

    // Fetch the database connection and check if it's the same across iterations
    var connection = dbContext.Database.GetDbConnection();
    connectionIds.Add(connection.GetHashCode());

    Console.WriteLine($"Iteration {i}: Database Connection Hash - {connection.GetHashCode()}");
});

// Check for unique connection IDs
Console.WriteLine();
Console.WriteLine($"Total unique connections: {connectionIds.Distinct().Count()}");
Console.WriteLine();

connectionIds.Clear();

Console.WriteLine("Tests db context pool where each execution uses parent scope");
Console.WriteLine();

using (var scope = serviceProvider.CreateScope())
{
    Parallel.ForEach(Enumerable.Range(1, 10), i =>
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PoolTestDbContext>();
        // Fetch the database connection and check if it's the same across iterations
        var connection = dbContext.Database.GetDbConnection();
        connectionIds.Add(connection.GetHashCode());

        Console.WriteLine($"Iteration {i}: Connection Hash - {connection.GetHashCode()}");
    });
}

// Check for unique connection IDs
Console.WriteLine();
Console.WriteLine($"Total unique connections: {connectionIds.Distinct().Count()}");
