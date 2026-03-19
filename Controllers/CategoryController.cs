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
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        public class CategoryInputModel
        {
            public string Title { get; set; }
            public string Icon { get; set; }
            public string Type { get; set; }
        }

        // GET: api/Category
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var userId = GetUserId();
            var categories = await _categoryRepository.GetCategoriesByUserAsync(userId);
            return Ok(categories);
        }

        // GET: api/Category/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var userId = GetUserId();
            var category = await _categoryRepository.GetCategoryByIdAsync(id, userId);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        // POST: api/Category
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(CategoryInputModel input)
        {
            var userId = GetUserId();
            var category = new Category
            {
                UserId = userId,
                Title = input.Title,
                Icon = input.Icon,
                Type = input.Type
            };

            var createdCategory = await _categoryRepository.AddAsync(category);

            return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.CategoryId }, createdCategory);
        }

        // PUT: api/Category/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, CategoryInputModel input)
        {
            var userId = GetUserId();

            // Verify Ownership
            var category = await _categoryRepository.GetCategoryByIdAsync(id, userId);
            if (category == null)
            {
                return NotFound();
            }

            category.Title = input.Title;
            category.Icon = input.Icon;
            category.Type = input.Type;

            await _categoryRepository.UpdateAsync(category);

            return NoContent();
        }

        // DELETE: api/Category/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = GetUserId();
            var success = await _categoryRepository.DeleteAsync(id, userId);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
