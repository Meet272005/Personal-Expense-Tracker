using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Expense_managment.Models;
using Expense_managment.Repositories;
using System.Security.Claims;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Expense_managment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DataExportController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;

        public DataExportController(ITransactionRepository transactionRepository, ICategoryRepository categoryRepository)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        // ---------- CSV EXPORT ----------
        [HttpGet("csv")]
        public async Task<IActionResult> ExportCsv()
        {
            var userId = GetUserId();
            var transactions = await _transactionRepository.GetTransactionsByUserAsync(userId);

            var exportData = transactions.Select(t => new
            {
                t.TransactionId,
                Date = t.Date.ToString("yyyy-MM-dd"),
                Category = t.Category?.Title ?? "Unknown",
                Type = t.Category?.Type ?? "Expense",
                t.Amount,
                t.Note,
                Wallet = t.Wallet?.Name ?? "None",
                IsRecurring = t.IsRecurring ? "Yes" : "No"
            }).ToList();

            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csvWriter = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture));

            csvWriter.WriteRecords(exportData);
            await streamWriter.FlushAsync();

            return File(memoryStream.ToArray(), "text/csv", $"Transactions_Export_{DateTime.Now:yyyyMMdd}.csv");
        }

        // ---------- PDF EXPORT ----------
        [HttpGet("pdf")]
        public async Task<IActionResult> ExportPdf()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var userId = GetUserId();
            var transactions = await _transactionRepository.GetTransactionsByUserAsync(userId);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Text("Transaction History Report")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Date
                            columns.RelativeColumn(3); // Category
                            columns.RelativeColumn(3); // Wallet
                            columns.RelativeColumn(4); // Note
                            columns.RelativeColumn(2); // Amount
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Text("Date").SemiBold();
                            header.Cell().Text("Category").SemiBold();
                            header.Cell().Text("Wallet").SemiBold();
                            header.Cell().Text("Note").SemiBold();
                            header.Cell().AlignRight().Text("Amount").SemiBold();
                            
                            header.Cell().ColumnSpan(5)
                                .PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                        });

                        // Data
                        foreach (var t in transactions)
                        {
                            table.Cell().Text(t.Date.ToString("MMM dd, yyyy"));
                            table.Cell().Text(t.Category?.Title ?? "Unknown");
                            table.Cell().Text(t.Wallet?.Name ?? "-");
                            table.Cell().Text(t.Note ?? "-");
                            
                            var amountText = (t.Category?.Type == "Expense" ? "-" : "+") + t.Amount.ToString("C0");
                            table.Cell().AlignRight().Text(amountText)
                                .FontColor(t.Category?.Type == "Expense" ? Colors.Red.Darken2 : Colors.Green.Darken2);
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Transactions_Report_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public class CsvImportModel
        {
            public DateTime Date { get; set; }
            public string Category { get; set; }
            public int Amount { get; set; }
            public string Note { get; set; }
        }

        // ---------- CSV IMPORT ----------
        [HttpPost("csv-import")]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty or not provided.");

            var userId = GetUserId();
            var categories = await _categoryRepository.GetCategoriesByUserAsync(userId);
            var categoryDict = categories.ToDictionary(c => c.Title.ToLower(), c => c.CategoryId);
            
            // Define a fallback category if the user doesn't have the imported one
            var defaultCategory = categories.FirstOrDefault();
            if (defaultCategory == null) return BadRequest("Please create at least one category before importing.");

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HeaderValidated = null, MissingFieldFound = null });

            var records = csv.GetRecords<CsvImportModel>().ToList();
            var importCount = 0;

            foreach (var record in records)
            {
                int catId = defaultCategory.CategoryId;
                if (!string.IsNullOrEmpty(record.Category) && categoryDict.ContainsKey(record.Category.ToLower()))
                {
                    catId = categoryDict[record.Category.ToLower()];
                }

                var transaction = new Transaction
                {
                    UserId = userId,
                    CategoryId = catId,
                    Amount = record.Amount,
                    Note = record.Note,
                    Date = record.Date,
                    IsRecurring = false
                };

                await _transactionRepository.AddAsync(transaction);
                importCount++;
            }

            return Ok(new { message = $"Successfully imported {importCount} transactions." });
        }
    }
}
