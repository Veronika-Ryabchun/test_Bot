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
        const string diet = "Diet";
        Dictionary<string, string> diets = new Dictionary<string, string>
        {
            { "dairy_free", "Dairy-Free"},
            { "vegan", "Vegan"},
            { "pescatarian" , "Pescatarian"},
            { "contains_alcohol" , "Contains Alcohol"},
            { "low_carb" , "Low-Carb"},
            { "indulgent_sweets" , "Indulgent Sweets"},
            { "vegetarian" , "Vegetarian"},
            { "kid_friendly" , "Kid-Friendly"},
            { "gluten_free" , "Gluten-Free"},
        };
TelegramBotClient botClient = new TelegramBotClient("5457813817:AAFO5-BBhxaHQp9QNg7WbtRHLWy9f0SuU00");
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
            else if (callbackQuery.Data.StartsWith(diet))
            {
                await AddDietAsync(callbackQuery.Data.Substring(diet.Length), callbackQuery.Message.Chat.Id);
            }
            else if (callbackQuery.Data == "no_diet")
            {
                await AddDietAsync("", callbackQuery.Message.Chat.Id);
            }
        }
        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter /keyboard to get started");
                return;
            }
            else if (message.Text == "/inline")
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
            }
            else if (message.Text == "/keyboard")
            {
                ReplyKeyboardMarkup replyKeyboardMapkup = new
                    (
                    new[]
                        {
                            new KeyboardButton[] {"Favorites", "Clear favorites"},
                            new KeyboardButton[] {"Random", "Diet"}
                        }
                    )
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Select button or enter ingredients and cuisine:\nBritish\nItalian\nMexican\nDominican\nIndian\nThai\nEthiopian\nCuban\nBrazilian\nTaiwanese", replyMarkup: replyKeyboardMapkup);
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
            else if (message.Text == "Diet")
            {
                int i = 0;
                int j = 0;
                List<List<InlineKeyboardButton>> buttons = new();
                foreach (var k in diets)
                {
                    if (i == 0)
                    {
                        buttons.Add(new List<InlineKeyboardButton>());
                    }
                    buttons[j].Add(InlineKeyboardButton.WithCallbackData(k.Value, $"{diet}{k.Key}"));
                    i++;

                    if (i == 3)
                    {
                        j++;
                        i = 0;
                    }
                }
                buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData("Delete", $"no_diet") });
                InlineKeyboardMarkup keyboardMarkup = new InlineKeyboardMarkup(buttons);
                await botClient.SendTextMessageAsync(message.Chat.Id, "Select diet", replyMarkup: keyboardMarkup);
            }
            else
            {
                await RecipeListAsync(message);
            }
        }
        public async Task Conclusion(string name, long messageChatId)
        {
            var result = foodClient.GetFoodRecipeAsync(name, messageChatId.ToString()).Result;
            if (result == null || result.Count == 0)
                await botClient.SendTextMessageAsync(messageChatId, "Nothing found");
            else
            {
                var res = result[0];
                for (int i = 0; i < result.Count; i++)
                {
                    if (result[i].name == name)
                    {
                        res = result[i];
                        break;
                    }
                } 
                string dishRecipe = "";
                if (res.instructions!=null && res.instructions.Count!=0 && res.description != null)
                { 
                    string instruction = res.instructions.Aggregate("", (string result, Model.InstructionItem instr) => String.Concat(result, instr.position.ToString() + ". ", instr.display_text + "\n"));
                    dishRecipe = $"{res.name}\n{res.description}\n{instruction}";
                }
                else if(res.instructions != null && res.instructions.Count != 0 && res.description == null)
                {
                    string instruction = res.instructions.Aggregate("", (string result, Model.InstructionItem instr) => String.Concat(result, instr.position.ToString() + ". ", instr.display_text + "\n"));
                    dishRecipe = $"{res.name}\n{instruction}";
                }
                else if ((res.instructions == null || res.instructions.Count == 0) && res.description != null)
                {
                    dishRecipe = $"{res.name}\n{res.description}";
                }
                else
                {
                    dishRecipe = $"{res.name}";
                }
                var favResult = foodClient.GetFavoritesAsync(messageChatId.ToString()).Result;
                var favList = favResult.ConvertAll(recipe => recipe.Recipe).ToArray();
                InlineKeyboardMarkup keyboardMarkup;
                if (favList.Contains(name))
                {
                    if (result[0].original_video_url == null)
                    {
                        keyboardMarkup = new[] { new[] { InlineKeyboardButton.WithCallbackData("Delete from favorites", $"{Cut(deleteFromFavorite, result[0].name)}") } };
                    }
                    else
                    {
                        keyboardMarkup = new[] { new[] { InlineKeyboardButton.WithCallbackData("Delete from favorites", $"{Cut(deleteFromFavorite, result[0].name)}"),
                                                         InlineKeyboardButton.WithUrl("Watch the video", result[0].original_video_url.ToString())} };
                    }
                }
                else
                {
                    if (result[0].original_video_url == null)
                    {
                        keyboardMarkup = new[] { new[] { InlineKeyboardButton.WithCallbackData("Add to favorites", $"{Cut(addToFavorite, result[0].name)}") } };
                    }
                    else
                    {
                        keyboardMarkup = new[] { new[] { InlineKeyboardButton.WithCallbackData("Add to favorites", $"{Cut(addToFavorite, result[0].name)}"),
                                                         InlineKeyboardButton.WithUrl("Watch the video", result[0].original_video_url.ToString())} };
                    }
                }
                await botClient.SendTextMessageAsync(messageChatId, dishRecipe, replyMarkup: keyboardMarkup);
            }
        }
        public async Task RecipeListAsync(Message message)
        {
            var result = foodClient.GetFoodRecipeAsync(message.Text, message.Chat.Id.ToString()).Result;
            if (result == null || result.Count == 0)
                await botClient.SendTextMessageAsync(message.Chat.Id, "Nothing found");
            else
            {
                /*foreach (Model.Recipe recipe  in result)
                {
                    Console.WriteLine($"{recipe.title.Replace(':', '_').Replace('/', '_').Replace("'"[0], '_').Replace(',', '_').Replace('(', '_').Replace(')', '_').Replace('-', '_')}");
                }*/ 
                InlineKeyboardMarkup keyboardMarkup = result.ConvertAll(recipe => new[] { InlineKeyboardButton.WithCallbackData(recipe.name, $"{Cut(getRecipe, recipe.name)}") }).ToArray();
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
        public async Task AddDietAsync(string diet, long messageChatId)
        {
            await foodClient.PostDietAsync(diet, messageChatId.ToString());
            if (diet == "")
            {
                await botClient.SendTextMessageAsync(messageChatId, $"Diet was deleted");
            }
            else
                await botClient.SendTextMessageAsync(messageChatId, $"{diets[diet]} was added");
        }
    }
}
