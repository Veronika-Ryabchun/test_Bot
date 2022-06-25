using System;
using TelegramBot;

namespace TelegramBot
{
    class Program
    {
        static void Main(string[] args)
        {
            RecipeBot recipeBot = new RecipeBot();
            recipeBot.Start();
            while (true)
            {
                System.Threading.Thread.Sleep(1000*3600*24);
            }
        }
    }
}
