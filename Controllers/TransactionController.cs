using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Expense_managment.Models;
using Expense_managment.Repositories;
using System.Security.Claims;

namespace Expense_managment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;

        public TransactionController(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        public class TransactionInputModel
        {
            public int CategoryId { get; set; }
            public int Amount { get; set; }
            public string? Note { get; set; }
            public DateTime Date { get; set; } = DateTime.Now;
            public bool IsRecurring { get; set; }
            public string? RecurrenceFrequency { get; set; }
            public int? WalletId { get; set; }
        }

        // GET: api/Transaction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
        {
            var userId = GetUserId();
            var transactions = await _transactionRepository.GetTransactionsByUserAsync(userId);
            return Ok(transactions);
        }

        // GET: api/Transaction/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            var userId = GetUserId();
            var transaction = await _transactionRepository.GetTransactionByIdAsync(id, userId);

            if (transaction == null)
            {
                return NotFound();
            }

            return Ok(transaction);
        }

        // POST: api/Transaction
        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction(TransactionInputModel input)
        {
            var userId = GetUserId();

            // Optional: verify category belongs to user
            var category = await _categoryRepository.GetCategoryByIdAsync(input.CategoryId, userId);
            if (category == null)
            {
                 return BadRequest("Invalid Category ID");
            }

            var transaction = new Transaction
            {
                UserId = userId,
                CategoryId = input.CategoryId,
                Amount = input.Amount,
                Note = input.Note,
                Date = input.Date,
                IsRecurring = input.IsRecurring,
                RecurrenceFrequency = input.IsRecurring ? input.RecurrenceFrequency : null,
                WalletId = input.WalletId
            };

            if (transaction.IsRecurring)
            {
                 transaction.NextRecurrenceDate = CalculateNextDate(transaction.Date, transaction.RecurrenceFrequency);
            }

            var createdTransaction = await _transactionRepository.AddAsync(transaction);

            return CreatedAtAction(nameof(GetTransaction), new { id = createdTransaction.TransactionId }, createdTransaction);
        }

        // PUT: api/Transaction/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int id, TransactionInputModel input)
        {
            var userId = GetUserId();

             // Verify Ownership
            var transaction = await _transactionRepository.GetTransactionByIdAsync(id, userId);
            if (transaction == null)
            {
                return NotFound();
            }

            var category = await _categoryRepository.GetCategoryByIdAsync(input.CategoryId, userId);
            if (category == null)
            {
                 return BadRequest("Invalid Category ID");
            }

            transaction.CategoryId = input.CategoryId;
            transaction.Amount = input.Amount;
            transaction.Note = input.Note;
            transaction.Date = input.Date;
            transaction.WalletId = input.WalletId;
            
            // Only update recurrence if the existing tx is the template, OR if changing a non-recurring tx to recurring
            // (Business logic might vary here, but this is a standard approach)
            if(input.IsRecurring != transaction.IsRecurring || input.RecurrenceFrequency != transaction.RecurrenceFrequency)
            {
                transaction.IsRecurring = input.IsRecurring;
                transaction.RecurrenceFrequency = input.IsRecurring ? input.RecurrenceFrequency : null;
                
                if (transaction.IsRecurring)
                     transaction.NextRecurrenceDate = CalculateNextDate(transaction.Date, transaction.RecurrenceFrequency);
                else
                     transaction.NextRecurrenceDate = null;
            }

            await _transactionRepository.UpdateAsync(transaction);

            return NoContent();
        }

        private DateTime CalculateNextDate(DateTime currentDate, string? frequency)
        {
            return frequency?.ToLower() switch
            {
                "daily" => currentDate.AddDays(1),
                "weekly" => currentDate.AddDays(7),
                "monthly" => currentDate.AddMonths(1),
                "yearly" => currentDate.AddYears(1),
                _ => currentDate.AddMonths(1) // Fallback
            };
        }

        // DELETE: api/Transaction/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var userId = GetUserId();
            var success = await _transactionRepository.DeleteAsync(id, userId);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
