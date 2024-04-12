using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Extensions.Logging;

namespace AeroexpressBotLibrary
{
    /// <summary>
    /// Статический класс, позволяющий обрабатывать обновления, получаемые ботом.
    /// </summary>
    public static class AeroBotHandle
    {
        /// <summary>
        /// Статические поля и флаги бота.
        /// </summary>
        private static bool isInitialMessageSent = false;
        private static bool isFirstFile = true;
        private static bool isFileReadSuccessfully = false;
        private static bool isFetching = false;
        private static string? FetchField;
        private static Message? secondFile;
        public static Microsoft.Extensions.Logging.ILogger BotLogger;
        private static List<Aeroexpress> data = null;

        /// <summary>
        /// Метод, для обработки обновлений, принимаемых ботом.
        /// </summary>
        /// <param name="botClient">Клиент бота.</param>
        /// <param name="update">Обновление.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Ничего.</returns>
        public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            long chatId;
            
            BotLogger.LogInformation($"Бот обработал обновление: {update.Type}, ID сообщения: {update.Message?.MessageId}, Текст сообщения: {update.Message?.Text}, Имя пользователя: {update.Message?.From.FirstName} {update.Message?.From.LastName}, ID чата: {update.Message?.Chat.Id}");
            
            var keyboardRemove = new ReplyKeyboardRemove();
            var keyboard = new ReplyKeyboardMarkup(new[] // Основные кнопки для работы, появляются после получения корректного файла.
            {
            new []
            {
                new KeyboardButton("Сортировка по полю."),
                new KeyboardButton("Выборка по полю."),
                new KeyboardButton("Скачать CSV-файл."),
                new KeyboardButton("Скачать JSON-файл."),
            }

            })
            { ResizeKeyboard = true };


            if (message != null)
            {

                if (data == null && message.Type != MessageType.Document) { await botClient.SendTextMessageAsync(message.Chat.Id, "Для начала работы загрузите файл.", replyMarkup: keyboardRemove); }
                chatId = message.Chat.Id;
            }
            else if (update.CallbackQuery != null)
            {
                chatId = update.CallbackQuery.Message.Chat.Id;
            }
            else
            {
                return;
            }


