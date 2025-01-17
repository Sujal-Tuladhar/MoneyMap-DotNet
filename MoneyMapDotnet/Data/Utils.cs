using System.Text.Json;

namespace MoneyMapDotnet.Data
{
    public class Utils
    {
        private const string MoneyMap_Folder = "MoneyMapDotnetData";

        // Get the base directory for storing data
        public static string GetDirectory()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Create the directory path
            string directoryPath = Path.Combine(desktopPath, MoneyMap_Folder);

            // Ensure the directory exists (create if not)
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }

        // Get the path for storing client.json
        public static string GetClientsPath() => Path.Combine(GetDirectory(), "client.json");

        // Get the path for storing transaction.json
        public static string GetTransactionPath() => Path.Combine(GetDirectory(), "transaction.json");

        // Get the path for storing debt.json
        public static string GetDebtPath() => Path.Combine(GetDirectory(), "debt.json");

        // Save any data to a JSON file
        public static async Task SaveJson<T>(T data, string path)
        {
            try
            {
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to file: {ex.Message}");
            }
        }

        // Load data from a JSON file
        public static async Task<T> LoadJson<T>(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string json = await File.ReadAllTextAsync(path);
                    return JsonSerializer.Deserialize<T>(json);
                }
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading from file: {ex.Message}");
                return default;
            }
        }
    }
}