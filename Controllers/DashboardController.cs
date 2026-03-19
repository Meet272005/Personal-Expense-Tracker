using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Expense_managment.Repositories;
using System.Security.Claims;

namespace Expense_managment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;

        public DashboardController(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var userId = GetUserId();
            var transactions = await _transactionRepository.GetTransactionsByUserAsync(userId);

            // Total Income
            int totalIncome = transactions
                .Where(t => t.Category != null && t.Category.Type == "Income")
                .Sum(t => t.Amount);

            // Total Expense
            int totalExpense = transactions
                .Where(t => t.Category != null && t.Category.Type == "Expense")
                .Sum(t => t.Amount);

            // Balance
            int balance = totalIncome - totalExpense;

            // Doughnut Chart Data (Expense by Category)
            var expenseByCategory = transactions
                .Where(t => t.Category != null && t.Category.Type == "Expense")
                .GroupBy(t => t.Category!.Title)
                .Select(g => new
                {
                    CategoryTitleWithIcon = g.First().Category!.Icon + " " + g.Key,
                    Amount = g.Sum(t => t.Amount)
                })
                .ToList();

            // Spline Chart Data (Income vs Expense over last 30 days)
            // Can be extended based on actual UI needs.
            var last30Days = DateTime.Today.AddDays(-30);
            
            var splineChartData = transactions
                .Where(t => t.Date >= last30Days)
                .GroupBy(t => t.Date.Date)
                .Select(g => new
                {
                    Day = g.Key.ToString("dd-MMM"),
                    Income = g.Where(t => t.Category != null && t.Category.Type == "Income").Sum(t => t.Amount),
                    Expense = g.Where(t => t.Category != null && t.Category.Type == "Expense").Sum(t => t.Amount)
                })
                .OrderBy(d => d.Day)
                .ToList();


            return Ok(new
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                Balance = balance,
                ExpenseByCategory = expenseByCategory,
                SplineChartData = splineChartData
            });
        }
    }
}
