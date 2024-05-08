namespace Chip8
{
    static class Program 
    {
        static void Main(string [] args) 
        {
            using Window window = new (640, 320, "chip8");
            //window.Run();
            Emulator emulator = new Emulator(window, "roms/ibm_logo.ch8");
            emulator.Run();
        }
    } 
} 