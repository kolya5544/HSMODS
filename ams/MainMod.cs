using Mod;
using RGiesecke.DllExport;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

/// <summary>
/// Main class of the mod.
/// You have to compile the DLL in either x86, or x64 mode. AnyCPU is not allowed.
/// </summary>
public static class AdvancedMod
{
    /// <summary>
    /// This function is called on game start.
    /// </summary>
    [ComVisible(true)]
    [DllExport("Start", System.Runtime.InteropServices.CallingConvention.Winapi)]
    public static void Start()
    {
        //It will show a messagebox on game start up.
        MessageBox.Show("Hello world", "Hello", MessageBoxButton.OK, MessageBoxImage.Error);

        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;

            //An example of writing and reading to memory using a mod, every second
            IntPtr CurrentProcess = Utilities.GetCurrentProcess();
            int baseAddress = (int)Utilities.GetModuleHandle(null);
            byte[] Buffer = new byte[8];
            while (true)
            {
                Thread.Sleep(1000);
                Utilities.ReadProcessMemory(CurrentProcess, baseAddress + 0x100, ref Buffer, Buffer.Length, 0);
                Utilities.WriteProcessMemory(CurrentProcess, baseAddress + 0x100, ref Buffer, Buffer.Length, 0);
            }
        }).Start();
    }
}

