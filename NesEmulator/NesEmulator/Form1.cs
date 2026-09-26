using System.Runtime.InteropServices;
using System.Text;
using NesEmulator.Hardware;

namespace NesEmulator;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }
    
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    private async void Form1_Load(object sender, EventArgs e)
    {
        AllocConsole();
        
        Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        var color = Color.FromArgb(0,0,0);
        for (int x = 0; x < bmp.Width; x++)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                bmp.SetPixel(x,y, color);
            }
        }
        
        pictureBox1.Image = bmp;

        Cpu cpu = new Cpu();
        cpu.ResetCpu();

        while (true)
        {
            await Task.Delay(1);
            cpu.ExecuteInstruction();
        }
        
        
        //
        // var sb = new StringBuilder();
        // var line = 0;
        // for (int i = 0; i < Ram.RamMemory.Length - 1; i = i + 2)
        // {
        //     sb.Append(Ram.RamMemory[i].ToString("X2"));
        //     sb.Append(Ram.RamMemory[i+1].ToString("X2"));
        //     sb.Append(" ");
        //     line++;
        //
        //     if (line % 12 == 0)
        //         sb.Append(Environment.NewLine);
        // }
        //
        // textBox1.Text = sb.ToString();
        // textBox1.Invalidate();

    }
}