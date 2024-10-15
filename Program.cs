static class Program
{
    static void Main(string[] args)
    {
        using (Window window = new Window(640, 320, "chip-8", "/home/glanaro/Games/roms/Chip8_roms/test5.ch8"))
        {
            window.UpdateFrequency = 60;
            window.Run();
        }
    }
}