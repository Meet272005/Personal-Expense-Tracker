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
    public class SavingsGoalController : ControllerBase
    {
        private readonly ISavingsGoalRepository _savingsGoalRepository;

        public SavingsGoalController(ISavingsGoalRepository savingsGoalRepository)
        {
            _savingsGoalRepository = savingsGoalRepository;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SavingsGoal>>> GetSavingsGoals()
        {
            var userId = GetUserId();
            var goals = await _savingsGoalRepository.GetSavingsGoalsByUserAsync(userId);
            return Ok(goals);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SavingsGoal>> GetSavingsGoal(int id)
        {
            var userId = GetUserId();
            var goal = await _savingsGoalRepository.GetSavingsGoalByIdAsync(id, userId);

            if (goal == null)
            {
                return NotFound();
            }

            return Ok(goal);
        }

        public class SavingsGoalInputModel
        {
            public string Title { get; set; }
            public decimal TargetAmount { get; set; }
            public decimal CurrentAmount { get; set; }
            public DateTime TargetDate { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult<SavingsGoal>> PostSavingsGoal(SavingsGoalInputModel input)
        {
            var userId = GetUserId();

            var goal = new SavingsGoal
            {
                UserId = userId,
                Title = input.Title,
                TargetAmount = input.TargetAmount,
                CurrentAmount = input.CurrentAmount,
                TargetDate = input.TargetDate
            };

            var created = await _savingsGoalRepository.AddAsync(goal);
            return CreatedAtAction(nameof(GetSavingsGoal), new { id = created.GoalId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSavingsGoal(int id, SavingsGoalInputModel input)
        {
            var userId = GetUserId();
            var goal = await _savingsGoalRepository.GetSavingsGoalByIdAsync(id, userId);

            if (goal == null)
            {
                return NotFound();
            }

            goal.Title = input.Title;
            goal.TargetAmount = input.TargetAmount;
            goal.CurrentAmount = input.CurrentAmount;
            goal.TargetDate = input.TargetDate;

            await _savingsGoalRepository.UpdateAsync(goal);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSavingsGoal(int id)
        {
            var userId = GetUserId();
            var success = await _savingsGoalRepository.DeleteAsync(id, userId);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
