using System.Collections;
using OpenTK.Core.Platform;

namespace Chip8
{
    public class Emulator
    {

        private byte[] Fonts =
        {
              0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
              0x20, 0x60, 0x20, 0x20, 0x70, // 1
              0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
              0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
              0x90, 0x90, 0xF0, 0x10, 0x10, // 4
              0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
              0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
              0xF0, 0x10, 0x20, 0x40, 0x40, // 7
              0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
              0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
              0xF0, 0x90, 0xF0, 0x90, 0x90, // A
              0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
              0xF0, 0x80, 0x80, 0x80, 0xF0, // C
              0xE0, 0x90, 0x90, 0x90, 0xE0, // D
              0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
              0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        public byte[] Memory { get; set; }
        public byte[,] Display { get; set; } 
        public ushort PC { get; set; }
        public ushort I { get; set; }
        public Stack<ushort> Stack { get; set; }
        public byte Delay_Timer { get; set; }
        public byte Sound_timer { get; set; }
        public byte[] V { get; set; }
        public ushort OpCode { get; set; }

        const ushort RomStart = 0x200;

        public Emulator(Window window, string rom_path) {

            Memory = new byte[4096];
            Display = new byte[64, 32];
            PC = RomStart;
            I = 0;
            Stack = new Stack<ushort>();
            Delay_Timer = 0;
            Sound_timer = 0;
            V = new byte[16];

            Fonts.CopyTo(Memory, 0x00);
            byte[] rom = File.ReadAllBytes(rom_path);
            rom.CopyTo(Memory, RomStart);

        }

        public void Run() {
            // fetch 
            ushort OpCode = (ushort)((Memory[PC] << 8) | Memory[PC+1]);   // combines 2 bytes (instructions for chip8 are 16bits)
            PC+=2;

            // decode
            
            byte fNibble = (byte)(OpCode >> 12);     // first nibble (half byte) (ho usato byte per salvare perchè non ci sono tipi minori)
            byte X = (byte)((OpCode & 0x0F00) >> 8); 
            byte Y = (byte)((OpCode & 0x00F0) >> 4); 
            byte N = (byte)(OpCode & 0x000F); 
            byte NN = (byte)(OpCode & 0x00FF); 
            ushort NNN = (ushort)(OpCode & 0x0FFF); 
            
        }   
    }
}