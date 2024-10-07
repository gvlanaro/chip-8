
using System.Collections;

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
    public Emulator(string rom_path) {
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

    public void Cycle() {
        // fetch 
        PC+=2;
        ushort OpCode = (ushort)((Memory[PC] << 8) | Memory[PC+1]);  // combines 2 bytes (instructions for chip8 are 16bits)
        System.Console.WriteLine("last code: " + OpCode);
        // decode and execute (switch case)
        byte fNibble = (byte)(OpCode >> 12);     // first nibble (half byte) (ho usato byte per salvare perchè non ci sono tipi minori)
        byte X = (byte)((OpCode & 0x0F00) >> 8); 
        byte Y = (byte)((OpCode & 0x00F0) >> 4); 
        byte N = (byte)(OpCode & 0x000F); 
        byte NN = (byte)(OpCode & 0x00FF); 
        ushort NNN = (ushort)(OpCode & 0x0FFF); 

        switch (fNibble)
        {
            case 0x0 when OpCode == 0x00E0:
                // clear screen
                Display = new byte [64, 32];
                break;
            case 0x1:
                // jump to address NNN
                PC = NNN;
                // remove the +2 since this is a jump
                PC-=2;
                break;
            case 0x6:
                // set register VX
                V[X] = NN;
                break;
            case 0x7:
                // add to VX
                V[X] += NN;
                break;
            case 0xA:
                // set I
                I = NNN;
                break;
            case 0xD:
                // display/draw
                Draw(X, Y, N);
                break;
            default:
                break;
        }
        
    }
    private void Draw(byte X, byte Y, byte N)
    {
        // collision detection
        V[0xF] = 0;

        for (int row = 0; row < N; row++)
        {
            var cY = (V[Y] + row) % 32;
            
            byte sprite_data = Memory[I + row];         // metodo alternativo: BitArray sprite_data = new BitArray(new byte[] { Memory[I + row] });
            int iX = 0;
            for (int pixel = 7; pixel >= 0; pixel--)
            {
                var cX = (V[X] + iX) % 64;
                iX++;

                //get most significant bit
                int shifted = sprite_data >> pixel;

                if ((shifted & 1)!= 0)
                {
                    if (Display[cX,cY] == 1)
                        V[0xF] = 1;
                    // Enable or disable the pixel (XOR operation).
                    Display[cX,cY] ^= 1;
                }
            }
            
        }
    }
}