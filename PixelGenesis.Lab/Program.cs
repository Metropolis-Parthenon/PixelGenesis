// See https://aka.ms/new-console-template for more information
using PixelGenesis.Lab;

Console.WriteLine("Hello, World!");


using (Game game = new Game(800, 600, "LearnOpenTK"))
{
    game.Run();
}