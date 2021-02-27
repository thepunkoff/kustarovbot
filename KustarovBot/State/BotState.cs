namespace KustarovBot.State
{
    public record BotState
    {
        public string BotIgnoreListName { get; init; } = "bot_iambusy_ignore";
        public string ReplyText { get; init; } = "Доброе время суток! Сегодня я не работаю, напишите мне в рабочий день. Спасибо за понимание!";
        public bool Monday { get; init; } = false;
        public bool Tuesday { get; init; } = false;
        public bool Wednesday { get; init; } = false;
        public bool Thursday { get; init; } = false;
        public bool Friday { get; init; } = false;
        public bool Saturday { get; init; } = true;
        public bool Sunday { get; init; } = true;
    }
}