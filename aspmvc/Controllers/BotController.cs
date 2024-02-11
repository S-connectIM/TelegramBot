using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace aspmvc.Controllers
{
    [ApiController]
    [Route("/")]
    public class BotController : ControllerBase
    {
        private TelegramBotClient bot = Bot.GetTelegramBot();
        private static Dictionary<long, UserState> userStates = new Dictionary<long, UserState>();

        [HttpPost]
        public async void Post(Update update)
        {
            UserState userState;
            long chatId = update.Message.Chat.Id;

            if(update.Message.Text == null)
                return;

            if(!userStates.TryGetValue(update.Message.Chat.Id, out userState))
            {
                userState = new UserState();
                userStates[update.Message.Chat.Id] = userState;
            }

            if(userState.DrawQuestion)
            {
                string drawPrompt = update.Message.Text;
                Console.WriteLine($"@{update.Message.Chat.Username}: " + drawPrompt);

                await bot.SendTextMessageAsync(chatId, $"Рисую: {drawPrompt}");
                Console.WriteLine($"Bot: " + $"Рисую: {drawPrompt}");

                userState.DrawQuestion = false;
                await Kandinsky.GetGenerateImage(drawPrompt); 

                using(var fileStream = new FileStream(Kandinsky.GetFilePath(), FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    await bot.SendPhotoAsync(
                        chatId: chatId,
                        photo: InputFile.FromStream(fileStream),
                        caption: drawPrompt
                    );
                }
            }

            else
            {
                if(update.Message.Text.StartsWith("/"))
                {
                    await ProcessCommand(update, update.Message.Text.Substring(1), userState);
                }
                else if(update.Message.Text == "Нарисуй")
                {
                    userState.DrawQuestion = true;
                    await bot.SendTextMessageAsync(chatId, "Что нарисовать?");
                }
                else
                {
                    Console.WriteLine($"@{update.Message.Chat.Username}: " + update.Message.Text);
                    string answer = await YandexGPT.SendCompletionRequest(update.Message.Text);

                    Console.WriteLine("Bot: " + answer);
                    await bot.SendTextMessageAsync(chatId, answer);
                }
            }
        }

        [HttpGet]
        public string Get()
        {
            //Здесь мы пишем, что будет видно если зайти на адрес,
            //указаную в ngrok и launchSettings
            return "Telegram bot was started";
        }

        private async Task ProcessCommand(Update update, string command, UserState userState)
        {
            switch(command)
            {
                case "reply":
                    var replyKeyboard = new ReplyKeyboardMarkup(new List<KeyboardButton[]>() {                                                                                
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Нарисуй"),
                        },})
                    {
                        ResizeKeyboard = true,
                    };
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "Готово", replyMarkup: replyKeyboard);
                    break;

                case "draw":
                    userState.DrawQuestion = true;
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "Что нарисовать?");
                    break;

                default:
                    break;
            }
        }
    }

    class UserState
    {
        public bool DrawQuestion { get; set; }
        // Другие свойства, если необходимо
    }

}