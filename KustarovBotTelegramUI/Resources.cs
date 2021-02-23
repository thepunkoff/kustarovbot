using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using KustarovBotTelegramUI.Menus;
using YamlDotNet.Serialization;

namespace KustarovBotTelegramUI
{
    public static class Resources
    {
        private static readonly string ExecutionPath = AppDomain.CurrentDomain.BaseDirectory;

        public static Menu GetMenu(string menuId)
        {
            var path = Path.Combine(ExecutionPath, "Data", "Menus", $"{menuId}.yml");
            var rawYaml = File.ReadAllText(path);
            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<Menu>(rawYaml);
        }
        
        public static string GetMessageText(string messageId)
        {
            var path = Path.Combine(ExecutionPath, "Data", "strings.xml");
            using var fs = new FileStream(path, FileMode.Open);
            var xDoc = XDocument.Load(fs);
            var xMessage = xDoc.Root.Elements().Single(x => x.Name == messageId);
            return xMessage.Value;
        }
    }
}