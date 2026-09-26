namespace NesEmulator.Hardware;

public class Cartridge
{
    private string _cartridgeFile = string.Empty;
    private byte[] _cartridgeRom;


    public void LoadCartridge()
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = ".NES File | *.nes";

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            _cartridgeFile = ofd.FileName;
            _cartridgeRom = File.ReadAllBytes(ofd.FileName);
        }
        
        ofd.Dispose();
    }

    public byte[] GetRomBytes => _cartridgeRom;
}