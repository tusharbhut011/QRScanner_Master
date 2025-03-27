using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QRScannerService_Core.Interfaces;
using QRScannerService_Core.Services;
using QRScannerService_GUI.Forms;

namespace QRScannerService_GUI
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);

            // Create service provider
            using (var serviceProvider = services.BuildServiceProvider())
            {
                // Initialize services
                var serialPortService = serviceProvider.GetRequiredService<ISerialPortService>();
                var workflowService = serviceProvider.GetRequiredService<IWorkflowService>();
                var excelService = serviceProvider.GetRequiredService<IExcelService>();

                // Create the main form
                MainForm mainForm = new MainForm(serialPortService, workflowService, excelService);

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
    }
}

