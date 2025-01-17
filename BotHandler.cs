﻿

using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using System.Reflection.Metadata;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Newtonsoft.Json.Serialization;
using TelegramBot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram_Bot
{
    public class BotHandler
    {
        public string botToken { get; set; }
        public int isCodeTrue = 0;

        public BotHandler(string token)
        {
            botToken = token;
        }
        public HashSet<long> Admins = new HashSet<long>() { };

        public async Task BotHandle()
        {
            var botClient = new TelegramBotClient(botToken);

            using CancellationTokenSource cts = new();

            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;

            long chatId = message.Chat.Id;

            CRUD.Create(new BotUser()
            {
                chatID = chatId,
                status = 0,
                phoneNumber = ""
            });
            if(isAdmin(chatId) == true)
            {
                Console.WriteLine("Adminku bu");
            }
            if (isCodeTrue == 1)
            {
                if (message.Text == "get_code")
                {
                    isCodeTrue = 2;
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Qabul qilindi!",
                        cancellationToken: cancellationToken);
                    return;
                }
                await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Code xato!",
                        cancellationToken: cancellationToken);
                return;

            }
            if (message.Contact != null)
            {
                CRUD.Update(chatId, message.Contact.PhoneNumber.ToString());
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Iltimos sizga yuborilgan code ni kiriting!",
                    replyMarkup: new ReplyKeyboardRemove(),
                    cancellationToken: cancellationToken);
                isCodeTrue = 1;
                return;
            }
            if (message.Text == "/start")
            {
                if (CRUD.IsPhoneNumberNull(chatId) == false)
                {

                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                        {
                    KeyboardButton.WithRequestContact("Contact")
                })
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(
                     chatId: chatId,
                     text: "Assalomu aleykum, Botimizga xush kelibsiz bu bot orqali siz kitob sotib olishingiz mumkkin\n" +
                     "Botdan to'liq foydalanish uchun telefon raqamingizni jo'nating",
                     replyMarkup: replyKeyboardMarkup,
                     cancellationToken: cancellationToken);
                    CRUD.ChangeStatusCode(chatId, 0);
                    return;
                }
                await botClient.SendTextMessageAsync(
                     chatId: chatId,
                     text: "Assalomu aleykum, Botimizga xush kelibsiz bu bot orqali siz nimadir qila olishingiz mumkin",
                     cancellationToken: cancellationToken);
                CRUD.ChangeStatusCode(chatId, 0);
                return;
            }


            if (CRUD.IsPhoneNumberNull(chatId) == false)
            {

                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                    KeyboardButton.WithRequestContact("Contact")
                })
                {
                    ResizeKeyboard = true
                };
            await botClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: "Botdan to'liq foydalanish uchun telefon raqamingizni jo'nating!",
                 replyMarkup: replyKeyboardMarkup,
                 cancellationToken: cancellationToken);
                CRUD.ChangeStatusCode(chatId, 0);
                return;
            }
            switch (CRUD.GetStatusCode(chatId))
            {
                case 0:
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Tugadi",
                        cancellationToken: cancellationToken);
                    break;
                default:
                    break;
            }



        }
        public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
        }
        public bool isAdmin(long id)
        {
            foreach (long a in Admins)
            {
                if (a == id) return true;
            }
            return false;
        }
    }
}
