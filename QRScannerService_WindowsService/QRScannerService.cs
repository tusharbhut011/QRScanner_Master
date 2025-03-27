using System;
using System.ServiceProcess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QRScannerService_Core.Interfaces; // Add this using directive
using QRScannerService_Core.Services; // Add this using directive

namespace QRScannerService
{
    public partial class QRScannerService : ServiceBase
    {
        private IServiceProvider _serviceProvider;
        private ISerialPortService _serialPortService;
        private IExcelService _excelService;
        private IWorkflowService _workflowService;
        private ILogger<QRScannerService> _logger; // Fixed type name

        public QRScannerService()
        {
            InitializeComponent();
            ConfigureServices();
        }
        private void InitializeComponent()
        {
            // Initialize components here if needed
            this.ServiceName = "QRScannerService";
        }

        private void ConfigureServices()
        {
            ServiceCollection services = new ServiceCollection();

            services.AddLogging(configure => configure.AddEventLog());
            services.AddSingleton<ISerialPortService, SerialPortService>();
            services.AddSingleton<IExcelService, ExcelService>();
            services.AddSingleton<IWorkflowService, WorkflowService>();

            _serviceProvider = services.BuildServiceProvider();

            _serialPortService = _serviceProvider.GetRequiredService<ISerialPortService>();
            _excelService = _serviceProvider.GetRequiredService<IExcelService>();
            _workflowService = _serviceProvider.GetRequiredService<IWorkflowService>();
            _logger = _serviceProvider.GetRequiredService<ILogger<QRScannerService>>();
        }

        protected override void OnStart(string[] args)
        {
            _logger.LogInformation("QR Scanner Service is starting.");

            try
            {
                // Load configuration
                ServiceConfig config = LoadConfiguration();

                // Initialize services
                _serialPortService.Initialize(config.PortName, config.BaudRate);
                _excelService.Initialize();

                // Start the serial port service
                _serialPortService.Start();

                _logger.LogInformation("QR Scanner Service started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting QR Scanner Service.");
                Stop();
            }
        }

        protected override void OnStop()
        {
            _logger.LogInformation("QR Scanner Service is stopping.");

            try
            {
                _serialPortService.Stop();
                _excelService.Cleanup();
                _logger.LogInformation("QR Scanner Service stopped successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping QR Scanner Service.");
            }
        }

        private ServiceConfig LoadConfiguration()
        {
            // TODO: Implement configuration loading from a file or registry
            // For now, we'll return default values
            return new ServiceConfig
            {
                PortName = "COM3",
                BaudRate = 9600
            };
        }
    }

    public class ServiceConfig
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
    }
}