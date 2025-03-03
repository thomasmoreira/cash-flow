using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CashFlow.Infraestructure.Persistence.Configuration
{
    public class DailyConsolidationConfig : IEntityTypeConfiguration<DailyConsolidation>
    {
        public void Configure(EntityTypeBuilder<DailyConsolidation> builder)
        {
            builder
                .ToTable("dailyconsolidation");

            builder.HasKey(c => c.Date);

            var dateConverter = new ValueConverter<DateTime, DateTime>(
                    v => v.Date,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                );
            
            builder.Property(d => d.Date)
                .HasConversion(dateConverter)
                .HasColumnType("DATE");

            builder.Property(d => d.Amount)
                .HasPrecision(10, 2)
                .IsRequired();
        }
    }
}
