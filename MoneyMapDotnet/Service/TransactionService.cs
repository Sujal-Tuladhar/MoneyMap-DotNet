using Microsoft.Extensions.Logging;
using MoneyMapDotnet.Data;
using MoneyMapDotnet.Models;

namespace MoneyMapDotnet.Service
{
    public class TransactionService
    {
        private readonly ILogger<TransactionService> _logger;
        private readonly string transactionPath = Utils.GetTransactionPath();

        private class MultiTransaction
        {
            public Dictionary<string, List<Transaction>> ClientTransactions { get; set; } = new();
        }

        public TransactionService(ILogger<TransactionService> logger)
        {
            _logger = logger;
        }

        private async Task<MultiTransaction> InitializeTransactionAsync()
        {
            try
            {
                var fileExist = await Utils.LoadJson<MultiTransaction>(transactionPath);
                if (fileExist == null)
                {
                    var newFile = new MultiTransaction();
                    await Utils.SaveJson(newFile, transactionPath);
                    return newFile;
                }
                return fileExist;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing transaction data.");
                throw;
            }
        }

        public async Task<List<Transaction>> GetTransactionDetail(string ClientID)
        {
            try
            {
                var initializedTransaction = await InitializeTransactionAsync();
                if (initializedTransaction.ClientTransactions.TryGetValue(ClientID, out var transactions))
                {
                    return transactions;
                }
                return new List<Transaction>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction details for client {ClientID}.", ClientID);
                throw;
            }
        }
       
        private async Task SaveTransactionDetail(IEnumerable<Transaction> transactions, string ClientID)
        {
            try
            {
                var initializedTransaction = await InitializeTransactionAsync();
                initializedTransaction.ClientTransactions[ClientID] = transactions.ToList();
                await Utils.SaveJson(initializedTransaction, transactionPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving transaction details for client {ClientID}.", ClientID);
            }
        }

        public async Task AddTransactionData(Transaction transaction, string ClientID)
        {
            try
            {
                var allTransaction = await GetTransactionDetail(ClientID);
                if (!allTransaction.Exists(t => t.Description == transaction.Description && t.Date == transaction.Date))
                {
                    allTransaction.Add(transaction);
                    await SaveTransactionDetail(allTransaction, ClientID);
                    _logger.LogInformation("Transaction added for client {ClientID}: {TransactionDescription}.", ClientID, transaction.Description);
                }
                else
                {
                    _logger.LogWarning("Transaction with description {Description} and date {Date} already exists for client {ClientID}.", transaction.Description, transaction.Date, ClientID);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed adding transaction for client {ClientID}.", ClientID);
            }
        }

        public async Task UpdateTransactionData(Transaction updatedTransaction, string ClientID)
        {
            try
            {
                var allTransaction = await GetTransactionDetail(ClientID);
                var index = allTransaction.FindIndex(t => t.Description == updatedTransaction.Description && t.Date == updatedTransaction.Date);

                if (index >= 0)
                {
                    allTransaction[index] = updatedTransaction;
                    await SaveTransactionDetail(allTransaction, ClientID);
                    _logger.LogInformation("Transaction updated for client {ClientID}: {TransactionDescription}.", ClientID, updatedTransaction.Description);
                }
                else
                {
                    _logger.LogWarning("Transaction not found for client {ClientID}: {TransactionDescription}.", ClientID, updatedTransaction.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed updating transaction for client {ClientID}.", ClientID);
            }
        }

        public async Task DeleteTransactionData(string description, DateTime? date, string ClientID)
        {
            try
            {
                var allTransaction = await GetTransactionDetail(ClientID);
                var transactionToRemove = allTransaction.FirstOrDefault(t => t.Description == description && t.Date == date);

                if (transactionToRemove != null)
                {
                    allTransaction.Remove(transactionToRemove);
                    await SaveTransactionDetail(allTransaction, ClientID);
                    _logger.LogInformation("Transaction deleted for client {ClientID}: {TransactionDescription}.", ClientID, description);
                }
                else
                {
                    _logger.LogWarning("Transaction not found for client {ClientID}: {TransactionDescription}.", ClientID, description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed deleting transaction for client {ClientID}.", ClientID);
            }
        }
        public async Task<List<Transaction>> GetTransactionsByDateRange(string clientId, DateTime startDate, DateTime endDate)
        {
            var allTransactions = await GetTransactionDetail(clientId);
            var filteredTransactions = allTransactions
                .Where(t => t.Date.HasValue && t.Date.Value.Date >= startDate.Date && t.Date.Value.Date <= endDate.Date)
                .ToList();
            return filteredTransactions;
        }




    }
}