            if (message != null && message.Type == MessageType.Document && isFetching == false) // Если отправлен документ, запускаем ряд действий.
            {
                try
                {
                    if (data == null && isFirstFile)
                    {
                        await GetFile(botClient, message);
                        
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Данные успешно считаны!");
                        isFirstFile = false;
                        isFileReadSuccessfully = true;
                    }
                    else if (!isFirstFile)
                    {
                        secondFile = update.Message;
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                    new [] 
                    {
                        InlineKeyboardButton.WithCallbackData("Да", "yes"),
                        InlineKeyboardButton.WithCallbackData("Нет", "no")
                    }
                    });

                        await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Хотите продолжить работу с другим файлом?",
                        replyMarkup: inlineKeyboard
                    );

                    }
                    if (!isInitialMessageSent && isFileReadSuccessfully)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите действие, которое хотите произвести с загруженными данными.", replyMarkup: keyboard);
                        isInitialMessageSent = true;
                    }
                }
                catch (ArgumentException)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Структура файла не удовлетворяет варианту!");
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Повторите отправку.");
                    isFileReadSuccessfully = false;
                }
                catch (Exception)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Не удалось прочитать файл!");
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Повторите отправку.");
                    isFileReadSuccessfully = false;
                }
            }


            if (message != null && isFetching == true) // Действия в режиме приёма строки для выборки.
            {
                if (message.Type != MessageType.Text) { await botClient.SendTextMessageAsync(message.Chat.Id, "Отправлена не строка. Повторите попытку."); return; }
                string fetchString = message.Text;
                switch (FetchField)
                {
                    case "1":
                        List<Aeroexpress> fetchedData = data.Where(a => a.StationStart.Contains(fetchString)).ToList();
                        if (fetchedData.Count > 0)
                        {
                            data = fetchedData;
                            fetchedData = new List<Aeroexpress>();
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Выборка прошла успешно!");
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Данные не найдены.");
                        }
                        break;
                    case "2":
                        List<Aeroexpress> fetchedDataZwei = data.Where(a => a.StationEnd.Contains(fetchString)).ToList();
                        if (fetchedDataZwei.Count > 0)
                        {
                            data = fetchedDataZwei;
                            fetchedData = new List<Aeroexpress>();
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Выборка прошла успешно!");
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Данные не найдены.");
                        }
                        break;
                    case "3":
                        try
                        {
                            string[] parts = fetchString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                            string stationFrom = parts[0].Trim();
                            string stationTo = parts[1].Trim();
                            List<Aeroexpress> selectedData = data.Where(a => a.StationStart == stationFrom && a.StationEnd == stationTo).ToList();
                            if (selectedData.Count > 0)
                            {
                                data = selectedData;
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Выборка прошла успешно!");
                                selectedData = new List<Aeroexpress>();
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Данные не найдены.");
                            }
                        }
                        catch { await botClient.SendTextMessageAsync(message.Chat.Id, "Введена строка некорректного формата."); }
                        break;

                }
                isFetching = false;
                FetchField = null;
            }


            if (message != null && isFetching == false) // Действия для приёма строки вне режима выборки.
            {
                switch (message.Text)
                {
                    case "Сортировка по полю.":
                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("TimeStart в порядке увеличения времени", "timestart"),
                    InlineKeyboardButton.WithCallbackData("TimeEnd в порядке увеличения времени", "timeend")
                }

                }
                     );

                        await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Выберите вариант сортировки:",
                        replyMarkup: inlineKeyboard
                    );
                        break;
                    case "Выборка по полю.":
                        var inlineKeyboardTwo = new InlineKeyboardMarkup(new[]
                    {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("StationStart", "stStart"),
                    InlineKeyboardButton.WithCallbackData("StationEnd", "stEnd"),
                    InlineKeyboardButton.WithCallbackData("StationStart и StationEnd", "stStEnd")
                }
                });

                        await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Выберите вариант выборки:",
                        replyMarkup: inlineKeyboardTwo
                    );
                        break;
                    case "Скачать CSV-файл.":

                        CSVProcessing write = new();
                        Stream stream = write.Write(data); // Создаем поток с помощью метода Write
                        await botClient.SendDocumentAsync
                            (
                            chatId: message.Chat.Id,
                            document: InputFile.FromStream(stream, fileName: "aeroexpress.csv"),
                            caption: "Обработанные данные."
                            );

                        break;
                    case "Скачать JSON-файл.":
                        JSONProcessing writeJSON = new();
                        Stream streamJSON = writeJSON.Write(data);
                        await botClient.SendDocumentAsync
                            (
                            chatId: message.Chat.Id,
                            document: InputFile.FromStream(streamJSON, fileName: "aeroexpress.json"),
                            caption: "Обработанные данные."
                            );
                        break;

                }
            }


            if (update.Type == UpdateType.CallbackQuery) // Действия для обработки inline кнопок.
            {
                var callbackQuery = update.CallbackQuery;
                var callbackData = callbackQuery.Data;

                switch (callbackData)
                {
                    case "timestart":
                        try
                        {
                            Sorting(botClient, callbackQuery);
                            await HideInlineButtonAsync(botClient, callbackQuery);
                            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Сортировка прошла успешно!");
                        }
                        catch (Exception e) { Console.WriteLine(e.Message); }
                        break;
                    case "timeend":
                        try
                        {
                            Sorting(botClient, callbackQuery);
                            await HideInlineButtonAsync(botClient, callbackQuery);
                            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Сортировка прошла успешно!");
                        }
                        catch (Exception e) { Console.WriteLine(e.Message); }
                        break;
                    case "stStart":
                        await Fetch(botClient, callbackQuery);
                        break;
                    case "stEnd":
                        await Fetch(botClient, callbackQuery);
                        break;
                    case "yes":
                        try
                        {
                            await HideInlineButtonAsync(botClient, callbackQuery);
                            await GetFile(botClient, secondFile);
                      
                            secondFile = null;
                            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Данные успешно считаны!");
                            isFileReadSuccessfully = true;
                        }
                        catch { isFileReadSuccessfully = false; }
                        break;
                    case "no":
                        await HideInlineButtonAsync(botClient, callbackQuery);
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Продолжаем работу с текущими данными.");
                        break;
                    case "stStEnd":
                        await Fetch(botClient, callbackQuery);
                        break;
                }
            }
        }


        /// <summary>
        /// Метод для получения файла телеграм ботом.
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static async Task GetFile(ITelegramBotClient botClient, Message message)
        {
            if (message.Document != null && message.Document.FileName.EndsWith(".csv") && message.Document.MimeType == "text/csv")
            {
                try
                {
                    var fileId = message.Document.FileId;
                    var file = await botClient.GetFileAsync(fileId);
                    var stream = new MemoryStream();
                    await botClient.DownloadFileAsync(file.FilePath, stream);
                    stream.Position = 0;
                    var read = new CSVProcessing();
                    data = read.Read(stream);
                }
                catch (Exception e)
                {
                    if (data != null)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Не удалось прочитать файл! Продолжаем работу с ранее загруженным файлом.");
                    }
                    throw new Exception();
                }
            }
            else if (message.Document != null && message.Document.FileName.EndsWith(".json") && message.Document.MimeType == "application/json")
            {
                try
                {
                    
                    var fileId = message.Document.FileId;
                    var file = await botClient.GetFileAsync(fileId);
                    var stream = new MemoryStream();
                    await botClient.DownloadFileAsync(file.FilePath, stream);
                    stream.Position = 0;
                    var read = new JSONProcessing();
                    data = read.Read(stream);
                }
                catch (Exception e) {
                    if (data != null)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Не удалось прочитать файл! Продолжаем работу с ранее загруженным файлом.");
                    }
                    Console.WriteLine("Мы тут"); throw new Exception(); }
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Выслан файл некорректного формата.");
                isFileReadSuccessfully = false;
                throw new Exception();
            }
        }


        /// <summary>
        /// Метод для сортировки 
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private static void Sorting(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            switch (callbackQuery.Data)
            {
                case "timestart":
                    data = data.OrderBy(x => x.TimeStart).ToList();
                    //isInitialMessageSent = false;
                    break;
                case "timeend":
                    data = data.OrderBy(x => x.TimeEnd).ToList();
                    //isInitialMessageSent = false;
                    break;
            }
        }


        /// <summary>
        /// Метод, помогающий сделать выборку по полю.
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private static async Task Fetch(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            switch (callbackQuery.Data)
            {
                case "stStart":
                    await HideInlineButtonAsync(botClient, callbackQuery);
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введите строку для выборки StationStart:");
                    isFetching = true;
                    FetchField = "1";
                    break;
                case "stEnd":
                    await HideInlineButtonAsync(botClient, callbackQuery);
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введите строку для выборки StationEnd:");
                    isFetching = true;
                    FetchField = "2";
                    break;
                case "stStEnd":
                    await HideInlineButtonAsync(botClient, callbackQuery);
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введите строку для выборки StationStart и StationEnd через точку с запятой, пример: Откуда;Куда:");
                    isFetching = true;
                    FetchField = "3";
                    break;
            }
        }
        /// <summary>
        /// Метод, скрывающий inline кнопку после нажатия на неё.
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="callbackQuery"></param>
        /// <returns></returns>
        private static async Task HideInlineButtonAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            await botClient.EditMessageReplyMarkupAsync(
                           chatId: callbackQuery.Message.Chat.Id,
                           messageId: callbackQuery.Message.MessageId);
        }
    }
}
