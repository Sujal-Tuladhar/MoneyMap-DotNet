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
            public Dictionary<Guid, List<Transaction>> ClientTransactions { get; set; } = new();
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

        public async Task<List<Transaction>> GetTransactionDetail(Guid clientId)
        {
            try
            {
                var initializedTransaction = await InitializeTransactionAsync();
                if (initializedTransaction.ClientTransactions.TryGetValue(clientId, out var transactions))
                {
                    return transactions;
                }
                return new List<Transaction>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction details for client {ClientId}.", clientId);
                throw;
            }
        }

        private async Task SaveTransactionDetail(IEnumerable<Transaction> transactions, Guid clientId)
        {
            try
            {
                var initializedTransaction = await InitializeTransactionAsync();
                initializedTransaction.ClientTransactions[clientId] = transactions.ToList();
                await Utils.SaveJson(initializedTransaction, transactionPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving transaction details for client {ClientId}.", clientId);
                throw;
            }
        }

        public async Task AddTransactionData(Transaction transaction, Guid clientId)
        {
            try
            {
                // Check if the transaction is a debit and if there is enough balance
                if (transaction.Type == "Debit")
                {
                    decimal availableBalance = await GetAvailableBalance(clientId);
                    if (availableBalance < transaction.Budget)
                    {
                        _logger.LogWarning("Insufficient balance for client {ClientId}. Available balance: {AvailableBalance}, Transaction amount: {TransactionAmount}.", clientId, availableBalance, transaction.Budget);
                        throw new Exception("Insufficient balance to perform the debit transaction.");
                    }
                }

                var allTransactions = await GetTransactionDetail(clientId);
                if (!allTransactions.Exists(t => t.Description == transaction.Description && t.Date == transaction.Date))
                {
                    allTransactions.Add(transaction);
                    await SaveTransactionDetail(allTransactions, clientId);
                    _logger.LogInformation("Transaction added for client {ClientId}: {TransactionDescription}.", clientId, transaction.Description);
                }
                else
                {
                    _logger.LogWarning("Transaction with description {Description} and date {Date} already exists for client {ClientId}.", transaction.Description, transaction.Date, clientId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed adding transaction for client {ClientId}.", clientId);
                throw;
            }
        }

        public async Task UpdateTransactionData(Transaction updatedTransaction, Guid clientId)
        {
            try
            {
                var allTransactions = await GetTransactionDetail(clientId);
                var index = allTransactions.FindIndex(t => t.Description == updatedTransaction.Description && t.Date == updatedTransaction.Date);

                if (index >= 0)
                {
                    allTransactions[index] = updatedTransaction;
                    await SaveTransactionDetail(allTransactions, clientId);
                    _logger.LogInformation("Transaction updated for client {ClientId}: {TransactionDescription}.", clientId, updatedTransaction.Description);
                }
                else
                {
                    _logger.LogWarning("Transaction not found for client {ClientId}: {TransactionDescription}.", clientId, updatedTransaction.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed updating transaction for client {ClientId}.", clientId);
                throw;
            }
        }

        public async Task DeleteTransactionData(string description, DateTime? date, Guid clientId)
        {
            try
            {
                var allTransactions = await GetTransactionDetail(clientId);
                var transactionToRemove = allTransactions.FirstOrDefault(t => t.Description == description && t.Date == date);

                if (transactionToRemove != null)
                {
                    allTransactions.Remove(transactionToRemove);
                    await SaveTransactionDetail(allTransactions, clientId);
                    _logger.LogInformation("Transaction deleted for client {ClientId}: {TransactionDescription}.", clientId, description);
                }
                else
                {
                    _logger.LogWarning("Transaction not found for client {ClientId}: {TransactionDescription}.", clientId, description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed deleting transaction for client {ClientId}.", clientId);
                throw;
            }
        }

        public async Task<List<Transaction>> GetTransactionsByDateRange(Guid clientId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var allTransactions = await GetTransactionDetail(clientId);
                var filteredTransactions = allTransactions
                    .Where(t => t.Date.HasValue && t.Date.Value.Date >= startDate.Date && t.Date.Value.Date <= endDate.Date)
                    .ToList();
                return filteredTransactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions by date range for client {ClientId}.", clientId);
                throw;
            }
        }

        public async Task<List<Transaction>> FilterTransactions(Guid clientId, string type = null, List<string> tags = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var allTransactions = await GetTransactionDetail(clientId);

                // Apply filters
                var filteredTransactions = allTransactions.AsQueryable();

                if (!string.IsNullOrEmpty(type))
                {
                    filteredTransactions = filteredTransactions.Where(t => t.Type == type).AsQueryable();
                }

                if (tags != null && tags.Any())
                {
                    filteredTransactions = filteredTransactions.Where(t => t.Tags.Any(tag => tags.Contains(tag))).AsQueryable();
                }

                if (startDate.HasValue)
                {
                    filteredTransactions = filteredTransactions.Where(t => t.Date.HasValue && t.Date.Value.Date >= startDate.Value.Date).AsQueryable();
                }

                if (endDate.HasValue)
                {
                    filteredTransactions = filteredTransactions.Where(t => t.Date.HasValue && t.Date.Value.Date <= endDate.Value.Date).AsQueryable();
                }

                return filteredTransactions.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering transactions for client {ClientId}.", clientId);
                throw;
            }
        }

        public async Task<List<Transaction>> SortTransactionsByDate(Guid clientId, bool ascending = true)
        {
            try
            {
                var allTransactions = await GetTransactionDetail(clientId);

                if (ascending)
                {
                    return allTransactions.OrderBy(t => t.Date).ToList();
                }
                else
                {
                    return allTransactions.OrderByDescending(t => t.Date).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sorting transactions by date for client {ClientId}.", clientId);
                throw;
            }
        }

        public async Task<decimal> GetAvailableBalance(Guid clientId)
        {
            try
            {
                var allTransactions = await GetTransactionDetail(clientId);

                // Calculate total credit and debit
                decimal totalCredit = allTransactions
                    .Where(t => t.Type == "Credit")
                    .Sum(t => t.Budget);

                decimal totalDebit = allTransactions
                    .Where(t => t.Type == "Debit")
                    .Sum(t => t.Budget);

                // Available balance = total credit - total debit
                decimal availableBalance = totalCredit - totalDebit;

                _logger.LogInformation("Available balance for client {ClientId}: {AvailableBalance}.", clientId, availableBalance);
                return availableBalance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating available balance for client {ClientId}.", clientId);
                throw;
            }
        }
    }
}