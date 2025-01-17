using Microsoft.Extensions.Logging;
using MoneyMapDotnet.Models;


namespace MoneyMapDotnet.Service
{
    public class DashboardService
    {
        private readonly ILogger<DashboardService> _logger;
        private readonly TransactionService _transactionService;
        private readonly DebtService _debtService;

        public DashboardService(ILogger<DashboardService> logger, TransactionService transactionService, DebtService debtService)
        {
            _logger = logger;
            _transactionService = transactionService;
            _debtService = debtService;
        }

        // Get Total Credit for a client within a date range
        public async Task<decimal> GetTotalCreditAsync(Guid clientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = await _transactionService.GetTransactionDetail(clientId);
            return transactions
                .Where(t => t.Type == "Credit" && (!startDate.HasValue || t.Date >= startDate) && (!endDate.HasValue || t.Date <= endDate))
                .Sum(t => t.Budget);
        }

        // Get Total Debit for a client within a date range
        public async Task<decimal> GetTotalDebitAsync(Guid clientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = await _transactionService.GetTransactionDetail(clientId);
            return transactions
                .Where(t => t.Type == "Debit" && (!startDate.HasValue || t.Date >= startDate) && (!endDate.HasValue || t.Date <= endDate))
                .Sum(t => t.Budget);
        }

        // Get Total Debt for a client
        public async Task<decimal> GetTotalDebtAsync(Guid clientId)
        {
            var debts = await _debtService.GetDebtsByClientAsync(clientId);
            return debts
                .Where(d => !d.IsCleared)
                .Sum(d => d.AmountOwed);
        }

        // Get Remaining Debt for a client
        public async Task<decimal> GetRemainingDebtAsync(Guid clientId)
        {
            var debts = await _debtService.GetDebtsByClientAsync(clientId);
            return debts
                .Where(d => !d.IsCleared)
                .Sum(d => d.AmountOwed);
        }

        // Get Current Balance for a client within a date range
        public async Task<decimal> GetCurrentBalanceAsync(Guid clientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var totalCredit = await GetTotalCreditAsync(clientId, startDate, endDate);
            var totalDebit = await GetTotalDebitAsync(clientId, startDate, endDate);
            return totalCredit - totalDebit;
        }

        // Get Credit vs Debit data for Donut Chart within a date range
        public async Task<(double[] Data, string[] Labels)> GetCreditVsDebitDataAsync(Guid clientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var totalCredit = await GetTotalCreditAsync(clientId, startDate, endDate);
            var totalDebit = await GetTotalDebitAsync(clientId, startDate, endDate);

            // Convert decimal to double for MudChart
            double[] data = { (double)totalCredit, (double)totalDebit };
            string[] labels = { "Credit", "Debit" };

            return (data, labels);
        }

        // Get Top 5 Highest Credits within a date range
        public async Task<List<Transaction>> GetTop5HighestCreditsAsync(Guid clientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = await _transactionService.GetTransactionDetail(clientId);
            return transactions
                .Where(t => t.Type == "Credit" && (!startDate.HasValue || t.Date >= startDate) && (!endDate.HasValue || t.Date <= endDate))
                .OrderByDescending(t => t.Budget)
                .Take(5)
                .ToList();
        }

        // Get Top 5 Lowest Credits within a date range
        public async Task<List<Transaction>> GetTop5LowestCreditsAsync(Guid clientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = await _transactionService.GetTransactionDetail(clientId);
            return transactions
                .Where(t => t.Type == "Credit" && (!startDate.HasValue || t.Date >= startDate) && (!endDate.HasValue || t.Date <= endDate))
                .OrderBy(t => t.Budget)
                .Take(5)
                .ToList();
        }

        // Get Top 5 Highest Debits within a date range
        public async Task<List<Transaction>> GetTop5HighestDebitsAsync(Guid clientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = await _transactionService.GetTransactionDetail(clientId);
            return transactions
                .Where(t => t.Type == "Debit" && (!startDate.HasValue || t.Date >= startDate) && (!endDate.HasValue || t.Date <= endDate))
                .OrderByDescending(t => t.Budget)
                .Take(5)
                .ToList();
        }

        // Get Top 5 Lowest Debits within a date range
        public async Task<List<Transaction>> GetTop5LowestDebitsAsync(Guid clientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = await _transactionService.GetTransactionDetail(clientId);
            return transactions
                .Where(t => t.Type == "Debit" && (!startDate.HasValue || t.Date >= startDate) && (!endDate.HasValue || t.Date <= endDate))
                .OrderBy(t => t.Budget)
                .Take(5)
                .ToList();
        }

        // Get Top 5 Debts
        public async Task<List<Debt>> GetTop5DebtsAsync(Guid clientId)
        {
            var debts = await _debtService.GetDebtsByClientAsync(clientId);
            return debts
                .OrderByDescending(d => d.AmountOwed)
                .Take(5)
                .ToList();
        }

        // Get Pending Debts
        public async Task<List<Debt>> GetPendingDebtsAsync(Guid clientId)
        {
            var debts = await _debtService.GetDebtsByClientAsync(clientId);
            return debts
                .Where(d => !d.IsCleared)
                .ToList();
        }

        // Get Total Amount (Credit - Debit - Remaining Debt) within a date range
        public async Task<decimal> GetTotalAmountAsync(Guid clientId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var totalCredit = await GetTotalCreditAsync(clientId, startDate, endDate);
            var totalDebit = await GetTotalDebitAsync(clientId, startDate, endDate);
            var remainingDebt = await GetRemainingDebtAsync(clientId);
            return totalCredit - totalDebit - remainingDebt;
        }
    }
}