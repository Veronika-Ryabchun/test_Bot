using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Exceptions;
using TelegramBot.Client;

namespace TelegramBot
{
    public class RecipeBot
    {
        const string getRecipe = "Get";
        const string addToFavorite = "Add";
        const string deleteFromFavorite = "Delete";
        TelegramBotClient botClient = new TelegramBotClient("5407272397:AAFb1NxscTLG74sybGLEhRWqeAfTZ4I06gk");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        FoodClient foodClient = new FoodClient();
        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"{botMe.Username} почав працювати");
            Console.ReadKey();
        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот АПІ:\n {apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                await HandlerMessageAsync(botClient, update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery.Data != null)
            {
                await HandlerCallbackAsync(botClient, update.CallbackQuery);
            }
        }
        private async Task HandlerCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data.StartsWith(getRecipe))
            {
                await Conclusion(callbackQuery.Data.Substring(getRecipe.Length), callbackQuery.Message.Chat.Id);
            }
            else if (callbackQuery.Data.StartsWith(addToFavorite))
            {
                await AddToFavoriteAsync(callbackQuery.Data.Substring(addToFavorite.Length), callbackQuery.Message.Chat.Id);
            }
            else if (callbackQuery.Data.StartsWith(deleteFromFavorite))
            {
                await DeleteFromFavoriteAsync(callbackQuery.Data.Substring(deleteFromFavorite.Length), callbackQuery.Message.Chat.Id);
            }
}
        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                Console.WriteLine(message.Chat.Id);
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter /keyboard to get started");
                return;
            }
            /*else if (message.Text == "/inline")
            {
                InlineKeyboardMarkup keyboardMarkup = new
                (
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("текст рецепту", $"Рецепт")
                    }
                );
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть рецепт", replyMarkup: keyboardMarkup);
                return;
            }*/
            else if (message.Text == "/keyboard")
            {
                ReplyKeyboardMarkup replyKeyboardMapkup = new
                    (
                    new[]
                        {
                            new KeyboardButton[] {"Favorites", "Clear favorites"},
                            new KeyboardButton[] {"Random"}
                        }
                    )
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Select button or enter ingredient", replyMarkup: replyKeyboardMapkup);
                return;
            }
            else if (message.Text == "Favorites")
            {
                await PrintFavoritesAsync(message.Chat.Id);
            }
            else if (message.Text == "Clear favorites")
            {
                await ClearFavoritesAsync(message.Chat.Id);
            }
            else if (message.Text == "Random")
            {
                await RandomAsync(message);
            }
            else
            {
                await RecipeListAsync(message);
            }
        }
        public async Task Conclusion(string name, long messageChatId)
        {
            var result = foodClient.GetFoodRecipeAsync(name).Result;
            if (result == null || result.Count == 0)
                await botClient.SendTextMessageAsync(messageChatId, "Nothing found");
            else
            {
                string dishRecipe = $"{result[0].title}\n{result[0].ingredients}\n{result[0].servings}\n{result[0].instructions}\n";
                var favResult = foodClient.GetFavoritesAsync(messageChatId.ToString()).Result;
                var favList = favResult.ConvertAll(recipe => recipe.Recipe).ToArray();
                InlineKeyboardMarkup keyboardMarkup;
                if (favList.Contains(name))
                {
                    keyboardMarkup = new[] { new[] { InlineKeyboardButton.WithCallbackData("Delete from favorites", $"{Cut(deleteFromFavorite, result[0].title)}") } };
                }
                else
                {
                    keyboardMarkup = new[] { new[] { InlineKeyboardButton.WithCallbackData("Add to favorites", $"{Cut(addToFavorite, result[0].title)}") } };
                }
                await botClient.SendTextMessageAsync(messageChatId, dishRecipe, replyMarkup: keyboardMarkup);
            }
        }
        public async Task RecipeListAsync(Message message)
        {
            var result = foodClient.GetFoodRecipeAsync(message.Text).Result;
            if (result == null || result.Count == 0)
                await botClient.SendTextMessageAsync(message.Chat.Id, "Nothing found");
            else
            {
                /*foreach (Model.Recipe recipe  in result)
                {
                    Console.WriteLine($"{recipe.title.Replace(':', '_').Replace('/', '_').Replace("'"[0], '_').Replace(',', '_').Replace('(', '_').Replace(')', '_').Replace('-', '_')}");
                }*/ 
                InlineKeyboardMarkup keyboardMarkup = result.ConvertAll(recipe => new[] { InlineKeyboardButton.WithCallbackData(recipe.title, $"{Cut(getRecipe, recipe.title)}") }).ToArray();
                await botClient.SendTextMessageAsync(message.Chat.Id, "Select recipe", replyMarkup: keyboardMarkup);
            }
        }
        public async Task RandomAsync(Message message)
        {
            string[] Random = { "Chicken", "Salad", "Olive", "Broccoli", "Fruit", "Garlic", "Herb", "Cream", "Cheese", "Vegetable", "Noodles", "Sause", "Rise", "Cake" };
            Random random = new Random();
            string choose = Random[random.Next(0, 13)];
            await botClient.SendTextMessageAsync(message.Chat.Id, "Random recipe");
            await Conclusion(choose, message.Chat.Id);
        }
        public async Task AddToFavoriteAsync(string title, long messageChatId)
        {
            await foodClient.PostFavoriteRecipeAsync(title, messageChatId.ToString());
            await botClient.SendTextMessageAsync(messageChatId, $"{title} was added to favorites");
        }
        public async Task PrintFavoritesAsync(long messageChatId)
        {
            var result = foodClient.GetFavoritesAsync(messageChatId.ToString()).Result;
            InlineKeyboardMarkup keyboardMarkup = result.ConvertAll(recipe => new[] { InlineKeyboardButton.WithCallbackData(recipe.Recipe, $"{Cut(getRecipe, recipe.Recipe)}") }).ToArray();
            await botClient.SendTextMessageAsync(messageChatId, "Favorites", replyMarkup: keyboardMarkup);
        }
        public async Task DeleteFromFavoriteAsync(string title, long messageChatId)
        {
            await foodClient.DeleteFavoriteRecipeAsync(title, messageChatId.ToString());
            await botClient.SendTextMessageAsync(messageChatId, $"{title} was deleted from favorites");
        }
        public async Task ClearFavoritesAsync(long messageChatId)
        {
            await foodClient.ClearFavoritesAsync(messageChatId.ToString());
            await botClient.SendTextMessageAsync(messageChatId, $"All recipes were deleted from favorites");
        }
        public string Cut(string prefix, string title)
        {
            string callBackData = prefix + title;
            if (callBackData.Length > 64)
                return callBackData.Substring(0, 64);
            else
                return callBackData;
        }
    }
}
