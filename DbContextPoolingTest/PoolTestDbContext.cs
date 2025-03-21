using Microsoft.EntityFrameworkCore;

namespace DbContextPoolingTest;

public class PoolTestDbContext(DbContextOptions<PoolTestDbContext> options) : DbContext(options)
{
    public DbSet<Person> People { get; set; }

}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
}