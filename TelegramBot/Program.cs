using System;
using System.Diagnostics;
using System.Linq;
using Telegram;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace Telegram_Bot
{
    class Program
    {
        private static string _token { get; set; } = "6487643126:AAGMs9_bJqAG91QPwJbp-r7PG5qBsJSsk-I";
        private static string _paymentToken = "381764678:TEST:71834";
        private static TelegramBotClient _client;
        private static List<Admin> _admins = new();

        private static ITelegramBotClient _botClient;

        private static ReceiverOptions _receiverOptions;

        private static string _imagesPath = @"\Images\";

        #region Dictionary
        private static Dictionary<GameDateTime, string> _dateTimeNames = new Dictionary<GameDateTime, string>()
        {
            {GameDateTime.Unlimited, "Безлимитно"},
            {GameDateTime.OneMonth, "1 месяц"},
            {GameDateTime.ThreeMonth, "3 месяца"},
            {GameDateTime.SixMounth, "6 месяцев"},
            {GameDateTime.OneYear, "1 год"},
            {GameDateTime.ThreeYear, "3 года"},
        };

        private static Dictionary<GameDateTime, int> _dateTimeMonthsValues = new Dictionary<GameDateTime, int>()
        {
            {GameDateTime.Unlimited, 999999},
            {GameDateTime.OneMonth, 1},
            {GameDateTime.ThreeMonth, 3},
            {GameDateTime.SixMounth, 6},
            {GameDateTime.OneYear, 12},
            {GameDateTime.ThreeYear, 36},
        };

        private static Dictionary<string, UserData> _userData = new Dictionary<string, UserData>();
        #endregion

        #region Keyboard
        private static ReplyKeyboardMarkup _backKeyboardMarkup = new ReplyKeyboardMarkup
        (new List<KeyboardButton>()
            {
                new KeyboardButton("Меню")
            }
        );
        #endregion

        static async Task Main()
        {
            _botClient = new TelegramBotClient(_token);
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] 
                {
                    UpdateType.Message,
                    UpdateType.PreCheckoutQuery,
                    UpdateType.CallbackQuery

                },
                ThrowPendingUpdates = false,
            };

            using var cts = new CancellationTokenSource();

            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

            var me = await _botClient.GetMeAsync();
            Console.WriteLine($"{me.FirstName} запущен!");

            _backKeyboardMarkup.ResizeKeyboard = true;
            await Task.Delay(-1);
        }

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.PreCheckoutQuery:
                    {
                        CheckPayment(botClient, update, cancellationToken);
                        return;
                    }

                    case UpdateType.Message:
                    {
                        var message = update.Message;
                        var chat = update.Message.Chat;

                        if (!SQL.IsUserIDExist(chat.Id.ToString()))
                        {
                            SQL.RegistrateUserID(chat.Id.ToString());
                        }

                        if (!GetUserData(chat.Id)._isBlockMenu)
                        {
                            Menu(botClient, update, cancellationToken);
                            Contacts(botClient, update, cancellationToken);
                            Cotalog(botClient, update, cancellationToken);
                            NewGames(botClient, update, cancellationToken);
                            Privacy(botClient, update, cancellationToken);
                            Support(botClient, update, cancellationToken);
                            QA(botClient, update, cancellationToken);
                        }
                        
                        CheckGame(botClient, update, cancellationToken);
                        ShowPrivacy(botClient, update, cancellationToken);
                        Regestration(botClient, update, cancellationToken);
                        PaymentInstruction(botClient, update, cancellationToken);
                        SuccesfulPaymentCheck(botClient, update, cancellationToken);
                        License(botClient, update, cancellationToken);
                        SystemCommands(botClient, update, cancellationToken);


                        if (CheckAdmin(update))
                        {
                            StartAdminPanel(botClient, update, cancellationToken);
                            AdminRegistrate(botClient, update, cancellationToken);
                            AdminGenerateLicense(botClient, update, cancellationToken);
                            PushDistribution(botClient, update, cancellationToken);
                        }
                        return;
                    }
                
                    default:
                    {
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                            "Извините, не понимаю");
                            return;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static async void CheckPayment(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            PreCheckoutQuery preCheckout = update.PreCheckoutQuery;
            if (preCheckout != null)
            {
                await botClient.AnswerPreCheckoutQueryAsync(update.PreCheckoutQuery.Id, cancellationToken);
                Console.WriteLine(update.PreCheckoutQuery.From.Id);
                GetUserData(update.PreCheckoutQuery.From.Id)._isPreCheckout = true;
            }
        }

        private static async void License(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;
            if (GetUserData(chat.Id)._gameToBuy != null && message.Text == "Установил игру 🎉")
            {
                await botClient.SendTextMessageAsync(chat.Id,
                    "Отлично, перейдем к активации игры! 🎮\r\n\r\nИгру необходимо активировать всего один раз, при первом запуске.\r\n\r\n1. Запустите игру 🚀\r\n2. На экране вы увидите две строки 📊\r\n3. Верхняя строка \"персональный код\" содержит ваш уникальный код 🔑\r\n4. Скопируйте его 📋\r\n5. Отправьте его в чат 📤\r\n6. Как только вы его отправите, через несколько минут вам придет сообщение с ключом 🗝️\r\n7. Копируете ключ из сообщения 📋\r\n8. Открываете игру 🎮\r\n9. Вставляете ключ в поле \"лицензия\", который вы скопировали из чата 🎫\r\n10. Нажимаете на кнопку \"ИГРАТЬ\" ▶️\r\n11. Игра будет активирована, и вы можете наслаждаться приключениями! 🌟");
                GetUserData(chat.Id)._isGenerateLicense = true;
                GetUserData(chat.Id)._isInput = true;
            }

            if (GetUserData(chat.Id)._isGenerateLicense)
            {

                if (message.Text == "Да, это он ✅")
                {
                    string openKey;
                    string closeKey;

                    SQL.GetOpenCloseKeys(GetUserData(chat.Id)._gameToBuy.Name, out openKey, out closeKey);

                    SerialNumberGeneratorTools.OpenKey = openKey;
                    SerialNumberGeneratorTools.CloseKey = closeKey;


                    if (openKey != null && closeKey != null)
                    {
                        var licenseKey = SerialNumberGeneratorTools.MakeLicense(GetUserData(chat.Id)._hardwareID, GetDate(GetUserData(chat.Id)._gameToBuy.DatePrice.DateTime), "", "");

                        await botClient.SendTextMessageAsync(chat.Id, "Приятной вам игры! 🎮🌟 Вот ваш ключ🗝️: " + licenseKey,
                            replyMarkup: _backKeyboardMarkup);

                        SQL.RegistrateOrder(chat.Id.ToString(), GetUserData(chat.Id)._hardwareID, licenseKey, GetUserData(chat.Id)._gameToBuy.DatePrice.Amount, GetUserData(chat.Id)._gameToBuy.Name, _dateTimeMonthsValues[GetUserData(chat.Id)._gameToBuy.DatePrice.DateTime], DateTime.Now.ToString(), GetUserData(chat.Id)._transactionKey);
                        GetUserData(chat.Id)._isBlockMenu = false;
                    }

                    return;
                }

                if (message.Text == "Нет, они разные. 📤")
                {
                    await botClient.SendTextMessageAsync(chat.Id,
                        "Пожалуйста, отправте код повторно");
                    GetUserData(chat.Id)._hardwareID = null;
                    GetUserData(chat.Id)._isInput = true;
                    return;
                }

                var replyKeyboard = new ReplyKeyboardMarkup(
                    new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Да, это он ✅"),
                            new KeyboardButton("Нет, они разные. 📤"),
                        },
                    })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true,
                };

                if(message.Text == null) return;

                if (message?.Text.Length == 8 && GetUserData(chat.Id)._isInput)
                {
                    GetUserData(chat.Id)._hardwareID = message.Text;
                    GetUserData(chat.Id)._isInput = false;
                }

                if (GetUserData(chat.Id)._isInput || GetUserData(chat.Id)._hardwareID == null) return;
                await botClient.SendTextMessageAsync(chat.Id,
                    $"Пожалуйста, проверьте код: {GetUserData(chat.Id)._hardwareID}, который вы отправили, с кодом, отображаемым в поле 'персональный код'. Все верно? 🤔📋", replyMarkup: replyKeyboard);
                GetUserData(chat.Id)._isInput = true;
                return;
            }
        }

        #region LogisticMethods
        private static async void SuccesfulPaymentCheck(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (GetUserData(chat.Id)._isPreCheckout)
            {
                SuccessfulPayment successfulPayment = update.Message?.SuccessfulPayment;
                if (successfulPayment != null)
                {
                    var replyKeyboard = new ReplyKeyboardMarkup(
                        new List<KeyboardButton[]>()
                        {
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Установил игру 🎉"),
                            },
                        })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = true
                    };

                    GetUserData(chat.Id)._isPreCheckout = false;
                    GetUserData(chat.Id)._isBlockMenu = true;
                    GetUserData(chat.Id)._transactionKey = successfulPayment.ProviderPaymentChargeId;

                    await botClient.SendTextMessageAsync(chat.Id,
                        "Отлично! 🎉 Платеж успешно завершен. Сейчас я отправлю вам файл игры и подробную инструкцию по установке. 📤🎮 Наслаждайтесь вашей новой игрой! 😊🚀");
                    await botClient.SendTextMessageAsync(chat.Id, "1. В сообщение будет отправлена ссылка на Гугл Диск, где вы должны скачать файл установки\U0001f9f7\r\n3. Запустите после скачивания файл Setup.exe ⚙️\r\n4. Откроется \"мастер установщик\" \U0001f9d9\r\n5. Нажимаете кнопку \"далее\" ➡️\r\n6. Выбираете путь, куда будет установлена игра (если вас не устраивает стандартный путь) 🏠\r\n7. После выбора пути, нажимаете кнопку \"далее\" ➡️\r\n8. Выбираете путь, где программа создаст свой ярлык (если вас устраивает стандартный путь) 🖥\r\n9. Можете поставить галочку в пункте \"Не создавать папку в меню \"Пуск\"\" (по желанию) ✅\r\n10. Нажимаете кнопку \"далее\" ➡️\r\n11. РЕКОМЕНДУЕМ поставить галочку в пункте \"Создать значок на Рабочем столе\" (по желанию) 🌟\r\n12. Нажимаете кнопку \"установить\", если вас устраивает путь установки, или \"назад\", чтобы изменить параметры. 🛠\r\n13. Запустите процесс установки игры ⏩\r\n14. Можете снять галочку в пункте \"Запустить игру\" (по желанию) 🎮\r\n15. Нажимаете на кнопку \"Завершить\" 🎉\r\n\r\nКак только установите программу, дайте мне знать, чтобы мы активировали игру. 🚀");
                    await botClient.SendTextMessageAsync(chat.Id, SQL.GetSetupLink(GetUserData(chat.Id)._gameToBuy.Name), replyMarkup: replyKeyboard);

                }
            }
        }

        private static async void Menu(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (message.Text == "/start" || message.Text == "/menu" || message.Text == "Меню" || message.Text == "/testStart")
            {
                GetUserData(chat.Id).ResetDate();

                var replyKeyboard = new ReplyKeyboardMarkup(
                    new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Каталог"),
                            new KeyboardButton("Новинки"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("🤝 Связаться с нами / 🛠️ Техподдержка"),
                            new KeyboardButton("Частые вопросы"),
                        }
                    })
                {
                    ResizeKeyboard = true,
                };

                if (message.Text == "/start" || message.Text == "/testStart")
                {
                    if (CheckAdmin(update))
                    {
                        await botClient.SendTextMessageAsync(chat.Id, $"Здраствуйте, {_admins.FirstOrDefault((x) => x.ID == chat.Id.ToString()).Name}, вы имеете права администратора, напишите /adminPanel, чтобы открыть панель администратора",
                            replyMarkup: replyKeyboard, replyToMessageId: message.MessageId);
                        if(message.Text != "/testStart") return;
                    }

                    await botClient.SendTextMessageAsync(chat.Id, "Продолжая переписку вы даёте согласие на обработку ваших персональных данных и получение информационной рассылки.С Политикой обработки персональных данных можно ознакомиться здесь:ссылка на сайт с политикой обработки данных",
                         cancellationToken: cancellationToken);

                    Task.Delay(1000);

                    await botClient.SendTextMessageAsync(chat.Id,
                        "Привет! 🎉 Рад приветствовать тебя в Лаборатории интерактивных игр! 😊 Я - Никита, твой гид в мире веселья и приключений. Здесь мы создаем игры для всех видов интерактивного оборудования. 🎮 Ты можешь у нас не только заказать уникальную игру, но и приобрести уже готовые варианты! 🕹️",
                         cancellationToken: cancellationToken);

                    Task.Delay(1000);

                    var newsMessages = SQL.GetNewsMessages();

                    if (newsMessages.Count > 0)
                    {
                        foreach (var newsMessage in newsMessages)
                        {
                            await botClient.SendTextMessageAsync(chat.Id, newsMessage,
                                cancellationToken: cancellationToken);

                            Task.Delay(1000);
                        }
                    }
                    

                    await botClient.SendTextMessageAsync(chat.Id,
                        "Как я могу помочь сегодня ? 🚀!",
                        replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                    return;
                }
                else
                {
                    await botClient.SendTextMessageAsync(chat.Id, "Возвращаю в меню",
                        replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                }
            }
        }

        private static async void CheckGame(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (message.Text == "Хочу посмотреть другие наборы! 🌐🎲" && GetUserData(chat.Id)._gamePack != null)
            {
                GetUserData(chat.Id)._isSelectGame = true;
            }

            if (GetUserData(chat.Id)._games.Count > 0 && GetUserData(chat.Id)._isSelectGame)
            {
                await Task.Delay(1000);
                var game = GetUserData(chat.Id)._games.FirstOrDefault((x) => x.Name == message.Text);

                if (message.Text == "Хочу посмотреть другие наборы! 🌐🎲" && GetUserData(chat.Id)._gamePack != null) game = GetUserData(chat.Id)._gamePack;
                
                if (game != null)
                {
                    if (game.IsPack)
                    {
                        GetUserData(chat.Id)._isSelectGame = false;
                        GetUserData(chat.Id)._gamePack = game;

                        await botClient.SendTextMessageAsync(chat.Id, game.Desciption,
                            cancellationToken: cancellationToken);

                        Task.Delay(2000);

                        SendGamesPool(botClient, SQL.GetGamePacks(game.Name), chat);
                        return;
                    }


                    await botClient.SendPhotoAsync(chat.Id, InputFileStream.FromStream(File.OpenRead(Environment.CurrentDirectory + _imagesPath + game.ImageName)), caption: game.Desciption);

                    Prices prices = SQL.GetPrices(game.Name);

                    if(prices.GamePrices.Count == 0) throw new Exception("Prices is Null");

                    if(prices.GamePrices.Count == 1) game.SetDatePrice(prices.GamePrices[0]);

                    List<KeyboardButton[]> selectGameKeyboardList = new();
                    selectGameKeyboardList.Add(new KeyboardButton[]
                    {
                        new KeyboardButton("Купить 🛒"),
                    });
                    selectGameKeyboardList.Add(new KeyboardButton[]
                    {
                        new KeyboardButton("Каталог"),
                        new KeyboardButton("Меню"),
                    });

                    if (GetUserData(chat.Id)._gamePack != null)
                    {
                        selectGameKeyboardList.Add(new KeyboardButton[]
                        {
                            new KeyboardButton("Хочу посмотреть другие наборы! 🌐🎲"),
                        });
                    }

                    var replyKeyboard = new ReplyKeyboardMarkup(selectGameKeyboardList)
                    {
                        ResizeKeyboard = true,
                    };

                    GetUserData(chat.Id)._gameToBuy = game;

                    await botClient.SendTextMessageAsync(chat.Id, "Прекрасный выбор, желаете купить?",
                        replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                    return;
                }
            }
        }

        private static async void ShowPrivacy(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (message.Text == "Купить \U0001f6d2" && GetUserData(chat.Id)._gameToBuy != null)
            {
                var replyKeyboard = new ReplyKeyboardMarkup(
                    new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Согласен"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Меню"),
                        }
                    })
                {
                    ResizeKeyboard = true,
                };

                await botClient.SendTextMessageAsync(chat.Id, "Пожалуйста, ознакомтесь с политикой конфиденциальности и пользовательским соглашением на обработку персональных данных",
                    replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                return;
            }
        }

        private static async void Regestration(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (GetUserData(chat.Id)._isPay) return;

            if (message.Text == "Согласен" && GetUserData(chat.Id)._gameToBuy != null)
            {
                if (SQL.IsUserExist(chat.Id.ToString()))
                {
                    Console.WriteLine("Пользователь зарегестрирован");
                    GetUserData(chat.Id)._isPaymentInsturction = true;
                    return;
                }

                GetUserData(chat.Id)._regestrationState = RegestrationState.Name;
                await botClient.SendTextMessageAsync(chat.Id, "Прекрасный выбор! Для завершения покупки, напишите мне следующие данные:",
                    replyMarkup: _backKeyboardMarkup, cancellationToken: cancellationToken);
            }

            if (GetUserData(chat.Id)._gameToBuy != null && GetUserData(chat.Id)._regestrationState != RegestrationState.empty)
            {
                if (message.Text == "Верно")
                {
                    if (GetUserData(chat.Id)._username != "" && GetUserData(chat.Id)._phone != "" && GetUserData(chat.Id)._email != "")
                    {
                        SQL.RegistrateUser(chat.Id.ToString(), GetUserData(chat.Id)._username, GetUserData(chat.Id)._phone, GetUserData(chat.Id)._email);
                        GetUserData(chat.Id)._regestrationState = RegestrationState.empty;
                        GetUserData(chat.Id)._isPaymentInsturction = true;
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chat.Id, $"ФИО или номер, не могут быть пустыми",
                            cancellationToken: cancellationToken);
                        GetUserData(chat.Id)._regestrationState = RegestrationState.Name;
                        return;
                    }
                }

                else if (message.Text == "Не верно")
                {
                    GetUserData(chat.Id)._regestrationState = RegestrationState.Name;
                }

                switch (GetUserData(chat.Id)._regestrationState)
                {
                    case RegestrationState.Name:
                    {
                        if (!GetUserData(chat.Id)._isRegistrationInput)
                        {
                            await botClient.SendTextMessageAsync(chat.Id, "Ваше ФИО 📝",
                                replyMarkup: _backKeyboardMarkup, cancellationToken: cancellationToken);
                            GetUserData(chat.Id)._isRegistrationInput = true;
                            return;
                        }
                        else
                        {
                            GetUserData(chat.Id)._isRegistrationInput = false;
                            GetUserData(chat.Id)._username = message.Text;
                            GetUserData(chat.Id)._regestrationState = RegestrationState.PhoneNumber;
                            Regestration(botClient, update, cancellationToken);
                        }

                        break;
                    }
                    case RegestrationState.PhoneNumber:
                    {
                        if (!GetUserData(chat.Id)._isRegistrationInput)
                        {
                            await botClient.SendTextMessageAsync(chat.Id, "Номер телефона 📱",
                                replyMarkup: _backKeyboardMarkup, cancellationToken: cancellationToken);

                            GetUserData(chat.Id)._isRegistrationInput = true;
                            return;
                        }
                        else
                        {
                            GetUserData(chat.Id)._phone = message.Text;
                            GetUserData(chat.Id)._isRegistrationInput = false;
                            GetUserData(chat.Id)._regestrationState = RegestrationState.Email;
                            Regestration(botClient, update, cancellationToken);
                        }
                                
                        break;
                    }
                    case RegestrationState.Email:
                    {
                        if (!GetUserData(chat.Id)._isRegistrationInput)
                        {
                            await botClient.SendTextMessageAsync(chat.Id, "Электронная почта 📧",
                                replyMarkup: _backKeyboardMarkup, cancellationToken: cancellationToken);

                            GetUserData(chat.Id)._isRegistrationInput = true;
                        }
                        else
                        {
                            GetUserData(chat.Id)._email = message.Text;
                            GetUserData(chat.Id)._isRegistrationInput = false;

                                var replyKeyboard = new ReplyKeyboardMarkup(
                                new List<KeyboardButton[]>()
                                {
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Верно"),
                                        new KeyboardButton("Не верно"),
                                    },
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Меню"),
                                    }
                                })
                            {
                                ResizeKeyboard = true,
                            };

                            await botClient.SendTextMessageAsync(chat.Id, $"Ваше ФИО: {GetUserData(chat.Id)._username} \nВаш номер: {GetUserData(chat.Id)._phone}\nВаша почта: {GetUserData(chat.Id)._email} \nВсе верно?",
                                replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                            return;
                        }
                        break;
                    }
                }
            }
        }

        private static async void PaymentInstruction(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (GetUserData(chat.Id)._isPay) return;

            if (message.Text == "Оплатить 💳")
            {
                await botClient.SendTextMessageAsync(chat.Id, $"Прекрасно! 🌟 Давайте завершим вашу покупку.\r\nДля этого я отправлю вам счет на онлайн-оплату. 🌐💳 После успешной оплаты, я мгновенно отправлю вам файл игры и подробную инструкцию по установке. 📤🎮 Спасибо за ваш заказ!😊🚀",
                    replyMarkup: _backKeyboardMarkup, cancellationToken: cancellationToken);
                SendPayment(botClient, update, cancellationToken);
                return;
            }

            if (GetUserData(chat.Id)._isPaymentInsturction)
            {
                var replyKeyboard = new ReplyKeyboardMarkup(
                    new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Оплатить 💳"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Меню"),
                        }
                    })
                {
                    ResizeKeyboard = true,
                };

                await botClient.SendTextMessageAsync(chat.Id, $"Отлично! Вот детали вашей покупки: \n🏷️ Цена: {GetUserData(chat.Id)._gameToBuy.DatePrice.Amount} рублей\n🎮 Название: {GetUserData(chat.Id)._gameToBuy.Name}\r\n⏰ Срок лицензии: {_dateTimeNames[GetUserData(chat.Id)._gameToBuy.DatePrice.DateTime]}",
                    replyMarkup: replyKeyboard, cancellationToken: cancellationToken);

                GetUserData(chat.Id)._isPaymentInsturction = false;
                return;
            }
        }

        private static async void SendPayment(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var chat = update.Message.Chat;

            if(GetUserData(chat.Id)._gameToBuy.DatePrice == null) return;

            Console.WriteLine(chat.Id.ToString());
            await botClient.SendInvoiceAsync(chatId: update.Message.Chat.Id,
                title: GetUserData(chat.Id)._gameToBuy.Name,
                description: GetUserData(chat.Id)._gameToBuy.Desciption,
                payload: "somePayload",
                providerToken: _paymentToken,
                currency: "RUB",
                prices: new List<LabeledPrice>() { new LabeledPrice(GetUserData(chat.Id)._gameToBuy.Name, GetUserData(chat.Id)._gameToBuy.DatePrice.Amount * 100) },
                isFlexible: false, startParameter: "start"
            );
        }

        private static async void Cotalog(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (message.Text == "Каталог" || message.Text == "/catalog")
            {
                await botClient.SendTextMessageAsync(chat.Id, "Хорошо, подождите пожалуйста");

                var games = SQL.GetGames();

                SendGamesPool(botClient, games, chat);
                return;
            }
        }

        private static async void SendGamesPool(ITelegramBotClient botClient, List<Game> games, Chat chat)
        {
            List<KeyboardButton[]> catalogKeyboard = new();
            KeyboardButton[] gamesButtons = new KeyboardButton[games.Count];

            for (int i = 0; i < games.Count; i++)
            {
                await botClient.SendPhotoAsync(chat.Id,
                    InputFileStream.FromStream(File.OpenRead(Environment.CurrentDirectory + _imagesPath + games[i].ImageName)),
                    caption: games[i].Name);
                gamesButtons[i] = new KeyboardButton(games[i].Name);
            }

            catalogKeyboard.Add(gamesButtons);
            catalogKeyboard.Add(new KeyboardButton[] { new KeyboardButton("Меню") });

            ReplyKeyboardMarkup _gamesKeyboardMarkup = new ReplyKeyboardMarkup(catalogKeyboard);
            _gamesKeyboardMarkup.ResizeKeyboard = true;
            GetUserData(chat.Id)._games = games;

            GetUserData(chat.Id)._isSelectGame = true;

            await botClient.SendTextMessageAsync(chat.Id, "Какой набор вас заинтересовал? 😎🎁",
                replyMarkup: _gamesKeyboardMarkup);
        }

        private static async void NewGames(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (message.Text == "Новинки" || message.Text == "/new")
            {
                await botClient.SendTextMessageAsync(chat.Id, "Хорошо, показываю новинки, подождите пожалуйста");

                var games = SQL.GetNewGames();

                SendGamesPool(botClient, games, chat);
                return;
            }
        }

        private static async void Privacy(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (message.Text == "Соглашение" || message.Text == "/privacy")
            {
                await botClient.SendTextMessageAsync(chat.Id, "Соглашение",
                    replyToMessageId: message.MessageId);
                return;
            }
        }

        private static async void QA(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            
            if (message.Text == "Частые вопросы")
            {
                GetUserData(chat.Id)._isGetQuestion = true;
                GetUserData(chat.Id)._qaList = SQL.GetQA();

                List<KeyboardButton[]> qaKeyboard = new();

                for (int i = 0; i < GetUserData(chat.Id)._qaList.Count; i++)
                {
                    qaKeyboard.Add(new KeyboardButton[]{ new KeyboardButton(GetUserData(chat.Id)._qaList[i].Question)});
                }
                qaKeyboard.Add(new KeyboardButton[] { new KeyboardButton("У меня другой вопрос. Как с вами связаться? 🤔"), new KeyboardButton("Меню") });
                
                ReplyKeyboardMarkup _qaKeyboardMarkup = new ReplyKeyboardMarkup(qaKeyboard);

                await botClient.SendTextMessageAsync(chat.Id, "Как вам удобно связаться с нами ? 🤗",
                    replyMarkup: _qaKeyboardMarkup);
                return;
            }

            if (GetUserData(chat.Id)._isGetQuestion)
            {
                var qa = GetUserData(chat.Id)._qaList.FirstOrDefault((x) => x.Question == message.Text);

                if (qa != null)
                {
                    var replyKeyboard = new ReplyKeyboardMarkup(
                        new List<KeyboardButton[]>()
                        {
                            new KeyboardButton[]
                            {
                                new KeyboardButton("Да, спасибо! 😊"),
                                new KeyboardButton("Я хочу задать свой вопрос. Как мне связаться с вами? 📩"),
                            },
                        })
                    {
                        ResizeKeyboard = true,
                    };


                    await botClient.SendTextMessageAsync(chat.Id, qa.Answer, cancellationToken: cancellationToken);

                    Task.Delay(3000);

                    await botClient.SendTextMessageAsync(chat.Id, "Вы нашли овет на свой вопрос?",
                        replyMarkup: replyKeyboard);
                }

                if (message.Text == "Да, спасибо! 😊")
                {
                    await botClient.SendTextMessageAsync(chat.Id, "Пожалуйста! Если у тебя будут еще вопросы, я здесь, чтобы помочь. 🌟",
                        replyMarkup: _backKeyboardMarkup);
                }
            }
        }

        private static async void Support(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            var replyKeyboard = new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>()
                {
                    new KeyboardButton[]
                    {
                        new KeyboardButton("📞 Контакты"),
                        new KeyboardButton("💬 Мессенджеры"),
                    },
                })
            {
                ResizeKeyboard = true,
            };


            if (message.Text == "🤝 Связаться с нами / 🛠️ Техподдержка" || message.Text == "Я хочу задать свой вопрос. Как мне связаться с вами? 📩" || message.Text == "У меня другой вопрос. Как с вами связаться? 🤔" || message.Text == "/support")
            {
                await botClient.SendTextMessageAsync(chat.Id, "Как вам удобно связаться с нами ? 🤗",
                    replyMarkup: replyKeyboard);
                return;
            }
        }

        private static async void Contacts(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            var replyKeyboard = new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>()
                {
                    new KeyboardButton[]
                    {
                        new KeyboardButton("📱 WhatsApp, мне подойдет"),
                        new KeyboardButton("📬 Telegram, мне подходит"),
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("Меню"),
                    }
                })
            {
                ResizeKeyboard = true,
            };

            if (message.Text == "📞 Контакты" || message.Text == "/contacts")
            {
                string phone;
                string mail;
                string website;
                string companyName;

                SQL.GetCompanyContacts(out phone, out mail, out website, out companyName);

                await botClient.SendTextMessageAsync(chat.Id, $"\ud83d\ude0a Вот наши контакты для связи:\n\u260e\ufe0f Номер телефона: {phone}\n\ud83d\udce7 Почта: {mail}\n\ud83c\udf10 Сайт: {website}\n\ud83c\udfe2 ООО Имя: {companyName}\nНе стесняйтесь обращаться! Мы всегда рады помочь. \ud83d\ude0a",
                    replyToMessageId: message.MessageId, replyMarkup: _backKeyboardMarkup);
                return;
            }

            if (message.Text == "💬 Мессенджеры")
            {
                await botClient.SendTextMessageAsync(chat.Id, "\nОтличный выбор! \ud83d\udcac Выберите удобный мессенджер для связи.",
                    replyMarkup: replyKeyboard);
                return;
            }

            if (message.Text == "📱 WhatsApp, мне подойдет")
            {
                await botClient.SendTextMessageAsync(chat.Id,
                    $"Отличный выбор! \ud83c\udf1f\nВот ссылка на чат в \ud83d\udcf1 WhatsApp.\n{SQL.GetWhatsAppLink()}\nПриятного диалога! \ud83d\udcac\nЕсли что-то нужно или есть вопросы, напишите мне! \ud83d\ude80",
                    replyMarkup: _backKeyboardMarkup);
                return;
            }

            if (message.Text == "📬 Telegram, мне подходит")
            {
                await botClient.SendTextMessageAsync(chat.Id,
                    $"Отличный выбор! \ud83c\udf1f\nВот ссылка на чат в \ud83d\udcec Telegram.\n{SQL.GetTelegramLink()}\nПриятного диалога! \ud83d\udcac\nЕсли что-то нужно или есть вопросы, напишите мне! \ud83d\ude80",
                    replyMarkup: _backKeyboardMarkup);
                return;
            }
        }
        #endregion

        #region Admin

        private static async void StartAdminPanel(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (message.Text == "/adminPanel")
            {
                GetUserData(chat.Id).ResetDate();

                var replyKeyboard = new ReplyKeyboardMarkup(
                    new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Создать ключ и внести данные клиента"),
                            new KeyboardButton("Рассылка"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Меню"),
                        }
                    })
                {
                    ResizeKeyboard = true,
                };

                await botClient.SendTextMessageAsync(chat.Id, "Вы в админ панеле",
                    replyMarkup: replyKeyboard);
            }
        }

        private static async void AdminRegistrate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (message.Text == "Создать ключ и внести данные клиента" || message.Text == "Не верно")
            {
                GetUserData(chat.Id)._adminRegestrationState = RegestrationState.Name;
            }

            switch (GetUserData(chat.Id)._adminRegestrationState)
            {
                case RegestrationState.Name:
                {
                    if (!GetUserData(chat.Id)._isRegistrationInput)
                    {
                        await botClient.SendTextMessageAsync(chat.Id, "Введите ФИО",
                            replyMarkup: _backKeyboardMarkup, cancellationToken: cancellationToken);
                        GetUserData(chat.Id)._isRegistrationInput = true;
                        return;
                    }
                    else
                    {
                        GetUserData(chat.Id)._isRegistrationInput = false;
                        GetUserData(chat.Id)._username = message.Text;
                        GetUserData(chat.Id)._adminRegestrationState = RegestrationState.PhoneNumber;
                        AdminRegistrate(botClient, update, cancellationToken);
                    }

                    break;
                }
                case RegestrationState.PhoneNumber:
                {
                    if (!GetUserData(chat.Id)._isRegistrationInput)
                    {
                        await botClient.SendTextMessageAsync(chat.Id, "Номер телефона",
                            replyMarkup: _backKeyboardMarkup, cancellationToken: cancellationToken);

                        GetUserData(chat.Id)._isRegistrationInput = true;
                        return;
                    }
                    else
                    {
                        GetUserData(chat.Id)._phone = message.Text;
                        GetUserData(chat.Id)._isRegistrationInput = false;
                        GetUserData(chat.Id)._adminRegestrationState = RegestrationState.Email;
                        AdminRegistrate(botClient, update, cancellationToken);
                    }

                    break;
                }
                case RegestrationState.Email:
                {
                    if (!GetUserData(chat.Id)._isRegistrationInput)
                    {
                        await botClient.SendTextMessageAsync(chat.Id, "Электронная почта",
                            replyMarkup: _backKeyboardMarkup, cancellationToken: cancellationToken);

                        GetUserData(chat.Id)._isRegistrationInput = true;
                    }
                    else
                    {
                       GetUserData(chat.Id)._email = message.Text;
                       GetUserData(chat.Id)._isRegistrationInput = false;

                        GetUserData(chat.Id)._adminRegestrationState = RegestrationState.Price;
                        AdminRegistrate(botClient, update, cancellationToken);
                    }

                    break;
                }
                case RegestrationState.Price:
                {
                    if (!GetUserData(chat.Id)._isRegistrationInput)
                    {
                        await botClient.SendTextMessageAsync(chat.Id, "Сумма оплаты",
                            replyMarkup: _backKeyboardMarkup, cancellationToken: cancellationToken);

                        GetUserData(chat.Id)._isRegistrationInput = true;
                    }
                    else
                    {
                        GetUserData(chat.Id)._price = int.Parse(message.Text);
                        GetUserData(chat.Id)._isRegistrationInput = false;
                        GetUserData(chat.Id)._adminRegestrationState = RegestrationState.empty;

                        var replyKeyboard = new ReplyKeyboardMarkup(
                            new List<KeyboardButton[]>()
                            {
                                new KeyboardButton[]
                                {
                                    new KeyboardButton("Верно"),
                                    new KeyboardButton("Не верно"),
                                },
                                new KeyboardButton[]
                                {
                                    new KeyboardButton("Меню"),
                                }
                            })
                        {
                            ResizeKeyboard = true,
                        };

                        await botClient.SendTextMessageAsync(chat.Id,
                            $"Проверте данные: \nФИО: {GetUserData(chat.Id)._username} \nНомер: {GetUserData(chat.Id)._phone}\nПочта: {GetUserData(chat.Id)._email} \nСумма оплаты: {GetUserData(chat.Id)._price} \nВсе верно?",
                            replyMarkup: replyKeyboard, cancellationToken: cancellationToken);
                        return;
                    }

                    break;
                    }
            }
        }

        private static async void AdminGenerateLicense(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (message.Text == "Верно")
            {
                GetUserData(chat.Id)._games = SQL.GetGames();

                ShowGamesKeyboard(botClient, update, GetUserData(chat.Id)._games);
                GetUserData(chat.Id)._isGenerateLicenseAdmin = true;
            }

            if (GetUserData(chat.Id)._isGenerateLicenseAdmin)
            {
                if (GetUserData(chat.Id)._gameToBuy == null)
                {
                    var game = GetUserData(chat.Id)._games.FirstOrDefault((x) => x.Name == message.Text);
                    if (game != null)
                    {
                        if (game.IsPack)
                        {
                            GetUserData(chat.Id)._games = SQL.GetGamePacks(game.Name);
                            ShowGamesKeyboard(botClient, update, GetUserData(chat.Id)._games);
                            return;
                        }

                        var replyKeyboard = new ReplyKeyboardMarkup(
                            new List<KeyboardButton[]>()
                            {
                                new KeyboardButton[]
                                {
                                    new KeyboardButton("Безлимитный"),
                                    new KeyboardButton("1 мес"),
                                    new KeyboardButton("3 мес"),
                                    new KeyboardButton("6 мес"),
                                    new KeyboardButton("1 год"),
                                    new KeyboardButton("3 года"),
                                },
                                new KeyboardButton[]
                                {
                                    new KeyboardButton("Меню"),
                                }
                            })
                        {
                            ResizeKeyboard = true,
                        };

                        GetUserData(chat.Id)._gameToBuy = game;
                        await botClient.SendTextMessageAsync(chat.Id, "Введите срок",
                            replyMarkup: replyKeyboard);
                        return;
                    }
                }

                if (GetUserData(chat.Id)._gameToBuy != null)
                {
                    if (message.Text == "Безлимитный")
                    {
                        GetUserData(chat.Id)._gameToBuy.SetDatePrice(new DatePrice(GameDateTime.Unlimited, 1));
                        await botClient.SendTextMessageAsync(chat.Id, "Введите код",
                            replyMarkup: _backKeyboardMarkup);
                        return;
                    }
                    if (message.Text == "1 мес")
                    {
                        GetUserData(chat.Id)._gameToBuy.SetDatePrice(new DatePrice(GameDateTime.OneMonth, 1));
                        await botClient.SendTextMessageAsync(chat.Id, "Введите код",
                            replyMarkup: _backKeyboardMarkup);
                        return;
                    }
                    if (message.Text == "3 мес")
                    {
                        GetUserData(chat.Id)._gameToBuy.SetDatePrice(new DatePrice(GameDateTime.ThreeMonth, 1));
                        await botClient.SendTextMessageAsync(chat.Id, "Введите код",
                            replyMarkup: _backKeyboardMarkup);
                        return;
                    }
                    if (message.Text == "6 мес")
                    {
                        GetUserData(chat.Id)._gameToBuy.SetDatePrice(new DatePrice(GameDateTime.SixMounth, 1));
                        await botClient.SendTextMessageAsync(chat.Id, "Введите код",
                            replyMarkup: _backKeyboardMarkup);
                        return;
                    }
                    if (message.Text == "1 год")
                    {
                        GetUserData(chat.Id)._gameToBuy.SetDatePrice(new DatePrice(GameDateTime.OneYear, 1));
                        await botClient.SendTextMessageAsync(chat.Id, "Введите код",
                            replyMarkup: _backKeyboardMarkup);
                        return;
                    }
                    if (message.Text == "3 года")
                    {
                        GetUserData(chat.Id)._gameToBuy.SetDatePrice(new DatePrice(GameDateTime.ThreeYear, 1));
                        await botClient.SendTextMessageAsync(chat.Id, "Введите код",
                            replyMarkup: _backKeyboardMarkup);
                        return;
                    }

                    if (message.Text.Length == 8 && GetUserData(chat.Id)._gameToBuy.DatePrice != null)
                    {
                        string openKey = null;
                        string closeKey = null;

                        SQL.GetOpenCloseKeys(GetUserData(chat.Id)._gameToBuy.Name, out openKey, out closeKey);

                        SerialNumberGeneratorTools.CloseKey = closeKey;
                        SerialNumberGeneratorTools.OpenKey = openKey;

                        GetUserData(chat.Id)._hardwareID = message.Text;

                        var licenseKey = SerialNumberGeneratorTools.MakeLicense(GetUserData(chat.Id)._hardwareID, GetDate(GetUserData(chat.Id)._gameToBuy.DatePrice.DateTime), "", "");

                        SQL.AdminRegistrateOrder(GetUserData(chat.Id)._username, GetUserData(chat.Id)._phone, GetUserData(chat.Id)._email, GetUserData(chat.Id)._hardwareID, licenseKey, GetUserData(chat.Id)._price, GetUserData(chat.Id)._gameToBuy.Name, _dateTimeMonthsValues[GetUserData(chat.Id)._gameToBuy.DatePrice.DateTime], DateTime.Now.ToString());

                        await botClient.SendTextMessageAsync(chat.Id, licenseKey,
                            replyMarkup: _backKeyboardMarkup);
                    }
                }
            }
        }

        private static async void PushDistribution(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (message.Text == "Рассылка" || message.Text == "Написать заново")
            {
                await botClient.SendTextMessageAsync(chat.Id, "Введите сообщение",
                    replyMarkup: _backKeyboardMarkup);

                GetUserData(chat.Id)._isGetDistributionMessage = true;
                return;
            }

            if (GetUserData(chat.Id)._isGetDistributionMessage)
            {
                if(message.Text == "/menu" || message.Text == "Меню" || message.Text == "/catalog" || message.Text == "/support" || message.Text == "/start" || message.Text == "/new") return;

                if (message.Text == "Разослать")
                {
                    var users = SQL.GetListUsersId();

                    for (int i = 0; i < users.Count; i++)
                    {
                        await botClient.SendTextMessageAsync(users[i], GetUserData(chat.Id)._distributionText);
                    }

                    await botClient.SendTextMessageAsync(chat.Id, "Сообщения разосланы", replyMarkup:_backKeyboardMarkup);
                    GetUserData(chat.Id)._isGetDistributionMessage = false;
                    return;
                }

                var replyKeyboard = new ReplyKeyboardMarkup(
                    new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Разослать"),
                            new KeyboardButton("Написать заново"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("Меню"),
                        }
                    })
                {
                    ResizeKeyboard = true,
                };

                GetUserData(chat.Id)._distributionText = message.Text;

                await botClient.SendTextMessageAsync(chat.Id, "Отослать ваше сообщение?",
                    replyMarkup: replyKeyboard, replyToMessageId: message.MessageId);
                
            }
        }

        private static bool CheckAdmin(Update update)
        {
            _admins = SQL.GetAdmins();

            for (int i = 0; i < _admins.Count; i++)
            {
                if (update.Message.Chat.Id.ToString() == _admins[i].ID) return true;
            }

            return false;
        }

        private static async void ShowGamesKeyboard(ITelegramBotClient botClient, Update update, List<Game> games)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            List<KeyboardButton[]> catalogKeyboard = new();
            KeyboardButton[] gamesButtons = new KeyboardButton[games.Count];

            for (int i = 0; i < games.Count; i++)
            {
                gamesButtons[i] = new KeyboardButton(games[i].Name);
            }

            catalogKeyboard.Add(gamesButtons);
            catalogKeyboard.Add(new KeyboardButton[] { new KeyboardButton("Меню") });

            ReplyKeyboardMarkup _gamesKeyboardMarkup = new ReplyKeyboardMarkup(catalogKeyboard);
            _gamesKeyboardMarkup.ResizeKeyboard = true;

            await botClient.SendTextMessageAsync(chat.Id, "Выберете игру",
                replyMarkup: _gamesKeyboardMarkup);
        }

        #endregion


        private static async void SystemCommands(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var chat = update.Message.Chat;

            if (message.Text == "/GetMyId")
            {
                await botClient.SendTextMessageAsync(chat.Id, update.Message.Chat.Id.ToString(),
                    replyToMessageId: message.MessageId);
            }
        }

        private static DateTime GetDate(GameDateTime gameDateTime)
        {
            switch (gameDateTime)
            {
                case GameDateTime.Unlimited:
                {
                    return DateTime.UtcNow.AddYears(2000);
                }
                case GameDateTime.OneMonth:
                {
                    return DateTime.UtcNow.AddMonths(1);
                }
                case GameDateTime.ThreeMonth:
                {
                    return DateTime.UtcNow.AddMonths(3);
                }
                case GameDateTime.SixMounth:
                {
                    return DateTime.UtcNow.AddMonths(6);
                }
                case GameDateTime.OneYear:
                {
                    return DateTime.UtcNow.AddYears(1);
                }
                case GameDateTime.ThreeYear:
                {
                    return DateTime.UtcNow.AddYears(3);
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private static UserData GetUserData(long chatId)
        {
            if (!_userData.ContainsKey(chatId.ToString()))
            {
                _userData.Add(chatId.ToString(), new UserData());
            }

            var userData = _userData[chatId.ToString()];
            return userData;
        }
    }

    enum RegestrationState
    {
        empty,
        Name,
        PhoneNumber,
        Email,
        Price
    }
}