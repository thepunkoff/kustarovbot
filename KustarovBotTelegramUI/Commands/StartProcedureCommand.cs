using System;
using System.Threading.Tasks;

namespace KustarovBotTelegramUI.Commands
{
    public class StartProcedureCommand : ICommand
    {
        public string DebugName { get; } = nameof(StartProcedureCommand);
        private readonly ProcedureCode _procedure;
        
        public StartProcedureCommand(string pocedureName)
        {
            _procedure = Enum.Parse<ProcedureCode>(pocedureName);
        }
        
        public Task Run()
        {
            Console.WriteLine($"running '{DebugName}' command");
            TelegramKustarovBotUI.ActiveProcedure = _procedure;
            return Task.CompletedTask;
        }
    }
}