using System;
using System.IO;
using System.Threading.Tasks;
using KustarovBot.State;
using YamlDotNet.Serialization;

namespace KustarovBot
{
    public static class Resources
    {
        private static readonly string ExecutionPath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string StatePath = Path.Combine(ExecutionPath, "Data", "State", $"state.yml");
        
        public static BotState LoadState()
        {
            var rawYaml = File.ReadAllText(StatePath);
            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<BotState>(rawYaml);
        }
        
        public static async Task SaveState(BotState state)
        {
            var serializer = new SerializerBuilder().Build();
            var rawYaml = serializer.Serialize(state);
            await File.WriteAllTextAsync(StatePath, rawYaml);
        }
    }
}