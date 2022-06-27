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
    class YtClient
    {
        private HttpClient _client;
        private static string _address;
        private static string _apikey;
        public YtClient()
        {
            _address = Constants.address;
            _apikey = Constants.apikey;
            _client = new HttpClient();
            _client.BaseAddress = new Uri(_address);
            _client.DefaultRequestHeaders.Add("X-RapidAPI-Host", _address.Substring(8));
            _client.DefaultRequestHeaders.Add("X-RapidAPI-Key", _apikey);
        }
        public async Task<Items> GetVideoAsync(string name, string messageChatId)
        {
            var responce = await _client.GetAsync($"Yt/GetVideo?name={name}&messageChatId={messageChatId}");
            var content = responce.Content.ReadAsStringAsync().Result;
            if (content != null && content.Length != 0)
            {
                var result = JsonConvert.DeserializeObject<Items>(content);
                return result;
            }
            else
                return null;
        }
    }
}
