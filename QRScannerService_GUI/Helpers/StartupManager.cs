using System;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using QRScannerService_Core; // Add this using directive

namespace QRScannerService_GUI.Helpers
{
    public static class StartupManager
    {
        private const string StartupKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private static readonly string AppName = "QRScannerService";

        /// <summary>
        /// Sets whether the application should start automatically with Windows
        /// </summary>
        public static void SetStartWithWindows(bool startWithWindows)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupKey, true))
                {
                    if (key != null)
                    {
                        if (startWithWindows)
                        {
                            string appPath = Assembly.GetExecutingAssembly().Location;
                            Debug.WriteLine($"Setting startup registry key. Path: {appPath}");

                            // Make sure the path is wrapped in quotes and includes the /minimized parameter
                            string registryValue = $"\"{appPath}\" /minimized";
                            key.SetValue(AppName, registryValue);

                            // Verify the entry was created
                            string verifyValue = key.GetValue(AppName) as string;
                            if (verifyValue != registryValue)
                            {
                                MessageBox.Show(
                                    "Failed to set startup registry key. Please run the application as administrator.",
                                    "Registry Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning
                                );
                            }
                            else
                            {
                                Debug.WriteLine("Successfully set startup registry key");
                            }
                        }
                        else
                        {
                            if (key.GetValue(AppName) != null)
                            {
                                key.DeleteValue(AppName);
                                Debug.WriteLine("Removed startup registry key");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            "Cannot access Windows startup registry key. Please run the application as administrator.",
                            "Registry Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error accessing registry: {ex.Message}");
                MessageBox.Show(
                    $"Error setting startup: {ex.Message}\n\nPlease run the application as administrator.",
                    "Registry Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Checks if the application is set to start with Windows
        /// </summary>
        public static bool IsStartWithWindowsEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupKey))
                {
                    if (key != null)
                    {
                        string value = key.GetValue(AppName) as string;
                        Debug.WriteLine($"Current startup registry value: {value}");
                        return value != null;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking startup status: {ex.Message}");
                return false;
            }
        }
    }
}