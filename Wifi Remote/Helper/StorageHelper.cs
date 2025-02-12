using Newtonsoft.Json;

namespace Wifi_Remote.Helper
{
    internal static class StorageHelper
    {
        private static readonly string filePath = Path.Combine(FileSystem.AppDataDirectory, "appState.json");
        private static readonly object _lockObject = new object();

        private static IDictionary<string, string> ReadFileContents()
        {
            IDictionary<string, string> result = new Dictionary<string, string>();
            try
            {
                lock (_lockObject)
                {
                    if (File.Exists(filePath))
                    {
                        string fileContents = File.ReadAllText(filePath);
                        if (!string.IsNullOrEmpty(fileContents))
                        {
                            result = JsonConvert.DeserializeObject<IDictionary<string, string>>(fileContents)!;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // if failed to read file, means data is couurpted, so delete the file
                File.Delete(filePath);
            }

            return result;
        }

        private static void WriteFileContents(IDictionary<string, string> items)
        {
            lock (_lockObject)
            {
                string json = JsonConvert.SerializeObject(items, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
        }

        public static async Task<string?> GetAsync(string key)
        {
            IDictionary<string, string> items = ReadFileContents();
            if (items.TryGetValue(key, out string? value))
            {
                return value;
            }
            await Task.CompletedTask;
            return null;
        }

        public static async Task SetAsync(string key, string value)
        {
            IDictionary<string, string> items = ReadFileContents();
            items[key] = value;
            WriteFileContents(items);
            await Task.CompletedTask;
        }

        public static void Remove(string key)
        {
            IDictionary<string, string> items = ReadFileContents();
            items.Remove(key);
        }

        public static void Clear()
        {
            File.Delete(filePath);
        }
    }
}
