using System.Collections.Generic;

namespace KustarovBotTelegramUI.Menus
{
    public class Menu
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public List<Row> Rows { get; set; }
    }
}