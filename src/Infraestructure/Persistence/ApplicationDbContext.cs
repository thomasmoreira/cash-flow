using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infraestructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Transaction> Transactions { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {        
        modelBuilder.Entity<Transaction>().HasKey(t => t.Id);
    }
}
