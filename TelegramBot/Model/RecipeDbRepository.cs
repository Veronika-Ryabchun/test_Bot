using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Model
{
    public class RecipeDbRepository
    {
        public RecipeDbRepository(string messageChatId, string recipe)
        {
            MessageChatId = messageChatId;
            Recipe = recipe;
        }

        public string MessageChatId { get; set; }
        public string Recipe { get; set; }
    }
}
