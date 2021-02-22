using System;
using System.IO;
using KustarovBotTelegramUI.Menus;
using YamlDotNet.Serialization;

namespace KustarovBotTelegramUI
{
    public static class Resources
    {
        private static readonly string ExecutionPath = AppDomain.CurrentDomain.BaseDirectory;

        public static Menu GetMenu(string menuId)
        {
            var path = Path.Combine(ExecutionPath, "Menus", "Data", $"{menuId}.yml");
            var rawYaml = File.ReadAllText(path);
            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<Menu>(rawYaml);
        }
    }
}