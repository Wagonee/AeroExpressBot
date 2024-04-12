using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using AeroexpressBotLibrary;
using HW_3_3_Ponomarev_1_var;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;

/*
 * 1 вариант.
 * Пономарев Николай Юрьевич, БПИ-234, ссылка на бота в телеграмм: @aeroexpress_var_1_bot
 */
public class Program
{
    public static Microsoft.Extensions.Logging.ILogger programLogger = Logging.Logger.CreateLogger("AeroexpressBot"); // Создаем поле с логгером, для записи логов.
    public static async Task Main() // Запуск бота.
    {
        AeroBotHandle.BotLogger = programLogger;
        var botClient = new TelegramBotClient("6645232947:AAFiqOFu68cZiNvs5oH_zLAAyqpAbIJ4HZs");
        using CancellationTokenSource cts = new CancellationTokenSource();
        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = Array.Empty<UpdateType>(),
            ThrowPendingUpdates = true
        };
        botClient.StartReceiving( 
        updateHandler: AeroBotHandle.UpdateHandler,
        pollingErrorHandler: HandlePollingErrorAsync,
        receiverOptions: receiverOptions,
         cancellationToken: cts.Token);
        var me = await botClient.GetMeAsync();
        programLogger.LogInformation($"Бот @{me.Username} запущен.");
        var keyboardRemove = new ReplyKeyboardRemove();
        Console.WriteLine("Бот успешно запущен!");
        Console.WriteLine("Для остановки бота нажмите любую клавишу.");
        Console.ReadKey();
        cts.Cancel();
        programLogger.LogInformation("Бот остановлен.");
    }
    private static Task HandlePollingErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };
        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}
