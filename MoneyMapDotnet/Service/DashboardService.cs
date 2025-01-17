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

        // Get Total Credit for a client
        public async Task<decimal> GetTotalCreditAsync(Guid clientId)
        {
            var transactions = await _transactionService.GetTransactionDetail(clientId);
            return transactions
                .Where(t => t.Type == "Credit")
                .Sum(t => t.Budget);
        }

        // Get Total Debit for a client
        public async Task<decimal> GetTotalDebitAsync(Guid clientId)
        {
            var transactions = await _transactionService.GetTransactionDetail(clientId);
            return transactions
                .Where(t => t.Type == "Debit")
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

        // Get Current Balance for a client
        public async Task<decimal> GetCurrentBalanceAsync(Guid clientId)
        {
            var totalCredit = await GetTotalCreditAsync(clientId);
            var totalDebit = await GetTotalDebitAsync(clientId);
            return totalCredit - totalDebit;
        }

        // Get Credit vs Debit data for Donut Chart
        public async Task<(double[] Data, string[] Labels)> GetCreditVsDebitDataAsync(Guid clientId)
        {
            var totalCredit = await GetTotalCreditAsync(clientId);
            var totalDebit = await GetTotalDebitAsync(clientId);

            // Convert decimal to double for MudChart
            double[] data = { (double)totalCredit, (double)totalDebit };
            string[] labels = { "Credit", "Debit" };

            return (data, labels);
        }

        // Get Top 5 Highest Credits
        public async Task<List<Transaction>> GetTop5HighestCreditsAsync(Guid clientId)
        {
            var transactions = await _transactionService.GetTransactionDetail(clientId);
            return transactions
                .Where(t => t.Type == "Credit")
                .OrderByDescending(t => t.Budget)
                .Take(5)
                .ToList();
        }

        // Get Top 5 Lowest Credits
        public async Task<List<Transaction>> GetTop5LowestCreditsAsync(Guid clientId)
        {
            var transactions = await _transactionService.GetTransactionDetail(clientId);
            return transactions
                .Where(t => t.Type == "Credit")
                .OrderBy(t => t.Budget)
                .Take(5)
                .ToList();
        }

        // Get Top 5 Highest Debits
        public async Task<List<Transaction>> GetTop5HighestDebitsAsync(Guid clientId)
        {
            var transactions = await _transactionService.GetTransactionDetail(clientId);
            return transactions
                .Where(t => t.Type == "Debit")
                .OrderByDescending(t => t.Budget)
                .Take(5)
                .ToList();
        }

        // Get Top 5 Lowest Debits
        public async Task<List<Transaction>> GetTop5LowestDebitsAsync(Guid clientId)
        {
            var transactions = await _transactionService.GetTransactionDetail(clientId);
            return transactions
                .Where(t => t.Type == "Debit")
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

        // Get Total Amount (Credit - Debit - Remaining Debt)
        public async Task<decimal> GetTotalAmountAsync(Guid clientId)
        {
            var totalCredit = await GetTotalCreditAsync(clientId);
            var totalDebit = await GetTotalDebitAsync(clientId);
            var remainingDebt = await GetRemainingDebtAsync(clientId);
            return totalCredit - totalDebit - remainingDebt;
        }
    }
}