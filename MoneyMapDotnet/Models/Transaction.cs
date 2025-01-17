namespace MoneyMapDotnet.Models
{
    public class Transaction
    {
        public string Description { get; set; }
        public decimal Budget { get; set; }
        public string Type { get; set; }
        public List<string> Tags { get; set; }
        public DateTime? Date { get; set; }
        public string Remarks { get; set; }

        public Guid ClientID { get; set; } // Updated to use Guid instead of string

        // Constructor to initialize all properties
        public Transaction(string description, decimal budget, string type, List<string> tags, DateTime? date, string remarks, Guid clientID)
        {
            Description = description;
            Budget = budget;
            Type = type;
            Tags = tags ?? new List<string>(); // Initialize Tags to avoid null reference
            Date = date;
            Remarks = remarks;
            ClientID = clientID;
        }

        // Optional: Add a parameterless constructor for flexibility (e.g., for deserialization)
        public Transaction()
        {
            Tags = new List<string>(); // Initialize Tags to avoid null reference
        }
    }
}