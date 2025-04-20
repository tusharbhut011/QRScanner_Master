using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QRScannerService_Core.Interfaces;
using QRScannerService_Core.Services;
using QRScannerService_GUI.Forms;
using QRScannerService_GUI.Helpers;

namespace QRScannerService_GUI
{
    static class Program
    {
        private static IExcelService _excelService;

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Set the application language based on saved settings
            LanguageManager.SetLanguage(LanguageManager.GetCurrentLanguage());

            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);

            // Create service provider
            using (var serviceProvider = services.BuildServiceProvider())
            {
                // Initialize services
                var serialPortService = serviceProvider.GetRequiredService<ISerialPortService>();
                var workflowService = serviceProvider.GetRequiredService<IWorkflowService>();
                _excelService = serviceProvider.GetRequiredService<IExcelService>();

                // Subscribe to the ProcessExit event
                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

                // Create the main form
                MainForm mainForm = new MainForm(serialPortService, workflowService, _excelService);

                // Check if we should start minimized (when launched at Windows startup)
                bool startMinimized = Array.Exists(args, arg => arg.ToLower() == "/minimized");
                if (startMinimized)
                {
                    mainForm.WindowState = FormWindowState.Minimized;
                    mainForm.ShowInTaskbar = false;
                }

                // Run the application
                Application.Run(mainForm);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Add logging
            services.AddLogging(configure => configure.AddConsole());

            // Register services
            services.AddSingleton<ISerialPortService, SerialPortService>();
            services.AddSingleton<IWorkflowService, WorkflowService>();
            services.AddSingleton<IExcelService, ExcelService>();
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                // Save collected data to Excel
                _excelService.SaveCollectedDataToExcel();
            }
            catch (Exception ex)
            {
                // Log the error (if logging is available) or handle it as needed
                Console.WriteLine($"Error saving data during process exit: {ex.Message}");
            }
        }
    }
}