using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

var options = new DbContextOptionsBuilder()
    .UseSqlServer("Data Source = .\\SQLEXPRESS; Database = EfCoreContructorBinding; Integrated Security = True; TrustServerCertificate=True");

var dbContext = new ApplicationDbContext(options.Options);
await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.EnsureCreatedAsync();



//
// Using .OwnsOne() with .ToJson() throws an ArgumentException
//
// The exception occurs with an enum parameter.
//
// 'CustomerVerificationStatus' cannot be used for constructor parameter of type 'System.Nullable`1[CustomerVerificationStatus]' Arg_ParamName_Name'
//
await dbContext.Customers.ToListAsync();

Console.Read();




public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLoggerFactory(LoggerFactory.Create(x => x
                .AddConsole()
                .AddFilter(y => y >= LogLevel.Debug)))
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .OwnsOne(x => x.Verification, x =>
            {
                //
                // Works without .ToJson()
                //
                x.ToJson();
            });

        base.OnModelCreating(modelBuilder);
    }
}

public class Customer
{
    public Customer(string email, CustomerStatus? status)
    {
        Email = email;
        Status = status;
    }

    public int Id { get; private set; }
    public string Email { get; private set; }
    public CustomerStatus? Status { get; private set; }
    public CustomerVerification? Verification { get; set; }
}

public class CustomerVerification
{
    public CustomerVerification(CustomerVerificationStatus? status, string text, DateTime creationDateTime)
    {
        Status = status;
        Text = text;
        CreationDateTime = creationDateTime;
    }

    public CustomerVerificationStatus? Status { get; private set; }
    public string Text { get; private set; }
    public DateTime CreationDateTime { get; private set; }
}

public enum CustomerStatus
{
    Active,
    Inactive
}

public enum CustomerVerificationStatus
{
    None = 1,
    Verified = 2
}