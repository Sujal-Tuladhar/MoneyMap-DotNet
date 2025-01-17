using MoneyMapDotnet.Data;
using MoneyMapDotnet.Models;

namespace MoneyMapDotnet.Service
{
    public class ClientService
    {
        private string clientDataPath = Utils.GetClientsPath();

        // Register a new client with a generated GUID
        public async Task RegisterClient(Client newClient)
        {
            try
            {
                // Load existing clients from file (if any)
                var existingClients = await Utils.LoadJson<List<Client>>(clientDataPath) ?? new List<Client>();

                // Check if username already exists (case-insensitive)
                if (existingClients.Any(c => c.Username.Equals(newClient.Username, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new Exception("Username already taken");
                }

                // Generate a new GUID for the client
                newClient.ClientId = Guid.NewGuid();

                // Add the new client and save back to file
                existingClients.Add(newClient);
                await Utils.SaveJson(existingClients, clientDataPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Registering. {ex.Message}");
                throw;
            }
        }

        // Retrieve a client by username
        public async Task<Client> GetUserAsync(string username)
        {
            try
            {
                // Load existing clients from file (if any)
                var existingClients = await Utils.LoadJson<List<Client>>(clientDataPath) ?? new List<Client>();

                // Find client by username (case-insensitive)
                return existingClients.FirstOrDefault(c => c.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user: {ex.Message}");
                return null;
            }
        }

        // Validate a client's password
        public bool ValidateClient(Client client, string inputPassword)
        {
            if (client == null || string.IsNullOrEmpty(inputPassword))
            {
                return false;
            }

            return client.ValidatePassword(inputPassword);
        }
    }
}