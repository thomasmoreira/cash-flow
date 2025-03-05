using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Infraestructure.Persistence.Configuration;

public class TransactionConfig : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {

        builder.ToTable("transaction");

        builder
            .HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(t => t.Description)
            .HasColumnType("varchar")
            .HasMaxLength(100);


        builder.Property(t => t.Date)
            .HasColumnType("timestamp")
            .IsRequired();
    }
}
