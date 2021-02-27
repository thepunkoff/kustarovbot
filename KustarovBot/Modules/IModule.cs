using System.Threading.Tasks;
using VkNet.Model;

namespace KustarovBot.Modules
{
    public interface IModule
    {
        Task ProcessMessage(Message message, User user);
    }
}