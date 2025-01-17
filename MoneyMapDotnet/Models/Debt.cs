namespace MoneyMapDotnet.Models;

public class Debt
{
    public string Source { get; set; } // The source of the debt (e.g., "Credit Card", "Loan")
    public decimal AmountOwed { get; set; } // Amount owed for the debt
    public DateTime DueDate { get; set; } // Due date of the debt
    public bool IsCleared { get; set; } // Indicates if the debt has been cleared
    public Guid ClientID { get; set; } // Reference to the client/user the debt belongs to
    public string ClearedByTransactionId { get; set; } // ID of the transaction that cleared the debt

    // Constructor to initialize a new debt
    public Debt(string source, decimal amountOwed, DateTime dueDate, Guid clientID)
    {
        Source = source;
        AmountOwed = amountOwed;
        DueDate = dueDate;
        IsCleared = false; // Default to false when created
        ClientID = clientID;
        ClearedByTransactionId = null; // Initially, no transaction has cleared the debt
    }

    // Clear the debt and associate it with a transaction ID
    public void ClearDebt(string transactionId = null)
    {
        IsCleared = true;
        ClearedByTransactionId = transactionId;
    }
}