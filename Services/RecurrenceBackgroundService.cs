using Expense_managment.Models;
using Expense_managment.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Expense_managment.Services
{
    public class RecurrenceBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RecurrenceBackgroundService> _logger;

        public RecurrenceBackgroundService(IServiceProvider serviceProvider, ILogger<RecurrenceBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Execute once on startup, then periodically
            await DoWork(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // Run once a day
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                await DoWork(stoppingToken);
            }
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RecurrenceBackgroundService is running.");

            using (var scope = _serviceProvider.CreateScope())
            {
                var transactionRepo = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                var today = DateTime.Now.Date;

                try
                {
                    var pendingTransactions = await transactionRepo.GetPendingRecurringTransactionsAsync(today);

                    foreach (var templateTx in pendingTransactions)
                    {
                        if (templateTx.NextRecurrenceDate == null) continue;

                        int periodsPassed = 0;
                        DateTime currentProcessDate = templateTx.NextRecurrenceDate.Value.Date;

                        // It's possible multiple recurrences were missed if the app was offline
                        while (currentProcessDate <= today.Date)
                        {
                            // Create new transaction instance
                            var newTx = new Transaction
                            {
                                UserId = templateTx.UserId,
                                CategoryId = templateTx.CategoryId,
                                Amount = templateTx.Amount,
                                Note = templateTx.Note + " (Auto-Recurring)",
                                Date = currentProcessDate, // Set date to when it SHOULD have occurred
                                IsRecurring = false,       // The new instance is NOT recurring itself
                                RecurrenceFrequency = null,
                                NextRecurrenceDate = null
                            };

                            await transactionRepo.AddAsync(newTx);

                            // Calculate next date for the loop
                            currentProcessDate = CalculateNextDate(currentProcessDate, templateTx.RecurrenceFrequency);
                            periodsPassed++;
                        }

                        if (periodsPassed > 0)
                        {
                            // Update the template with the new future date
                            templateTx.NextRecurrenceDate = currentProcessDate;
                            await transactionRepo.UpdateAsync(templateTx);
                            
                            _logger.LogInformation($"Processed {periodsPassed} recurrences for TransactionId {templateTx.TransactionId}");
                        }
                    }
                }
                catch (Exception ex)
                {
                     _logger.LogError(ex, "Error occurred executing RecurrenceBackgroundService.");
                }
            }
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
    }
}
