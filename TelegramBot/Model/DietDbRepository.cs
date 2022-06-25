using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Model
{
    class DietDbRepository
    {
        public DietDbRepository(string messageChatId, string diet)
        {
            MessageChatId = messageChatId;
            Diet = diet;
        }
        public string MessageChatId { get; set; }
        public string Diet { get; set; }
    }
}
