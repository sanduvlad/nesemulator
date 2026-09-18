namespace NesEmulator.Hardware;

using cycles = int;

public class Cpu
{
    public static Registers Registers { get; private set; }
    private static readonly Bus _bus = new Bus();
    
    public enum AddressingMode
    {
        Imd,
        ZP,
        ZX,
        ZY,
        Abs,
        AX,
        AY,
        IndX,
        IndY
    }

    public enum Instruction
    {
        ADC,AND,ASL,BCC,BCS,BEQ,BIT,BMI,BNE,BPL,BRK,BVC,BVS,CLC,
        CLD,CLI,CLV,CMP,CPX,CPY,DEC,DEX,DEY,EOR,INC,INX,INY,JMP,
        JSR,LDA,LDX,LDY,LSR,NOP,ORA,PHA,PHP,PLA,PLP,ROL,ROR,RTI,
        RTS,SBC,SEC,SED,SEI,STA,STX,STY,TAX,TAY,TSX,TXA,TXS,TYA,
    }

    private Dictionary<byte, Func<cycles>> _opCodeToFunction = new  Dictionary<byte, Func<cycles>>
    {
        { 0x69, () => ADC(0x69, AddressingMode.Imd, 2, false) },
        { 0x65, () => ADC(0x65, AddressingMode.ZP, 3, false) },
        { 0x75, () => ADC(0x75, AddressingMode.ZX, 4, false) },
        { 0x6D, () => ADC(0x6D, AddressingMode.Abs, 4, false) },
        { 0x7D, () => ADC(0x7D, AddressingMode.AX,4, true) },
        { 0x79, () => ADC(0x79, AddressingMode.AY, 4, true) },
        { 0x61, () => ADC(0x61, AddressingMode.IndX, 6, false) },
        { 0x71, () => ADC(0x71, AddressingMode.IndY, 6, true) },
        //
        
        
    };
    
    public cycles ExecuteInstruction(byte opcode)
    {
        return _opCodeToFunction[opcode]();
    }

    private static ushort Fetch(AddressingMode mode, bool shouldCountExtraCycle, out cycles extraCycle)
    {
        extraCycle = 0;
        var PC = Registers.PC;
        var X = Registers.X;
        var Y = Registers.Y;
        var ReadByte = _bus.ReadByte;
        

        switch (mode)
        {
            // case AddressingMode.Implied:
            // case AddressingMode.Accumulator:
            //     return 0;
            case AddressingMode.Imd:
            {
                ushort address = PC;
                PC++;

                return address;
            }

            case AddressingMode.ZP:
            {
                byte address = ReadByte(PC);
                PC++;

                return address;
            }

            case AddressingMode.ZX:
            {
                byte address = ReadByte(PC);
                PC++;

                return (byte)(address + X);
            }

            case AddressingMode.ZY:
            {
                byte address = ReadByte(PC);
                PC++;

                return (byte)(address + Y);
            }

            case AddressingMode.Abs:
            {
                byte low = ReadByte(PC);
                byte high = ReadByte((ushort)(PC + 1));

                PC += 2;

                return (ushort)(low | (high << 8));
            }

            case AddressingMode.AX:
            {
                byte low = ReadByte(PC);
                byte high = ReadByte((ushort)(PC + 1));

                PC += 2;

                ushort baseAddress = (ushort)(low | (high << 8));
                ushort address = (ushort)(baseAddress + X);

                // Page crossed?
                if ((baseAddress & 0xFF00) != (address & 0xFF00))
                    extraCycle = 1;

                return address;
            }

            case AddressingMode.AY:
            {
                byte low = ReadByte(PC);
                byte high = ReadByte((ushort)(PC + 1));

                PC += 2;

                ushort baseAddress = (ushort)(low | (high << 8));
                ushort address = (ushort)(baseAddress + Y);

                // Page crossed?
                if ((baseAddress & 0xFF00) != (address & 0xFF00))
                    extraCycle = 1;

                return address;
            }

            // case AddressingMode.Indirect:
            // {
            //     byte low = ReadByte(PC);
            //     byte high = ReadByte((ushort)(PC + 1));
            //
            //     PC += 2;
            //
            //     ushort pointer = (ushort)(low | (high << 8));
            //
            //     // 6502 JMP indirect hardware bug:
            //     // JMP ($12FF) reads the high byte from $1200.
            //     ushort highAddress =
            //         (ushort)((pointer & 0xFF00) |
            //                  ((pointer + 1) & 0x00FF));
            //
            //     byte targetLow = ReadByte(pointer);
            //     byte targetHigh = ReadByte(highAddress);
            //
            //     return (ushort)(targetLow | (targetHigh << 8));
            // }

            // case AddressingMode.IndexedIndirect:
            // {
            //     // ($12,X)
            //
            //     byte zeroPageAddress = ReadByte(PC);
            //     PC++;
            //
            //     byte pointer = (byte)(zeroPageAddress + X);
            //
            //     byte low = ReadByte(pointer);
            //     byte high = ReadByte((byte)(pointer + 1));
            //
            //     return (ushort)(low | (high << 8));
            // }
            //
            // case AddressingMode.IndirectIndexed:
            // {
            //     // ($12),Y
            //
            //     byte pointer = ReadByte(PC);
            //     PC++;
            //
            //     byte low = ReadByte(pointer);
            //     byte high = ReadByte((byte)(pointer + 1));
            //
            //     ushort baseAddress = (ushort)(low | (high << 8));
            //     ushort address = (ushort)(baseAddress + Y);
            //
            //     // Page crossed?
            //     if ((baseAddress & 0xFF00) != (address & 0xFF00))
            //         extraCycle = 1;
            //
            //     return address;
            // }
            //
            // case AddressingMode.Relative:
            // {
            //     sbyte offset = (sbyte)ReadByte(PC);
            //     PC++;
            //
            //     ushort address = (ushort)(PC + offset);
            //
            //     // IMPORTANT:
            //     // Branch instructions have different cycle rules.
            //     // The page-crossing penalty belongs to the branch
            //     // instruction itself, not simply to Relative addressing.
            //     return address;
            // }

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }

        extraCycle = shouldCountExtraCycle ? extraCycle : 0;
    }

    private static void Write(ushort address, byte value)
    {
        _bus.Write(address, value);
    }

    private static cycles ADC(byte opCode, AddressingMode addressingMode, cycles baseCycles, bool shouldCountExtraCycle)
    {
        var data = Fetch(addressingMode, shouldCountExtraCycle, out cycles extraCycle);
        
        // code
        
        return baseCycles + extraCycle;
    }
}

public struct Registers
{
    public byte A { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    public ushort PC { get; set; }
    public byte SP { get; set; }
    public byte P { get; set; }
}