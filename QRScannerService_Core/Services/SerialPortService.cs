using System;
using System.IO.Ports;
using Microsoft.Extensions.Logging;
using QRScannerService_Core.Interfaces;

namespace QRScannerService_Core.Services
{
    public class SerialPortService : ISerialPortService
    {
        private SerialPort _serialPort;
        private readonly IExcelService _excelService;
        private readonly ILogger<SerialPortService> _logger;

        public event EventHandler<string> DataReceived;

        public SerialPortService(IExcelService excelService, ILogger<SerialPortService> logger)
        {
            _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Initialize(string portName, int baudRate)
        {
            try
            {
                _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
                _serialPort.DataReceived += SerialPort_DataReceived;
                _logger.LogInformation($"Serial port initialized: {portName}, Baud Rate: {baudRate}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing serial port");
                throw;
            }
        }

        public void Start()
        {
            try
            {
                if (_serialPort == null)
                {
                    throw new InvalidOperationException("Serial port not initialized. Call Initialize first.");
                }

                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    _logger.LogInformation($"Serial port opened: {_serialPort.PortName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open serial port");
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _logger.LogInformation($"Serial port closed: {_serialPort.PortName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to close serial port");
                throw;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null) return;

            try
            {
                string data = _serialPort.ReadExisting();
                _logger.LogDebug($"Raw data received: {data}");

                if (!string.IsNullOrEmpty(data))
                {
                    ProcessReceivedData(data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from serial port");
            }
        }

        private void ProcessReceivedData(string data)
        {
            try
            {
                // Split the data into an array
                string[] dataArray = data.Split(new[] { "' '", "'" }, StringSplitOptions.RemoveEmptyEntries);

                // Trim each element in the array
                for (int i = 0; i < dataArray.Length; i++)
                {
                    dataArray[i] = dataArray[i].Trim();
                }

                // Log processed data
                _logger.LogInformation($"Processed data: {string.Join(", ", dataArray)}");

                // Add the data to Excel
                _excelService.AppendToExcel(dataArray);

                // Raise the DataReceived event
                DataReceived?.Invoke(this, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing data: {data}");
            }
        }
    }
}