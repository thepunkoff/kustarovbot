using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using KustarovBotTelegramUI.State;
using NLog;

namespace KustarovBotTelegramUI.Menus
{
    public class Menu
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string MenuLabel = "menu";

        public string Id { get; set; }
        public string Text { get; set; }
        public List<Row> Rows { get; set; }

        // ToDo: Убрать отсюда
        public static async Task<Menu> CreateActualScheduleMenu()
        {
            var ub = new UriBuilder(TelegramKustarovBotUI.Target);
            ub.Path += "iambusy/getSchedule";

            Logger.Trace($"[{MenuLabel}] sending request to {ub}");
            var request = WebRequest.CreateHttp(ub.ToString());
            request.Method = "GET";

            Schedule schedule = null;
            try
            {
                var webResponse = await request.GetResponseAsync();
                var httpResponse = (HttpWebResponse) webResponse;
                Logger.Trace($"[{MenuLabel}] bot returned status code '{httpResponse.StatusCode}'");
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    await using var responseStream = httpResponse.GetResponseStream();
                    using var sr = new StreamReader(responseStream);
                    var rawJson = await sr.ReadToEndAsync();
                    schedule = JsonSerializer.Deserialize<Schedule>(rawJson);
                }
            }
            catch (WebException wex)
            {
                var errorResponse = (HttpWebResponse) wex.Response;
                Logger.Trace($"[{MenuLabel}] bot returned status code '{errorResponse.StatusCode}'");
            }

            return new Menu
            {
                Text = "Пометьте галочками (✔) дни, в которые хотите, чтобы бот отвечал за вас.",
                Rows = new List<Row>
                {
                    new Row()
                    {
                        Buttons = new List<Button>
                        {
                            new Button()
                            {
                                Text = "Пн" + $" ({Emoji(schedule.Monday)})",
                                Commands = new List<string>{$"/changeDay monday {!schedule.Monday}"}
                            },
                            new Button()
                            {
                                Text = "Вт" + $" ({Emoji(schedule.Tuesday)})",
                                Commands = new List<string>{$"/changeDay tuesday {!schedule.Tuesday}"}
                            },
                            new Button()
                            {
                                Text = "Ср" + $" ({Emoji(schedule.Wednesday)})",
                                Commands = new List<string>{$"/changeDay wednesday {!schedule.Wednesday}"}
                            },
                        }
                    },
                    new Row()
                    {
                        Buttons = new List<Button>
                        {
                            new Button()
                            {
                                Text = "Чт" + $" ({Emoji(schedule.Thursday)})",
                                Commands = new List<string>{$"/changeDay thursday {!schedule.Thursday}"}
                            },
                            new Button()
                            {
                                Text = "Пт" + $" ({Emoji(schedule.Friday)})",
                                Commands = new List<string>{$"/changeDay friday {!schedule.Friday}"}
                            },
                            new Button()
                            {
                                Text = "Сб" + $" ({Emoji(schedule.Saturday)})",
                                Commands = new List<string>{$"/changeDay saturday {!schedule.Saturday}"}
                            },
                        },
                        
                    },
                    new Row()
                    {
                        Buttons = new List<Button>
                        {
                            new Button()
                            {
                                Text = "Вс" + $" ({Emoji(schedule.Sunday)})",
                                Commands = new List<string>{$"/changeDay sunday {!schedule.Monday}"}
                            },
                        },
                    },
                    new Row()
                    {
                        Buttons = new List<Button>
                        {
                            new Button()
                            {
                                Text = "<- Назад",
                                Commands = new List<string>{"/iambusy"}
                            },
                        },
                    }
                },
            };

            static string Emoji(bool value) => value ? "✔" : "❌";
        }
    }
}
