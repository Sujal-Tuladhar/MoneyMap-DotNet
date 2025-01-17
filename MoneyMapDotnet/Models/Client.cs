namespace MoneyMapDotnet.Models;

public class Client
{
    public Guid ClientId { get; set; } // Unique identifier for the client
    public string Username { get; set; }
    public string Password { get; set; }
    public string Currency { get; set; }

    // Constructor to initialize the ClientId along with other properties
    public Client(string username, string password, string currency)
    {
        ClientId = Guid.NewGuid(); // Automatically generate a new GUID for ClientId
        Username = username;
        Password = password;
        Currency = currency;
    }

    // Method to validate the password
    public bool ValidatePassword(string inputPassword)
    {
        return Password.Equals(inputPassword);
    }
}