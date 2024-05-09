using System.Net;

static class Program 
{
    static void Main(string [] args) 
    {
        Emulator emulator = new Emulator("roms/ibm_logo.ch8");
        using Window window = new (640, 320, "chip8", emulator);

        window.Run();
    }
} 