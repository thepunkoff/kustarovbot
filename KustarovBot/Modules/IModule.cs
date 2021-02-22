using System.Threading.Tasks;
using VkNet.Model;

namespace KustarovBot.MessageProcessing
{
    public interface IModule
    {
        Task ProcessMessage(Message message, User user);
    }
}