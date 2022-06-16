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
            Console.ReadKey();
        }
    }
}
