
using System.Globalization;

using System.Text; // Add this line for StringBuilder
using MoneyMapDotnet.Models;

namespace MoneyMapDotnet.Service
{
    public class Export
    {
        private readonly TransactionService _transactionService;

        public Export(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

      
        public async Task ExportTransactionsToCsvAsync(Guid clientId, string filePath)
        {
            try
            {
                // Retrieve transactions for the specified user ID
                var transactions = await _transactionService.GetTransactionDetail(clientId);

                if (transactions == null || !transactions.Any())
                {
                    Console.WriteLine("No transactions found for the specified user.");
                    return;
                }

                // Create the CSV content
                var csvContent = new StringBuilder();
                csvContent.AppendLine("Description,Budget,Type,Remarks,Date,Tags");

                foreach (var transaction in transactions)
                {
                    var formattedDate = transaction.Date?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) ?? "N/A";
                    var tags = transaction.Tags != null ? string.Join(";", transaction.Tags) : "N/A";

                    csvContent.AppendLine($"\"{transaction.Description}\",{transaction.Budget},\"{transaction.Type}\",\"{transaction.Remarks}\",{formattedDate},\"{tags}\"");
                }

                // Write the CSV content to the file
                await File.WriteAllTextAsync(filePath, csvContent.ToString());

                Console.WriteLine($"Transactions exported successfully to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting transactions: {ex.Message}");
            }
        }
    }
}