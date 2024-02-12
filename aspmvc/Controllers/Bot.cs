using Telegram.Bot;

namespace aspmvc.Controllers
{
    public class BotController : Controller
    {
        private static TelegramBotClient? client { get; set; }

        public static TelegramBotClient GetTelegramBot()
        {
            if(client != null)
            {
                return client;
            }
            client = new TelegramBotClient("6791814675:AAFA9__aGx879v1y-1FtnZNr1LHgRU1i7sc");
            return client;
        }
        public static Index(){
            return "Тёма даун";
        }
    }
}
