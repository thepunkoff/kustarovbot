using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace KustarovBot.Shared
{
    public static class Configuration
    {
        private static JsonDocument _json;
        public static async Task <string> GetValue(string key)
        {
            _json ??= await JsonDocument.ParseAsync(File.OpenRead("config.json"));
            return _json.RootElement.GetProperty(key).GetString();
        }
    }
}