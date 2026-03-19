using Expense_managment.Models;
using Expense_managment.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Expense_managment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITransactionRepository _transactionRepository;

        public BudgetController(
            IBudgetRepository budgetRepository,
            ICategoryRepository categoryRepository,
            ITransactionRepository transactionRepository)
        {
            _budgetRepository = budgetRepository;
            _categoryRepository = categoryRepository;
            _transactionRepository = transactionRepository;
        }

        private int GetUserIdFromClaims()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid user token or user ID not found.");
        }

        // GET: api/Budget
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Budget>>> GetBudgets(int? month, int? year)
        {
            var userId = GetUserIdFromClaims();
            var budgets = await _budgetRepository.GetBudgetsByUserAsync(userId, month, year);
            return Ok(budgets);
        }

        // GET: api/Budget/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Budget>> GetBudget(int id)
        {
            var budget = await _budgetRepository.GetBudgetByIdAsync(id);

            if (budget == null)
            {
                return NotFound();
            }

            var userId = GetUserIdFromClaims();
            if (budget.UserId != userId)
            {
                return Forbid();
            }

            return Ok(budget);
        }

        // POST: api/Budget
        [HttpPost]
        public async Task<ActionResult<Budget>> CreateBudget(Budget budget)
        {
            var userId = GetUserIdFromClaims();
            
            // Validate category
            var category = await _categoryRepository.GetCategoryByIdAsync(budget.CategoryId, userId);
            if(category == null)
            {
                return BadRequest("Invalid Category ID");
            }

            // Check if budget already exists for this category/month/year
            var existingBudget = await _budgetRepository.GetBudgetByCategoryAsync(userId, budget.CategoryId, budget.Month, budget.Year);
            if(existingBudget != null)
            {
                return Conflict("A budget for this category in the specified month and year already exists.");
            }

            budget.UserId = userId;
            budget.User = null; // Prevent EF tracking issues
            budget.Category = null;

            await _budgetRepository.CreateBudgetAsync(budget);

            return CreatedAtAction("GetBudget", new { id = budget.BudgetId }, budget);
        }

        // PUT: api/Budget/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBudget(int id, Budget budget)
        {
            if (id != budget.BudgetId)
            {
                return BadRequest("Budget ID mismatch");
            }

            var userId = GetUserIdFromClaims();
            var existingBudget = await _budgetRepository.GetBudgetByIdAsync(id);

            if (existingBudget == null)
            {
                return NotFound();
            }

            if (existingBudget.UserId != userId)
            {
                return Forbid();
            }

            // Validate Category if it changed
            if (budget.CategoryId != existingBudget.CategoryId)
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(budget.CategoryId, userId);
                if (category == null)
                {
                    return BadRequest("Invalid Category ID");
                }
            }

            existingBudget.AmountLimit = budget.AmountLimit;
            existingBudget.Month = budget.Month;
            existingBudget.Year = budget.Year;
            existingBudget.CategoryId = budget.CategoryId;

            await _budgetRepository.UpdateBudgetAsync(existingBudget);

            return NoContent();
        }

        // DELETE: api/Budget/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            var userId = GetUserIdFromClaims();
            var existingBudget = await _budgetRepository.GetBudgetByIdAsync(id);

            if (existingBudget == null)
            {
                return NotFound();
            }

            if (existingBudget.UserId != userId)
            {
                return Forbid();
            }

            await _budgetRepository.DeleteBudgetAsync(id);

            return NoContent();
        }

        // GET: api/Budget/status
        [HttpGet("status")]
        public async Task<ActionResult<IEnumerable<object>>> GetBudgetStatus(int? month, int? year)
        {
            var userId = GetUserIdFromClaims();
            var queryMonth = month ?? DateTime.Now.Month;
            var queryYear = year ?? DateTime.Now.Year;

            var budgets = await _budgetRepository.GetBudgetsByUserAsync(userId, queryMonth, queryYear);
            
            // Get all transactions for the user for the given month/year
            var allTransactions = await _transactionRepository.GetTransactionsByUserAsync(userId);
            var monthTransactions = allTransactions.Where(t => t.Date.Month == queryMonth && t.Date.Year == queryYear);

            var statusList = new List<object>();

            foreach(var budget in budgets)
            {
                var spent = monthTransactions
                    .Where(t => t.CategoryId == budget.CategoryId)
                    .Sum(t => t.Amount);

                statusList.Add(new
                {
                    BudgetId = budget.BudgetId,
                    CategoryId = budget.CategoryId,
                    CategoryTitle = budget.Category?.Title,
                    CategoryIcon = budget.Category?.Icon,
                    AmountLimit = budget.AmountLimit,
                    AmountSpent = spent,
                    Remaining = budget.AmountLimit - spent,
                    Month = budget.Month,
                    Year = budget.Year
                });
            }

            return Ok(statusList);
        }
    }
}
