using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace aspmvc.Controllers
{
    [ApiController]
    [Route("/")]
    public class BotController : ControllerBase
    {
        private TelegramBotClient bot = GetTelegramBot();
        private static TelegramBotClient? client { get; set; }
        private static Dictionary<long, UserState> userStates = new Dictionary<long, UserState>();

        private static TelegramBotClient GetTelegramBot()
        {
            if(client != null)
            {
                return client;
            }
            client = new TelegramBotClient("6791814675:AAFA9__aGx879v1y-1FtnZNr1LHgRU1i7sc");

            return client;
        }

        public async void Index(Update update)
        {
            UserState userState;
            long chatId = update.Message.Chat.Id;
            bool isCan = true;

            try
            {
                if(update.Message.Chat.Type.ToString() == "Supergroup" || update.Message.Chat.Type.ToString() == "Group")
                {
                    chatId = update.Message.From.Id;
                    if(!isCan)
                    {
                        await client.SendTextMessageAsync(update.Message.Chat.Id, "У вас нет подписки, чтобы бот мог писать в групповой чат");
                        return;
                    }
                }

                else
                    chatId = update.Message.Chat.Id;

                var memberStatus = await client.GetChatMemberAsync("@WhizGPT", chatId);


                if(memberStatus.Status == ChatMemberStatus.Member || memberStatus.Status == ChatMemberStatus.Creator || memberStatus.Status == ChatMemberStatus.Administrator)
                {
                    if(!userStates.TryGetValue(update.Message.Chat.Id, out userState))
                    {
                        userState = new UserState();
                        userStates[update.Message.Chat.Id] = userState;
                    }

                    if(update.Message.Text == null)
                        return;

                    switch(userState.CurrentAction)
                    {
                        case UserAction.DrawQuestion:
                            await HandleDrawQuestionAsync(userState, update, update.Message.Chat.Id);
                            break;

                        case UserAction.Question:
                            await HandleQuestionAsync(userState, update, update.Message.Chat.Id);
                            break;

                        default:
                            await HandleDefaultActionAsync(userState, update, update.Message.Chat.Id);
                            break;
                    }
                }
                else
                {
                    await client.SendTextMessageAsync(update.Message.Chat.Id, "Вы не подписаны на канал @WhizGPT");
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
                await client.SendTextMessageAsync(update.Message.Chat.Id, $"Произошла ошибка {ex.Message}");
                await client.SendTextMessageAsync(chatId, update.Message.Chat.Type.ToString());
                await client.SendTextMessageAsync(chatId, chatId.ToString());
            }

        }

        private async Task HandleDrawQuestionAsync(UserState userState, Update update, long chatId)
        {
            string drawPrompt = update.Message.Text;

            await bot.SendTextMessageAsync(chatId, $"Рисую: {drawPrompt}");
            userState.CurrentAction = UserAction.None;

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

        private async Task HandleQuestionAsync(UserState userState, Update update, long chatId)
        {
            if(update.Message.Chat.Type == ChatType.Supergroup || update.Message.Chat.Type == ChatType.Group)
            {
                userState.CurrentAction = UserAction.None;

                Console.WriteLine($"@{update.Message.Chat.Username}: " + update.Message.Text);

                string answer = await YandexGPT.SendCompletionRequest(update.Message.Text);
                await bot.SendTextMessageAsync(chatId, answer);

                Console.WriteLine("Bot: " + answer);
            }
        }

        private async Task HandleDefaultActionAsync(UserState userState, Update update, long chatId)
        {
            if(update.Message.Text.StartsWith("/"))
            {
                await ProcessCommand(update, update.Message.Text.Substring(1), userState);
            }

            else if(update.Message.Chat.Type == ChatType.Private)
            {
                Console.WriteLine($"@{update.Message.Chat.Username}: " + update.Message.Text);

                string answer = await YandexGPT.SendCompletionRequest(update.Message.Text);
                await bot.SendTextMessageAsync(chatId, answer);

                Console.WriteLine("Bot: " + answer);
            }
        }

        private async Task ProcessCommand(Update update, string command, UserState userState)
        {
            switch(command)
            {
                case "draw":
                    userState.CurrentAction = UserAction.DrawQuestion;
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "Что нарисовать?");
                    //лог
                    break;

                case "draw@WhizGPT_bot":
                    userState.CurrentAction = UserAction.DrawQuestion;
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "Что нарисовать?");
                    //лог
                    break;

                case "question@WhizGPT_bot":
                    userState.CurrentAction = UserAction.Question;
                    await bot.SendTextMessageAsync(update.Message.Chat.Id, "Ваш запрос?");
                    //лог
                    break;

                default:
                    break;
            }
        }

        class UserState
        {
            public UserAction CurrentAction { get; set; }

            public string LastQuery { get; set; }

            public UserState()
            {
                CurrentAction = UserAction.None;
                LastQuery = string.Empty;
            }
        }

        enum UserAction
        {
            None,
            DrawQuestion,
            Question
        }
    }
}