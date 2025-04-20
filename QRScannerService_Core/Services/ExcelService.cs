using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using QRScannerService_Core.Interfaces;
using Excel = Microsoft.Office.Interop.Excel;

namespace QRScannerService_Core.Services
{
    public class ExcelService : IExcelService
    {
        private readonly ILogger<ExcelService> _logger;
        private Excel.Application _excelApp;
        private Excel.Workbook _workbook;
        private Excel.Worksheet _worksheet;
        private Excel.Range _currentCell;
        private bool _isExcelOwned;

        // Collection to store data when Excel is not open
        private List<TimestampedData> _collectedData = new List<TimestampedData>();
        private string _targetExcelFile;
        private bool _isHeadlessMode = false;

        public ExcelService(ILogger<ExcelService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Initialize()
        {
            if (_isHeadlessMode)
            {
                _logger.LogInformation("Running in headless mode - Excel will not be opened");
                return;
            }

            try
            {
                // Try to get the running Excel instance
                try
                {
                    _excelApp = (Excel.Application)Marshal.GetActiveObject("Excel.Application");
                    _isExcelOwned = false;
                    _logger.LogInformation("Connected to existing Excel instance");
                }
                catch (COMException)
                {
                    _logger.LogWarning("No Excel instance found. Please open Excel first.");
                    throw new InvalidOperationException("Excel is not open. Please open Excel and position the cursor where you want to insert data.");
                }

                // Get the active workbook and worksheet
                try
                {
                    _workbook = _excelApp.ActiveWorkbook;
                    if (_workbook == null)
                    {
                        _logger.LogWarning("No active workbook found.");
                        throw new InvalidOperationException("Could not find an active workbook. Please ensure a workbook is open.");
                    }

                    _worksheet = (Excel.Worksheet)_workbook.ActiveSheet;
                    if (_worksheet == null)
                    {
                        _logger.LogWarning("No active worksheet found.");
                        throw new InvalidOperationException("Could not find an active worksheet. Please ensure a worksheet is open and selected.");
                    }

                    _currentCell = _excelApp.ActiveCell;
                    if (_currentCell == null)
                    {
                        _logger.LogWarning("No active cell found in the worksheet.");
                        throw new InvalidOperationException("Could not find an active cell in the worksheet. Please ensure a cell is selected.");
                    }

                    _logger.LogInformation($"Connected to active worksheet. Current cell: {_currentCell.Address}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accessing active worksheet");
                    throw new InvalidOperationException("Could not access the active Excel worksheet. Please ensure a worksheet is open and selected.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Excel service");
                Cleanup();
                throw;
            }
        }

        public void AppendToExcel(string[] data)
        {
            // If in headless mode, just store the data
            if (_isHeadlessMode)
            {
                StoreDataWithoutExcel(data, _targetExcelFile);
                return;
            }

            if (_worksheet == null)
            {
                throw new InvalidOperationException("Excel worksheet not initialized. Call Initialize first.");
            }

            try
            {
                // Find the last used row in the worksheet
                int lastUsedRow = _worksheet.Cells[_worksheet.Rows.Count, 1].End(Excel.XlDirection.xlUp).Row;

                // If the worksheet is empty, start from the first row
                int nextRow = lastUsedRow == 1 && _worksheet.Cells[1, 1].Value == null ? 1 : lastUsedRow + 1;

                // Add timestamp as the first column
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _worksheet.Cells[nextRow, 1].Value = timestamp;

                // Write data to the next available row - No duplicate checking
                for (int i = 0; i < data.Length; i++)
                {
                    _worksheet.Cells[nextRow, i + 2].Value = data[i]; // +2 because column 1 is timestamp
                }

                _logger.LogInformation($"Data written successfully at row {nextRow} with timestamp {timestamp}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to append data to Excel");
                throw;
            }
        }

        public void Cleanup()
        {
            // If in headless mode, save collected data to Excel
            if (_isHeadlessMode && _collectedData.Count > 0)
            {
                try
                {
                    SaveCollectedDataToExcel();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save collected data to Excel during cleanup");
                }
            }

            try
            {
                if (_currentCell != null)
                {
                    Marshal.ReleaseComObject(_currentCell);
                    _currentCell = null;
                }

                if (_worksheet != null)
                {
                    Marshal.ReleaseComObject(_worksheet);
                    _worksheet = null;
                }

                if (_workbook != null)
                {
                    Marshal.ReleaseComObject(_workbook);
                    _workbook = null;
                }

                if (_excelApp != null)
                {
                    if (_isExcelOwned)
                    {
                        _excelApp.Quit();
                    }
                    Marshal.ReleaseComObject(_excelApp);
                    _excelApp = null;
                }

                _logger.LogInformation("Excel resources cleaned up successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup");
            }
        }

        public void OpenExcelFile(string filePath)
        {
            try
            {
                // Check if the Excel application is already running
                try
                {
                    _excelApp = (Excel.Application)Marshal.GetActiveObject("Excel.Application");
                    _isExcelOwned = false;
                    _logger.LogInformation("Connected to existing Excel instance");
                }
                catch (COMException)
                {
                    _excelApp = new Excel.Application();
                    _isExcelOwned = true;
                    _logger.LogInformation("Started a new Excel instance");
                }

                // Check if the workbook is already open
                foreach (Excel.Workbook wb in _excelApp.Workbooks)
                {
                    if (wb.FullName.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                    {
                        _workbook = wb;
                        _worksheet = (Excel.Worksheet)_workbook.Sheets[1];
                        _currentCell = _worksheet.Cells[1, 1];
                        _excelApp.Visible = true;

                        _logger.LogInformation($"Excel file is already opened: {filePath}");
                        MessageBox.Show("Excel file is already opened or exists", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                // Open the workbook if it is not already open
                _workbook = _excelApp.Workbooks.Open(filePath);
                _worksheet = (Excel.Worksheet)_workbook.Sheets[1];
                _currentCell = _worksheet.Cells[1, 1];
                _excelApp.Visible = true;

                _logger.LogInformation($"Opened Excel file: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to open Excel file: {filePath}");
                throw;
            }
        }

        // New method to store data without opening Excel
        public void StoreDataWithoutExcel(string[] data, string filePath)
        {
            _isHeadlessMode = true;
            _targetExcelFile = filePath;

            // Store data without duplicate checking
            if (data.Length > 0)  // Only add non-empty data arrays
            {
                // Create a timestamped data object
                var timestampedData = new TimestampedData
                {
                    Timestamp = DateTime.Now,
                    Data = data
                };

                _collectedData.Add(timestampedData);
                _logger.LogInformation($"Data stored in memory with timestamp {timestampedData.Timestamp}. Total records: {_collectedData.Count}");
            }
        }

        // New method to save collected data to Excel
        public void SaveCollectedDataToExcel()
        {
            if (_collectedData.Count == 0)
            {
                _logger.LogInformation("No data to save to Excel");
                return;
            }

            if (string.IsNullOrEmpty(_targetExcelFile))
            {
                _logger.LogWarning("No target Excel file specified");
                return;
            }

            try
            {
                bool fileExists = File.Exists(_targetExcelFile);

                // Create a new Excel application
                Excel.Application excelApp = new Excel.Application();
                Excel.Workbook workbook = null;
                Excel.Worksheet worksheet = null;

                try
                {
                    if (fileExists)
                    {
                        // Open existing file
                        workbook = excelApp.Workbooks.Open(_targetExcelFile);
                        worksheet = (Excel.Worksheet)workbook.Sheets[1];

                        // Find the last used row
                        int lastRow = worksheet.Cells[worksheet.Rows.Count, 1].End(Excel.XlDirection.xlUp).Row;
                        int startRow = lastRow == 1 && worksheet.Cells[1, 1].Value == null ? 1 : lastRow + 1;

                        // Write data with timestamps
                        for (int i = 0; i < _collectedData.Count; i++)
                        {
                            var item = _collectedData[i];

                            // Write timestamp in first column
                            worksheet.Cells[startRow + i, 1].Value = item.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

                            // Write data in subsequent columns
                            for (int j = 0; j < item.Data.Length; j++)
                            {
                                worksheet.Cells[startRow + i, j + 2].Value = item.Data[j]; // +2 because column 1 is timestamp
                            }
                        }

                        // Save the workbook
                        workbook.Save();
                    }
                    else
                    {
                        // Create new file
                        workbook = excelApp.Workbooks.Add();
                        worksheet = (Excel.Worksheet)workbook.Sheets[1];

                        // Write data with timestamps
                        for (int i = 0; i < _collectedData.Count; i++)
                        {
                            var item = _collectedData[i];

                            // Write timestamp in first column
                            worksheet.Cells[i + 1, 1].Value = item.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

                            // Write data in subsequent columns
                            for (int j = 0; j < item.Data.Length; j++)
                            {
                                worksheet.Cells[i + 1, j + 2].Value = item.Data[j]; // +2 because column 1 is timestamp
                            }
                        }

                        // Save as new file
                        workbook.SaveAs(_targetExcelFile);
                    }

                    _logger.LogInformation($"Successfully saved {_collectedData.Count} records to {_targetExcelFile}");

                    // Clear the collected data
                    _collectedData.Clear();
                }
                finally
                {
                    // Clean up
                    if (worksheet != null) Marshal.ReleaseComObject(worksheet);
                    if (workbook != null)
                    {
                        workbook.Close(true);
                        Marshal.ReleaseComObject(workbook);
                    }
                    if (excelApp != null)
                    {
                        excelApp.Quit();
                        Marshal.ReleaseComObject(excelApp);
                    }

                    // Force garbage collection to release COM objects
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                // Add a small delay to ensure the file is written to disk
                System.Threading.Thread.Sleep(1000);

                _logger.LogInformation("Excel save operation completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to save collected data to Excel file: {_targetExcelFile}");
                throw;
            }
        }

        public void ClearHeadlessMode()
        {
            _isHeadlessMode = false;
            _collectedData.Clear();
            _targetExcelFile = null;
            _logger.LogInformation("Headless mode cleared and collected data reset");
        }

        // Class to store data with timestamp
        private class TimestampedData
        {
            public DateTime Timestamp { get; set; }
            public string[] Data { get; set; }
        }
    }
}
