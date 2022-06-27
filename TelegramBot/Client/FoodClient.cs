using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBot.Const;
using TelegramBot.Model;

namespace TelegramBot.Client
{
    class FoodClient
    {
        private HttpClient _client;
        private static string _address;
        private static string _apikey;
        public FoodClient()
        {
            _address = Constants.address;
            _apikey = Constants.apikey;
            _client = new HttpClient();
            _client.BaseAddress = new Uri(_address);
            _client.DefaultRequestHeaders.Add("X-RapidAPI-Host", _address.Substring(8));
            _client.DefaultRequestHeaders.Add("X-RapidAPI-Key", _apikey);
        }
        public async Task<List<ResultItem>> GetFoodRecipeAsync(string recipe, string messageChatId, bool ignoreDiet)
        {
            var responce = await _client.GetAsync($"Recipe/GetAll?Recipe={recipe}&MessageChatId={messageChatId}&ignoreDiet={ignoreDiet}");
            var content = responce.Content.ReadAsStringAsync().Result;
            if (content != null && content.Length != 0)
            {
                var result = JsonConvert.DeserializeObject<List<ResultItem>>(content);
                return result;
            }
            else
                return null;
        }
        public async Task PostFavoriteRecipeAsync(string recipe, string messageChatId)
        {
            RecipeDbRepository recipeDbRepository = new(messageChatId, recipe);
            var json = JsonConvert.SerializeObject(recipeDbRepository);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("Recipe/AddtoFavorites", data);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);
        }
        public async Task<List<RecipeDbRepository>> GetFavoritesAsync(string messageChatId)
        {
            var responce = await _client.GetAsync($"Recipe/GetFavorites?MessageChatId={messageChatId}");
            var content = responce.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<List<RecipeDbRepository>>(content);
            return result;
        }
        public async Task DeleteFavoriteRecipeAsync(string recipe, string messageChatId)
        {
            RecipeDbRepository recipeDbRepository = new(messageChatId, recipe);
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = JsonContent.Create(recipeDbRepository),
                Method = HttpMethod.Delete,
                RequestUri = new Uri("Recipe/DeleteFromFavorites", UriKind.Relative)
            };
            var response = await _client.SendAsync(request);

            /*var json = JsonConvert.SerializeObject(recipeDbRepository);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.DeleteAsync("Recipe/DeleteFromFavorites", data);*/
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);
        }
        public async Task ClearFavoritesAsync(string messageChatId)
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = JsonContent.Create(messageChatId),
                Method = HttpMethod.Delete,
                RequestUri = new Uri("Recipe/ClearAllFavorites", UriKind.Relative)
            };
            var response = await _client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);
        }
        public async Task PostDietAsync(string diet, string messageChatId)
        {
            DietDbRepository dietDbRepository = new(messageChatId, diet);
            var json = JsonConvert.SerializeObject(dietDbRepository);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("Recipe/AddDiet", data);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);
        }
    }
}
