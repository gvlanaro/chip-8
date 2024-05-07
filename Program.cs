namespace Chip8
{
    static class Program 
    {
        static void Main(string [] args) 
        {
            using Game game = new (800, 600, "LearnOpenTK");
            game.Run();
        }
    } 
} 