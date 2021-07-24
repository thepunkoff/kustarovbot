using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KustarovBot.Shared
{
    public static class Configuration
    {
        private static JsonDocument _json;
        public static async Task <string> GetValue(string key)
        {
            _json ??= await JsonDocument.ParseAsync(File.OpenRead(Path.Combine(Environment.CurrentDirectory, "Configuration", "config.json")));
            return _json.RootElement.GetProperty(key).GetString();
        }
        
        public static async Task<IEnumerable<string>> GetValues(string key)
        {
            _json ??= await JsonDocument.ParseAsync(File.OpenRead(Path.Combine(Environment.CurrentDirectory, "Configuration", "config.json")));
            return _json.RootElement.GetProperty(key).EnumerateArray()
                .Select(element => element.GetString());
        }
    }
}