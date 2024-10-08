
static class Program 
{
    static void Main(string [] args) 
    {
        using (Window window = new Window(640, 320, "chip-8")){
            window.UpdateFrequency = 600;
            window.Run();
        }
    }
} 