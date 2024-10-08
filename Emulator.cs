using System.Collections;
using System.ComponentModel;
using Microsoft.VisualBasic;
using OpenTK.Windowing.Common;

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
    public bool[] Keys;
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
    private uint TimeCounter;
    public Emulator(string rom_path) {
        TimeCounter = 0;
        Keys = new bool[16];

        Memory = new byte[4096];
        Display = new byte[64, 32];
        PC = RomStart;
        I = 0;
        Stack = new Stack<ushort>(new ushort[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        Delay_Timer = 0;
        Sound_timer = 0;
        V = new byte[16];

        Fonts.CopyTo(Memory, 0x00);
        byte[] rom = File.ReadAllBytes(rom_path);
        rom.CopyTo(Memory, RomStart);
    }

    public void Cycle() {
        PC += 2;
        ushort OpCode = (ushort)((Memory[PC] << 8) | Memory[PC+1]);  // combines 2 bytes (instructions for chip8 are 16bits)

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
            case 0x8 when n == 0:
                I8xy0(x, y);
                break;
            case 0x8 when n == 1:
                I8xy1(x, y);
                break;
            case 0x8 when n == 2:
                I8xy2(x, y);
                break;
            case 0x8 when n == 3:
                I8xy3(x, y);
                break;
            case 0x8 when n == 4:
                I8xy4(x, y);
                break;
            case 0x8 when n == 5:
                I8xy5(x, y);
                break;
            case 0x8 when n == 6:
                I8xy6(x, y);
                break;
            case 0x8 when n == 7:
                I8xy7(x, y);
                break;
            case 0x8 when n == 0xE:
                I8xyE(x, y);
                break;
            case 0x9:
                I9xy0(x, y);
                break;
            case 0xA:
                IAnnn(nnn);
                break;
            case 0xB:
                IBnnn(nnn);
                break;
            case 0xC:
                ICxkk(x, kk);
                break;
            case 0xD:
                IDxyn(x, y, n);
                break;
            case 0xE when kk == 0x9E:
                IEx9E(x);
                break;
            case 0xE when kk == 0xA1:
                IExA1(x);
                break;
            case 0xF when kk == 0x07:
                IFx07(x);
                break;
            case 0xF when kk == 0x0A:
                IFx0A(x);
                break;
            case 0xF when kk == 0x15:
                IFx15(x);
                break;
            case 0xF when kk == 0x18:
                IFx18(x);
                break;
            case 0xF when kk == 0x1E:
                IFx1E(x);
                break;
            case 0xF when kk == 0x29:
                IFx29(x);
                break;
            case 0xF when kk == 0x33:
                IFx33(x);
                break;
            case 0xF when kk == 0x55:
                IFx55(x);
                break;
            case 0xF when kk == 0x65:
                IFx65(x);
                break;
            default:
                break;
        }

        UpdateTimers();
    }

    private void UpdateTimers()
    {
        if (TimeCounter % 10 == 0)
        {
            if (Delay_Timer > 0)
            {
                Delay_Timer--;
            }
            if (Sound_timer > 0)
            {   
                Sound_timer--;
            }
        }
        TimeCounter++;
    }

    private void IFx65(byte x)
    {
        // Read registers V0 through Vx from memory starting at location I.
        // The interpreter reads values from memory starting at location I into registers V0 through Vx.
        for (int i = 0; i <= x; i++)
        {
            V[i] = Memory[I+i];
        }
    }

    private void IFx55(byte x)
    {
        // Store registers V0 through Vx in memory starting at location I.
        // The interpreter copies the values of registers V0 through Vx into memory, starting at the address in I.
        for (int i = 0; i <= x; i++)
        {
            Memory[I+i] = V[i];
        }
    }

    private void IFx33(byte x)
    {
        // Store BCD representation of Vx in memory locations I, I+1, and I+2.
        // The interpreter takes the decimal value of Vx, and places the hundreds digit in memory at location in I, the tens digit at location I+1, and the ones digit at location I+2.
        Memory[I+2] = (byte)(V[x] % 10);               // ones
        Memory[I+1] = (byte)((V[x] / 10) % 10);        // tens
        Memory[I] = (byte)((V[x] / 100) % 10);       // hundreds
    }

    private void IFx29(byte x)
    {
        // Set I = location of sprite for digit Vx.
        // The value of I is set to the location for the hexadecimal sprite corresponding to the value of Vx. See section 2.4, Display, for more information on the Chip-8 hexadecimal font.
        I = (byte)(V[x] * 5);       // every digit sprite is 5 bytes long
    }

    private void IFx1E(byte x)
    {
        // Set I = I + Vx.
        // The values of I and Vx are added, and the results are stored in I.
        I += V[x];
    }

    private void IFx18(byte x)
    {
        // Set sound timer = Vx.
        // ST is set equal to the value of Vx.
        Sound_timer = V[x];
    }

    private void IFx15(byte x)
    {
        // Set delay timer = Vx.
        // DT is set equal to the value of Vx.
        Delay_Timer = V[x];
    }

    private void IFx0A(byte x)
    {
        // Wait for a key press, store the value of the key in Vx.
        // All execution stops until a key is pressed, then the value of that key is stored in Vx.
        for (int i = 0; i < Keys.Length; i++)
        {
            if (Keys[i])
            {
                V[x] = (byte)i;
                return;
            }
        }

        // no button pressed, loop until one is
        PC -= 2;
    }

    private void IFx07(byte x)
    {
        // Set Vx = delay timer value.
        // The value of DT is placed into Vx.
        V[x] = Delay_Timer;
    }

    private void IEx9E(byte x)
    {
        // Skip next instruction if key with the value of Vx is pressed.
        // Checks the keyboard, and if the key corresponding to the value of Vx is currently in the down position, PC is increased by 2.
        if(Keys[V[x]])
        {
            PC +=2;
        }
    }

    private void IExA1(byte x)
    {
        // Skip next instruction if key with the value of Vx is not pressed.
        // Checks the keyboard, and if the key corresponding to the value of Vx is currently in the up position, PC is increased by 2.
        if(!Keys[V[x]])
        {
            PC +=2;
        }
    }

    private void ICxkk(byte x, byte kk)
    {
        // Set Vx = random byte AND kk.
        // The interpreter generates a random number from 0 to 255, which is then ANDed with the value kk. The results are stored in Vx. See instruction 8xy2 for more information on AND.

        Random rnd = new Random();
        V[x] = (byte)(rnd.Next(0, 256) & kk);
    }

    private void IBnnn(ushort nnn)
    {
        // Jump to location nnn + V0.
        // The program counter is set to nnn plus the value of V0.
        PC = (ushort)(nnn + V[0] - 2);
    }

    private void IAnnn(ushort nnn)
    {
        // Set I = nnn.
        // The value of register I is set to nnn.
        I = nnn;
    }

    private void I9xy0(byte x, byte y)
    {
        // Skip next instruction if Vx != Vy.
        // The values of Vx and Vy are compared, and if they are not equal, the program counter is increased by 2.
        if (V[x] != V[y])
        {
            PC += 2;
        }
    }

    private void I8xyE(byte x, byte y)
    {
        // Set Vx = Vx SHL 1.
        // If the most-significant bit of Vx is 1, then VF is set to 1, otherwise to 0. Then Vx is multiplied by 2
        byte temp = V[x];
        int tempF = temp >> 7;
        temp <<= 1;
        V[x] = temp;
        V[0xF] = (byte)tempF;
    }

    private void I8xy7(byte x, byte y)
    {
        // Set Vx = Vy - Vx, set VF = NOT borrow.
        // If Vy > Vx, then VF is set to 1, otherwise 0. Then Vx is subtracted from Vy, and the results stored in Vx.
        byte temp = (byte)(V[y] - V[x]);
        byte tempF = 0;
        if (V[y] >= V[x])
            tempF = 1;
            
        V[x] = temp;
        V[0xF] = tempF;
    }

    private void I8xy6(byte x, byte y)
    {
        // Set Vx = Vx SHR 1.
        // If the least-significant bit of Vx is 1, then VF is set to 1, otherwise 0. Then Vx is divided by 2.
        byte temp = V[x];
        byte tempF = 0;
        if ((V[x] & 1) == 1)
            tempF = 1;

        temp >>= 1;
        V[x] = temp;
        V[0xF] = tempF;
    }

    private void I8xy5(byte x, byte y)
    {
        // Set Vx = Vx - Vy, set VF = NOT borrow.
        // If Vx > Vy, then VF is set to 1, otherwise 0. Then Vy is subtracted from Vx, and the results stored in Vx.
        byte temp = (byte)(V[x] - V[y]);
        byte tempF = 0;
        if (V[x] >= V[y])
            tempF = 1;

        V[x] = temp;
        V[0xF] = tempF;
    }

    private void I8xy4(byte x, byte y)
    {
        // Set Vx = Vx + Vy, set VF = carry.
        // The values of Vx and Vy are added together. If the result is greater than 8 bits (i.e., > 255,) VF is set to 1, otherwise 0. Only the lowest 8 bits of the result are kept, and stored in Vx.
        ushort temp = (ushort)(V[x] + V[y]);
        byte tempF = 0;
        if (temp > 255)
            tempF = 1;
        V[x] = (byte)(temp & 255);
        V[0xF] = tempF;
            
    }

    private void I8xy3(byte x, byte y)
    {
        // Set Vx = Vx XOR Vy.
        // Performs a bitwise exclusive OR on the values of Vx and Vy, then stores the result in Vx. An exclusive OR compares the corrseponding bits from two values, and if the bits are not both the same, then the corresponding bit in the result is set to 1. Otherwise, it is 0. 
        V[x] = (byte)(V[x] ^ V[y]);
    }

    private void I8xy2(byte x, byte y)
    {
        // Set Vx = Vx AND Vy.
        // Performs a bitwise AND on the values of Vx and Vy, then stores the result in Vx. A bitwise AND compares the corrseponding bits from two values, and if both bits are 1, then the same bit in the result is also 1. Otherwise, it is 0. 
        V[x] = (byte)(V[x] & V[y]);
    }

    private void I8xy1(byte x, byte y)
    {
        // Set Vx = Vx OR Vy.
        // Performs a bitwise OR on the values of Vx and Vy, then stores the result in Vx. A bitwise OR compares the corrseponding bits from two values, and if either bit is 1, then the same bit in the result is also 1. Otherwise, it is 0.
        V[x] = (byte)(V[x] | V[y]);
    }

    private void I8xy0(byte x, byte y)
    {
        // Set Vx = Vy.
        // Stores the value of register Vy in register Vx.
        V[x] = V[y];
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
        Stack.Push(PC);
        PC = (ushort)(nnn - 2);
    }

    private void I1nnn(ushort nnn)
    {
        // Jump to location nnn.
        // The interpreter sets the program counter to nnn.
        PC = (ushort)(nnn - 2);
        // this is to ignore the next +2
        //PC-=2;
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

    private void IDxyn(byte X, byte Y, byte N)
    {
        // Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
        // The interpreter reads n bytes from memory, starting at the address stored in I. 
        // These bytes are then displayed as sprites on screen at coordinates (Vx, Vy). 
        // Sprites are XORed onto the existing screen. If this causes any pixels to be erased, VF is set to 1, otherwise it is set to 0. 
        // If the sprite is positioned so part of it is outside the coordinates of the display, it wraps around to the opposite side of the screen. 
        // See instruction 8xy3 for more information on XOR, and section 2.4, Display, for more information on the Chip-8 screen and sprites.
        V[0xF] = 0;

        for (int row = 0; row < N; row++)
        {
            var cY = (V[Y] + row) % 32;
            
            byte sprite_data = Memory[I + row];         // alternative: BitArray sprite_data = new BitArray(new byte[] { Memory[I + row] });
            int iX = 0;
            for (int pixel = 7; pixel >= 0; pixel--)
            {
                var cX = (V[X] + iX) % 64;
                iX++;

                int shifted = sprite_data >> pixel;     // get most significant bit

                if ((shifted & 1)!= 0)
                {
                    if (Display[cX,cY] == 1)
                        V[0xF] = 1;
                    
                    Display[cX,cY] ^= 1;                // enable or disable the pixel (XOR operation).
                }
            }
            
        }
    }
}