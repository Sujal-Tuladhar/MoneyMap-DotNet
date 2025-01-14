namespace MoneyMapDotnet.Models;

public class Client
{
    public string Username { get; set; } 
    
    public string Password { get; set; }
    
    public string Currency { get; set; }

    public Client(string username, string password, string currency)
    {
        
    }
}