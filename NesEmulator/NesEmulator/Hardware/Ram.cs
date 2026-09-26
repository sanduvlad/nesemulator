namespace NesEmulator.Hardware;


/*
 *
 *      ┌───────────────┐
                    │      CPU      │
                    │               │
                    │ PC = $8000    │
                    └───────┬───────┘
                            │
                     Read($8000)
                            │
                            ▼
                    ┌───────────────┐
                    │   NES BUS     │
                    └───────┬───────┘
                            │
             ┌──────────────┼──────────────┐
             ▼              ▼              ▼
          CPU RAM          PPU        Cartridge
       $0000-$1FFF    $2000-$3FFF    $4020-$FFFF
                                           │
                                           ▼
                                         PRG-ROM
 *
 * 
 */


public static class Ram
{
    public static byte[] RamMemory = new byte[0xFFFF + 1];

    private static Cartridge cassete = new Cartridge();

    public static void InitializeRam()
    {
        Array.Clear(RamMemory, 0, RamMemory.Length);

        cassete.LoadCartridge();

        byte[] cartridge = cassete.GetRomBytes;

        // iNES header
        int prgRomBanks = cartridge[4];

        // Each PRG bank is 16 KB.
        int prgRomSize = prgRomBanks * 0x4000;

        const int headerSize = 16;
        const int prgRomStart = headerSize;

        Console.WriteLine($"PRG-ROM size: {prgRomSize:X} bytes");

        // Map PRG-ROM at CPU address $8000.
        for (int i = 0; i < prgRomSize; i++)
        {
            RamMemory[0x8000 + i] =
                cartridge[prgRomStart + i];
        }

        // If PRG-ROM is 16 KB, mirror it at $C000.
        if (prgRomSize == 0x4000)
        {
            for (int i = 0; i < 0x4000; i++)
            {
                RamMemory[0xC000 + i] =
                    RamMemory[0x8000 + i];
            }
        }
    }
}


