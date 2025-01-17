using Microsoft.Extensions.Logging;
using MoneyMapDotnet.Data;
using MoneyMapDotnet.Models;

namespace MoneyMapDotnet.Service
{
    public class DebtService
    {
        private readonly ILogger<DebtService> _logger;
        private readonly TransactionService _transactionService;
        private readonly string debtPath = Utils.GetDebtPath();

        public class MultiDebt
        {
            public Dictionary<Guid, List<Debt>> ClientDebts { get; set; } = new();
        }

        public DebtService(ILogger<DebtService> logger, TransactionService transactionService)
        {
            _logger = logger;
            _transactionService = transactionService;
        }

        // Initialize debt data (load from file or create new)
        public async Task<MultiDebt> InitializeDebtAsync()
        {
            try
            {
                var fileExist = await Utils.LoadJson<MultiDebt>(debtPath);
                if (fileExist == null)
                {
                    var newFile = new MultiDebt();
                    await Utils.SaveJson(newFile, debtPath);
                    return newFile;
                }
                return fileExist;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing debt data.");
                throw;
            }
        }

        // Get all debts for a specific client
        public async Task<List<Debt>> GetDebtsByClientAsync(Guid clientId)
        {
            try
            {
                var initializedDebt = await InitializeDebtAsync();
                if (initializedDebt.ClientDebts.TryGetValue(clientId, out var debts))
                {
                    return debts;
                }
                return new List<Debt>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving debts for client {ClientId}.", clientId);
                throw;
            }
        }

        // Save debts for a specific client
        public async Task SaveDebtsAsync(List<Debt> debts, Guid clientId)
        {
            try
            {
                var initializedDebt = await InitializeDebtAsync();
                initializedDebt.ClientDebts[clientId] = debts;
                await Utils.SaveJson(initializedDebt, debtPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving debts for client {ClientId}.", clientId);
                throw;
            }
        }

        // Add a new debt
        public async Task AddDebtAsync(Debt debt)
        {
            try
            {
                if (debt == null)
                    throw new ArgumentNullException(nameof(debt));

                var allDebts = await GetDebtsByClientAsync(debt.ClientID);
                allDebts.Add(debt);
                await SaveDebtsAsync(allDebts, debt.ClientID);
                _logger.LogInformation("Debt added for client {ClientId}: {Source}.", debt.ClientID, debt.Source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed adding debt for client {ClientId}.", debt.ClientID);
                throw;
            }
        }

        // Clear a debt (if the user has sufficient balance)
        public async Task ClearDebtAsync(string source, Guid clientId)
        {
            try
            {
                var allDebts = await GetDebtsByClientAsync(clientId);
                var debtToClear = allDebts.FirstOrDefault(d => d.Source == source && d.ClientID == clientId);

                if (debtToClear == null)
                {
                    _logger.LogWarning("Debt not found for client {ClientId}: {Source}.", clientId, source);
                    return;
                }

                if (debtToClear.IsCleared)
                {
                    _logger.LogWarning("Debt already cleared for client {ClientId}: {Source}.", clientId, source);
                    return;
                }

                decimal availableBalance = await _transactionService.GetAvailableBalance(clientId);
                if (availableBalance < debtToClear.AmountOwed)
                {
                    _logger.LogWarning("Insufficient balance to clear debt for client {ClientId}. Available balance: {AvailableBalance}, Debt amount: {DebtAmount}.", clientId, availableBalance, debtToClear.AmountOwed);
                    throw new Exception("Insufficient balance to clear the debt.");
                }

                var debitTransaction = new Transaction
                {
                    Description = $"Debt Clearance: {debtToClear.Source}",
                    Budget = debtToClear.AmountOwed,
                    Type = "Debit",
                    Date = DateTime.Now,
                    ClientID = clientId,
                    Tags = new List<string> { "Debt Clearance" }
                };

                await _transactionService.AddTransactionData(debitTransaction, clientId);

                debtToClear.ClearDebt();
                await SaveDebtsAsync(allDebts, clientId);

                _logger.LogInformation("Debt cleared for client {ClientId}: {Source}.", clientId, source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed clearing debt for client {ClientId}.", clientId);
                throw;
            }
        }

        // Delete a debt
        public async Task DeleteDebtAsync(string source, Guid clientId)
        {
            try
            {
                var allDebts = await GetDebtsByClientAsync(clientId);
                var debtToRemove = allDebts.FirstOrDefault(d => d.Source == source && d.ClientID == clientId);

                if (debtToRemove != null)
                {
                    allDebts.Remove(debtToRemove);
                    await SaveDebtsAsync(allDebts, clientId);
                    _logger.LogInformation("Debt deleted for client {ClientId}: {Source}.", clientId, source);
                }
                else
                {
                    _logger.LogWarning("Debt not found for client {ClientId}: {Source}.", clientId, source);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed deleting debt for client {ClientId}.", clientId);
                throw;
            }
        }
    }
}