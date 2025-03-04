using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
            .HasColumnType("VARCHAR")
            .HasMaxLength(100);

        var utcConverter = new ValueConverter<DateTime, DateTime>(
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc),  // Ao salvar, força UTC
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)   // Ao ler, força UTC
    );

        builder.Property(t => t.Date)
            .HasConversion(utcConverter)
            .HasColumnType("timestamptz")
            .IsRequired();
    }
}
