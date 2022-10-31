using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace NewGear.MainMachine.GUI {
    internal static partial class MainWindow {
        [DllImport("dwmapi.dll", SetLastError = true)]
        private static extern bool DwmSetWindowAttribute(IntPtr handle, int param, in int value, int size);

        internal static void InitColorMode(IntPtr handle) {
            const string ColorThemeKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            const string ColorThemeValue = "SystemUsesLightTheme";

            if(!OperatingSystem.IsWindowsVersionAtLeast(10))
                return; // Only works on windows 10+

            // Set color mode the first time:
            CheckColorMode();

            void CheckColorMode() {
                int value = ((int?) Registry.GetValue(ColorThemeKey, ColorThemeValue, 0)) ?? 0;
                value = value == 1 ? 0 : 1;

                if(!DwmSetWindowAttribute(handle, 20, value, sizeof(int)))
                    DwmSetWindowAttribute(handle, 19, value, sizeof(int));
            }

            // Use a timer to check color mode every second:
            System.Timers.Timer timer = new(1000);
            timer.Elapsed += (o, e) => CheckColorMode();

            timer.Start();
        }
    }
}
