namespace QRScannerService_Core.Interfaces
{
    public interface IExcelService
    {
        void Initialize();
        void AppendToExcel(string[] data);
        void Cleanup();
        void OpenExcelFile(string filePath);
        void StoreDataWithoutExcel(string[] data, string filePath);
        void SaveCollectedDataToExcel();
        void ClearHeadlessMode();
    }
}
