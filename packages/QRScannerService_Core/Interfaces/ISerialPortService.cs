using System;

namespace QRScannerService_Core.Interfaces
{
    public interface ISerialPortService
    {
        void Initialize(string portName, int baudRate);
        void Start();
        void Stop();
        event EventHandler<string> DataReceived;
    }
}