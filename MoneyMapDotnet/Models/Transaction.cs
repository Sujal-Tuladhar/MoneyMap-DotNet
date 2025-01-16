

namespace MoneyMapDotnet.Models
{
    public class Transaction
    {
        public string Description  { get; set; }
        public decimal Budget { get; set; }
        public string Type { get; set; }
        public List<string> Tags { get; set; }
        public DateTime? Date { get; set; }
        public string Remarks { get; set; }

        public string ClientID { get; set; }
        public Transaction(string description, decimal budget, string type, List<string> tags, DateTime? date,  string remarks, string clientID)
        {
            Description = description;
            Budget = budget;
            Type = type;
            Tags = tags;
            Date = date;
            Remarks = remarks;
            ClientID = clientID;
        }
    }
}
