using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Model
{
        public class Recipe
        {
            public string title { get; set; }
            public string ingredients { get; set; }
            public string servings { get; set; }
            public string instructions { get; set; }
        }
}
