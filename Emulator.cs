using System.Collections;
using System.ComponentModel;

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
        PC+=2;
        ushort OpCode = (ushort)((Memory[PC] << 8) | Memory[PC+1]);  // combines 2 bytes (instructions for chip8 are 16bits)
        
        System.Console.WriteLine("last code: " + OpCode);

        byte fNibble = (byte)(OpCode >> 12);        // first nibble (4-bit value)
        byte x = (byte)((OpCode & 0x0F00) >> 8);    // A 4-bit value, the lower 4 bits of the high byte of the instruction 
        byte y = (byte)((OpCode & 0x00F0) >> 4);    // A 4-bit value, the upper 4 bits of the low byte of the instruction 
        byte n = (byte)(OpCode & 0x000F);           // A 4-bit value, the lowest 4 bits of the instruction
        byte kk = (byte)(OpCode & 0x00FF);          // An 8-bit value, the lowest 8 bits of the instruction 
        ushort nnn = (ushort)(OpCode & 0x0FFF);     // A 12-bit value, the lowest 12 bits of the instruction 

        switch (fNibble)
        {
            case 0x0 when OpCode == 0x00E0:
                I00E0();
                break;
            case 0x0 when OpCode == 0x00EE:
                I00EE();
                break;
            case 0x0:
                I0nnn();
                break;
            case 0x1:
                I1nnn(nnn);
                break;
            case 0x2:
                I2nnn(nnn);
                break;
            case 0x3:
                I3xkk(x, kk);
                break;
            case 0x4:
                I4xkk(x, kk);
                break;
            case 0x5:
                I5xy0(x, y);
                break;          
            case 0x6:
                I6xkk(x, kk);
                break;
            case 0x7:
                I7xkk(x, kk);
                break;
            case 0xA:
                // set I
                I = nnn;
                break;
            case 0xD:
                // display/draw
                Draw(x, y, n);
                break;
            default:
                break;
        }
        
    }

    private void I7xkk(byte x, byte kk)
    {
        // Set Vx = Vx + kk.
        // Adds the value kk to the value of register Vx, then stores the result in Vx. 
        V[x] += kk;
    }

    private void I6xkk(byte x, byte kk)
    {
        // Set Vx = kk.
        // The interpreter puts the value kk into register Vx.
        V[x] = kk;
    }

    private void I5xy0(byte x, byte y)
    {
        // Skip next instruction if Vx = Vy.
        // The interpreter compares register Vx to register Vy, and if they are equal, increments the program counter by 2.
        if (V[x] == V[y])
        {
            PC += 2;
        }
    }

    private void I4xkk(byte x, byte kk)
    {
        // Skip next instruction if Vx != kk.
        // The interpreter compares register Vx to kk, and if they are not equal, increments the program counter by 2.
        if (V[x] != kk)
        {
            PC += 2;
        }
    }

    private void I3xkk(byte x, byte kk)
    {
        // Skip next instruction if Vx = kk.
        // The interpreter compares register Vx to kk, and if they are equal, increments the program counter by 2.
        if (V[x] == kk)
        {
            PC += 2;
        }
    }

    private void I2nnn(ushort nnn)
    {
        // Call subroutine at nnn.
        // The interpreter increments the stack pointer, then puts the current PC on the top of the stack. The PC is then set to nnn.
        Stack.Pop();
        Stack.Push(PC);
        PC = nnn;
    }

    private void I1nnn(ushort nnn)
    {
        // Jump to location nnn.
        // The interpreter sets the program counter to nnn.
        PC = nnn;
        // this is to ignore the next +2
        PC-=2;
    }

    private void I0nnn()
    {
        // Jump to a machine code routine at nnn.
        // This instruction is only used on the old computers on which Chip-8 was originally implemented. It is ignored by modern interpreters.
    }

    private void I00EE()
    {
        // Return from a subroutine.
        // The interpreter sets the program counter to the address at the top of the stack, then subtracts 1 from the stack pointer.
        PC = Stack.Pop();
    }

    private void I00E0()
    {
        // Clear the display.
        Display = new byte [64, 32];
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