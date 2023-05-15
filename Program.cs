using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bots.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bots.Http;
using RuTracker.Client;
using RuTracker.Client.Model.SearchTopics.Request;

namespace TelegramBotExperiments
{

    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("6000150225:AAEqq05KHlwwBCsIezaJoN1yI0xDHH99C3I");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/test")
                {
                    SendInline(botClient: botClient, chatId: message.Chat.Id, cancellationToken: cancellationToken);
                    return;
                }
                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать на борт, добрый путник!");
                    SendInline(botClient: botClient, chatId: message.Chat.Id, cancellationToken: cancellationToken);
                    return;
                }
                await botClient.SendTextMessageAsync(message.Chat, "Привет-привет!!");
            }
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                string codeOfButton = update.CallbackQuery.Data;
                if (codeOfButton == "post")
                {
                    Console.WriteLine("Нажата Кнопка 1");
                    string telegramMessage = "Вы нажали Кнопку 1";
                    await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id, telegramMessage, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }
                if (codeOfButton == "games")
                {
                    var client = new RuTrackerClient();
                    await client.Login("PestLord91", "scx91");
                    
                    var forums = await client.GetForums();
                    IReadOnlyList<int> answer = forums.Where(x => x.Path.Count == 3 && x.Path[2].Contains("Горячие Новинки", StringComparison.OrdinalIgnoreCase)).Select(x => x.Id).ToList();
                    var res = await client.SearchTopics(new SearchTopicsRequest(
                        Title: "",
                        Forums: answer,
                        SortBy: SearchTopicsSortBy.Downloads,
                        SortDirection: SearchTopicsSortDirection.Descending));


                    var textAnswer = "";
                    for (int i = 0; i < res.Topics.Count; i++) 
                    {
                        textAnswer += res.Topics[i].Title;
                        var torrentBytes = await client.GetTopicTorrentFile(res.Topics[i].Id);

                        var topic = await client.GetTopic(res.Topics[i].Id);
                        textAnswer += topic.MagnetLink;
                        textAnswer += "\n";
                        if (i >= 4) break;
                    }
                    await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat, textAnswer);

                }
                if (codeOfButton == "films")
                {

                }
                if (codeOfButton == "search")
                {
                    var client = new RuTrackerClient();
                    await client.Login("PestLord91", "scx91");
                    var res = await client.SearchTopics(new SearchTopicsRequest(
                        Title: "Виктор Цой FLAC",
                        SortBy: SearchTopicsSortBy.Downloads,
                        SortDirection: SearchTopicsSortDirection.Descending
                    ));
                    var topic = res.Topics.First();
                    var torrentFileBytes = await client.GetTopicTorrentFile(topic.Id);
                }
                if (codeOfButton == "12")
                {
                    Console.WriteLine("Нажата Кнопка 2");
                    string telegramMessage = "Вы нажали Кнопку 2";
                    // await botClient.SendTextMessageAsync(chatId: update.CallbackQuery.Message.Chat.Id, telegramMessage, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                    InlineKeyboardMarkup inlineKeyBoard = new InlineKeyboardMarkup(
                        new[]
                        {
                            // first row
                            new[]
                            {
                                // first button in row
                                InlineKeyboardButton.WithCallbackData(text: "Кнопка 3", callbackData: "post3"),
                                // second button in row
                                InlineKeyboardButton.WithCallbackData(text: "Кнопка 4", callbackData: "post4"),
                            },

                        });

                    // await botClient.EditMessageCaptionAsync(chatId: update.CallbackQuery.Message.Chat.Id, caption: telegramMessage, messageId: update.CallbackQuery.Message.MessageId);
                    await bot.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, telegramMessage, replyMarkup: inlineKeyBoard, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }
            }
        }

        //static void ParseTorrent(byte[] torrentBytes)
        //{
        //    var ms = new MemoryStream(torrentBytes);
        //    var torrentParser = new TorrentParser(TorrentParserMode.Strict);
        //    var torrent = torrentParser.Parse(ms);
        //    Console.WriteLine($"Here are '{torrent.DisplayName}' torrent files:");
        //    foreach (var file in torrent.Files)
        //    {
        //        Console.WriteLine(file.FullPath);
        //    }
        //}

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }

        public static async void SendInline(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
                // keyboard
                new[]
                {
                    // first row
                    new[]
                    {
                        // first button in row
                        InlineKeyboardButton.WithCallbackData(text: "Горячие новинки игр", callbackData: "games"),
                        // second button in row
                        InlineKeyboardButton.WithCallbackData(text: "Новинки кино", callbackData: "films"),
                    },
                    // second row
                    new[]
                    {
                        // first button in row
                        InlineKeyboardButton.WithCallbackData(text: "Поиск", callbackData: "Search"),
                        InlineKeyboardButton.WithCallbackData("Отмена")
                    },

                });

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "за что мне это??",
                replyMarkup: inlineKeyboard,
                cancellationToken: cancellationToken);
        }
    }
}