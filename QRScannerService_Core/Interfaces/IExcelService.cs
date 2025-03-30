namespace QRScannerService_Core.Interfaces
{
    public interface IExcelService
    {
        void Initialize();
        void AppendToExcel(string[] data);
        void Cleanup();
        void OpenExcelFile(string filePath); // Add this method
    }
}



//namespace QRScannerService_Core.Interfaces
//{
//    public interface IExcelService
//    {
//        void AppendToExcel(string filePath, string data);
//        bool IsFileOpen(string filePath);
//    }
//}

//namespace QRScannerService_Core.Interfaces
//{
//    public interface IExcelService
//    {
//        void AppendToExcel(string[] data);
//    }
//}


