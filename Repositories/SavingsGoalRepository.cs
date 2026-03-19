using Expense_managment.Data;
using Expense_managment.Models;
using Microsoft.EntityFrameworkCore;

namespace Expense_managment.Repositories
{
    public class SavingsGoalRepository : ISavingsGoalRepository
    {
        private readonly ApplicationDbContext _context;

        public SavingsGoalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SavingsGoal>> GetSavingsGoalsByUserAsync(int userId)
        {
            return await _context.SavingsGoals
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();
        }

        public async Task<SavingsGoal?> GetSavingsGoalByIdAsync(int goalId, int userId)
        {
            return await _context.SavingsGoals
                .FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);
        }

        public async Task<SavingsGoal> AddAsync(SavingsGoal goal)
        {
            _context.SavingsGoals.Add(goal);
            await _context.SaveChangesAsync();
            return goal;
        }

        public async Task UpdateAsync(SavingsGoal goal)
        {
            _context.Entry(goal).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int goalId, int userId)
        {
            var goal = await GetSavingsGoalByIdAsync(goalId, userId);
            if (goal == null) return false;

            _context.SavingsGoals.Remove(goal);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
