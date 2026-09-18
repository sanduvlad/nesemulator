namespace NesEmulator.Hardware;

public class Bus
{
    public byte ReadByte(ushort address)
    {
        return Ram.RamMemory[address];
    }

    public void Write(ushort address, byte value)
    {
        Ram.RamMemory[address] = value;
    }
}