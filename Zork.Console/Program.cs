using System;
using System.IO;
using Newtonsoft.Json;
using Zork.Common;

namespace Zork
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string defaultGameFilename = "Zork.json";
            string gameFilename = (args.Length > 0 ? args[(int)CommandLineArguments.GameFilename] : defaultGameFilename);

            Game game = Game.Load(gameFilename);
            ConsoleInputService input = new ConsoleInputService();
            ConsoleOutputService output = new ConsoleOutputService();

            game.Player.LocationChanged += PlayerLocationChanged;
            game.Player.MovesChanged += PlayerMovesChanged;
            game.Player.ScoreChanged += PlayerScoreChanged;


            game.Start(input, output);
            game.Player.Score = 0;
            Room previousRoom = null;
            while (game.IsRunning)
            {
                output.WriteLine(game.Player.Location);
                if (previousRoom != game.Player.Location)
                {
                    Game.Look(new CommandContext(game,"Look"));
                    previousRoom = game.Player.Location;
                }

                output.Write("\n> ");
                input.ProcessInput();
            }

            output.WriteLine(string.IsNullOrWhiteSpace(game.ExitMessage) ? "Thank you for playing!" : game.ExitMessage);
        }
        private enum CommandLineArguments
        {
            GameFilename = 0
        }

        private static void PlayerScoreChanged(object sender, int e)
        {
            System.Console.WriteLine($"Incredible! Your score has increased to {e}");
        }

        private static void PlayerMovesChanged(object sender, int e)
        {
            
        }

        private static void PlayerLocationChanged(object sender, Room e)
        {
            System.Console.WriteLine($"You moved to {e.Name}");
        }
    }
}