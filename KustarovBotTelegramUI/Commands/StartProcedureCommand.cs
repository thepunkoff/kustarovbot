using System;
using System.Threading.Tasks;
using NLog;

namespace KustarovBotTelegramUI.Commands
{
    public class StartProcedureCommand : ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string StartProcedure = "startprocedure";
        
        public string DebugName { get; } = nameof(StartProcedureCommand);
        private readonly ProcedureCode _procedure;
        
        public StartProcedureCommand(string pocedureName)
        {
            _procedure = Enum.Parse<ProcedureCode>(pocedureName);
        }
        
        public Task Run()
        {
            Logger.Trace($"[{StartProcedure}] starting {_procedure} procedure");

            TelegramKustarovBotUI.ActiveProcedure = _procedure;
            return Task.CompletedTask;
        }
    }
}