using CashFlow.Domain.Entities;
using CashFlow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Infraestructure.Persistence.Repositories
{
    public class ConsolidatingRepository : IConsolidatingRepository
    {
        private readonly ApplicationDbContext _context;

        public ConsolidatingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DailyConsolidation dailyConsolidation)
        {
            await _context.DailyConsolidations.AddAsync(dailyConsolidation);
            await _context.SaveChangesAsync();
        }

        public Task<DailyConsolidation?> GetDailyConsolidatingAsync(DateTime date)
        {
            return _context.DailyConsolidations
                .Where(d => d.Date.Date == date.Date)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(DailyConsolidation dailyConsolidation)
        {
            _context.DailyConsolidations.Entry(dailyConsolidation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}