namespace NesEmulator.Hardware;

using cycles = int;

/*
 *
// SET a flag
P |= mask;

// CLEAR a flag
P &= (byte)~mask;

// CHECK a flag
bool set = (P & mask) != 0;
 */

public partial class Cpu
{
    private static void SetZeroAndNegative(byte value)
    {
        if (value == 0)
            Registers.P |= StatusFlags.Zero;
        else
            Registers.P &= ~StatusFlags.Zero;

        if ((value & 0x80) != 0)
            Registers.P |= StatusFlags.Negative;
        else
            Registers.P &= ~StatusFlags.Negative;
    }
    
    private static cycles ADC(byte opCode, AddressingMode addressingMode, cycles baseCycles, bool shouldCountExtraCycle)
    {
        var data = Fetch(addressingMode, shouldCountExtraCycle, out cycles extraCycle);
        
        // code
        
        return baseCycles + extraCycle;
    }

    public static cycles SEI(byte opCode, AddressingMode addressingMode, cycles baseCycles, bool shouldCountExtraCycle)
    {
        Fetch(addressingMode, shouldCountExtraCycle, out var _);
        Registers.P |= StatusFlags.InterruptDisable;

        return baseCycles;
    }
    
    public static cycles CLD(byte opCode, AddressingMode addressingMode, cycles baseCycles, bool shouldCountExtraCycle)
    {
        Fetch(addressingMode, shouldCountExtraCycle, out var _);
        Registers.P &= ~StatusFlags.Decimal;
        
        return baseCycles;
    }

    public static cycles LDA(byte opCode, AddressingMode addressingMode, cycles baseCycles, bool shouldCountExtraCycle)
    {
        ushort address = Fetch(
            addressingMode,
            shouldCountExtraCycle,
            out cycles extraCycle);

        Registers.A = _bus.ReadByte(address);

        SetZeroAndNegative(Registers.A);

        return baseCycles + (shouldCountExtraCycle ? extraCycle : 0);
    }

    public static cycles BPL(byte opCode, AddressingMode addressingMode, cycles baseCycles, bool shouldCountExtraCycle)
    {
        ushort targetAddress = Fetch(addressingMode, shouldCountExtraCycle, out cycles extraCycle);

        if ((Registers.P & StatusFlags.Negative) == 0)
        {
            // Branch page-crossing penalty: compare PC's page (after
            // the branch instruction bytes) to the target's page.
            bool pageCrossed = (Registers.PC & 0xFF00) != (targetAddress & 0xFF00);

            Registers.PC = targetAddress;

            return baseCycles + 1 + (pageCrossed ? 1 : 0);
        }

        return baseCycles;
    }
    
    public static cycles ORA(byte opCode, AddressingMode addressingMode, cycles baseCycles, bool shouldCountExtraCycle)
    {
        // Fetch
        var address = Fetch(addressingMode, shouldCountExtraCycle, out var extraCycle);
        
        // Operation
        var a = Registers.A;
        var d = _bus.ReadByte(address);

        Registers.A = (byte)(a | d);
        SetZeroAndNegative(Registers.A);
        
        // Return
        return baseCycles + (shouldCountExtraCycle ? extraCycle : 0);
    }
    
    public static cycles BRK(byte opCode, AddressingMode addressingMode, cycles baseCycles, bool shouldCountExtraCycle)
    {
        // BRK is 2 bytes: opcode + a padding byte that's read but discarded.
        Registers.PC++;

        // Push return address (PC now points past the padding byte).
        byte pcHigh = (byte)(Registers.PC >> 8);
        byte pcLow  = (byte)(Registers.PC & 0xFF);
        Push(pcHigh);
        Push(pcLow);

        // Push status with B flag and unused bit set (BRK/PHP-specific).
        byte statusToPush = (byte)(Registers.P | StatusFlags.Break | StatusFlags.Unused);
        Push(statusToPush);

        // Set interrupt disable.
        Registers.P |= StatusFlags.InterruptDisable;

        // Jump to IRQ/BRK vector.
        byte vectorLow  = _bus.ReadByte(0xFFFE);
        byte vectorHigh = _bus.ReadByte(0xFFFF);
        Registers.PC = (ushort)(vectorLow | (vectorHigh << 8));

        return baseCycles;
    }

    public static cycles DummyOpCode(byte opCode, AddressingMode addressingMode, cycles baseCycles, bool shouldCountExtraCycle)
    {
        // Fetch
        var data = Fetch(addressingMode, shouldCountExtraCycle, out var extraCycle);
        
        // Operation
        
        // Return
        return baseCycles + (shouldCountExtraCycle ? extraCycle : 0);
    }

}