using System.Threading.Tasks;

namespace KustarovBotTelegramUI.Commands
{
    public interface ICommand
    {
        string DebugName { get; }
        Task Run();
    }
}