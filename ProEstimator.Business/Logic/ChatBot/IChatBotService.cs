using System.Threading.Tasks;

namespace ProEstimator.Business.Logic.ChatBot
{
    public interface IChatBotService
    {
        string GetAnswerAsync(string question);
    }
}