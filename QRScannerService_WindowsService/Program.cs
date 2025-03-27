using System.ServiceProcess;
using QRScannerService_WindowsService;

namespace QRScannerService
{
    internal static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new QRScannerWindowsService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

