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
    public class WalletController : ControllerBase
    {
        private readonly IWalletRepository _walletRepository;

        public WalletController(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // GET: api/Wallet
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Wallet>>> GetWallets()
        {
            var userId = GetUserId();
            var wallets = await _walletRepository.GetWalletsByUserAsync(userId);
            return Ok(wallets);
        }

        // GET: api/Wallet/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Wallet>> GetWallet(int id)
        {
            var userId = GetUserId();
            var wallet = await _walletRepository.GetWalletByIdAsync(id, userId);

            if (wallet == null)
            {
                return NotFound();
            }

            return Ok(wallet);
        }

        // POST: api/Wallet
        public class WalletInputModel
        {
            public string Name { get; set; }
            public decimal InitialBalance { get; set; } = 0;
        }

        [HttpPost]
        public async Task<ActionResult<Wallet>> PostWallet(WalletInputModel input)
        {
            var userId = GetUserId();

            var wallet = new Wallet
            {
                Name = input.Name,
                InitialBalance = input.InitialBalance,
                UserId = userId
            };

            var createdWallet = await _walletRepository.CreateWalletAsync(wallet);

            return CreatedAtAction(nameof(GetWallet), new { id = createdWallet.WalletId }, createdWallet);
        }

        // PUT: api/Wallet/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWallet(int id, WalletInputModel input)
        {
            var userId = GetUserId();
            var wallet = await _walletRepository.GetWalletByIdAsync(id, userId);

            if (wallet == null)
            {
                return NotFound();
            }

            wallet.Name = input.Name;
            wallet.InitialBalance = input.InitialBalance;

            await _walletRepository.UpdateWalletAsync(wallet);

            return NoContent();
        }

        // DELETE: api/Wallet/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWallet(int id)
        {
            var userId = GetUserId();
            var success = await _walletRepository.DeleteWalletAsync(id, userId);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
