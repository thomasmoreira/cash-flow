using CashFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlow.Infraestructure.Persistence.Configuration
{
    public class DailyConsolidationConfig : IEntityTypeConfiguration<DailyConsolidation>
    {
        public void Configure(EntityTypeBuilder<DailyConsolidation> builder)
        {
            builder
                .ToTable("dailyconsolidation");

            builder.HasKey(c => c.Date);
            
            builder.Property(d => d.Date)
                .HasColumnType("timestamp");

            builder.Property(d => d.Amount)
                .HasPrecision(10, 2)
                .IsRequired();
        }
    }
}
