using Microsoft.EntityFrameworkCore;
using Expense_managment.Data;
using Expense_managment.Models;

namespace Expense_managment.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByUserAsync(int userId)
        {
            return await _context.Transactions
                .Include(t => t.Category) // Include Category for UI rendering
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id, int userId)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);
        }

        public async Task<Transaction> AddAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction> UpdateAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var transaction = await GetTransactionByIdAsync(id, userId);
            if (transaction == null) return false;

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Transaction>> GetPendingRecurringTransactionsAsync(DateTime date)
        {
            return await _context.Transactions
                .Where(t => t.IsRecurring && t.NextRecurrenceDate.HasValue && t.NextRecurrenceDate.Value.Date <= date.Date)
                .ToListAsync();
        }
    }
}
