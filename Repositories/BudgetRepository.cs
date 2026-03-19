using Expense_managment.Data;
using Expense_managment.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Expense_managment.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly ApplicationDbContext _context;

        public BudgetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Budget>> GetBudgetsByUserAsync(int userId, int? month = null, int? year = null)
        {
            var query = _context.Budgets.Include(b => b.Category).Where(b => b.UserId == userId);

            if (month.HasValue)
                query = query.Where(b => b.Month == month.Value);

            if (year.HasValue)
                query = query.Where(b => b.Year == year.Value);

            return await query.ToListAsync();
        }

        public async Task<Budget?> GetBudgetByIdAsync(int budgetId)
        {
            return await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BudgetId == budgetId);
        }

        public async Task<Budget?> GetBudgetByCategoryAsync(int userId, int categoryId, int month, int year)
        {
            return await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.UserId == userId 
                    && b.CategoryId == categoryId 
                    && b.Month == month 
                    && b.Year == year);
        }

        public async Task<IEnumerable<Budget>> GetCurrentBudgetsAsync(int userId)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            return await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && b.Month == currentMonth && b.Year == currentYear)
                .ToListAsync();
        }

        public async Task<Budget> CreateBudgetAsync(Budget budget)
        {
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            return budget;
        }

        public async Task UpdateBudgetAsync(Budget budget)
        {
            _context.Entry(budget).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBudgetAsync(int budgetId)
        {
            var budget = await _context.Budgets.FindAsync(budgetId);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
            } 
        }

        public async Task<bool> BudgetExistsAsync(int budgetId)
        {
            return await _context.Budgets.AnyAsync(e => e.BudgetId == budgetId);
        }
    }
}
