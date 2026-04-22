using System;
using System.Runtime.InteropServices;

namespace MiniTransportTycoon.UI.Rendering.Utils
{
    public static class ConsoleAllocator
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();

        public static void ShowConsole()
        {
            AllocConsole();
            Console.SetOut(new System.IO.StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }

        public static void HideConsole()
        {
            FreeConsole();
        }
    }
}