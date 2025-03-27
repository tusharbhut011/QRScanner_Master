using System;
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

        public ExcelService(ILogger<ExcelService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Initialize()
        {
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
                    _worksheet = (Excel.Worksheet)_excelApp.ActiveSheet;
                    _currentCell = _excelApp.ActiveCell;

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
            if (_worksheet == null || _currentCell == null)
            {
                throw new InvalidOperationException("Excel worksheet not initialized. Call Initialize first.");
            }

            try
            {
                // Store the current selection
                Excel.Range originalSelection = _excelApp.Selection;

                _logger.LogInformation($"Writing to Excel at row {_currentCell.Row}, column {_currentCell.Column}");

                // Define the column to check for duplicates (e.g., column 1)
                int columnToCheck = 1;
                bool isDuplicate = false;

                // Check for duplicates in the specified column
                Excel.Range usedRange = _worksheet.UsedRange;
                for (int row = 1; row <= usedRange.Rows.Count; row++)
                {
                    Excel.Range cell = _worksheet.Cells[row, columnToCheck];
                    if (cell.Value != null && cell.Value.ToString() == data[0])
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (isDuplicate)
                {
                    _logger.LogInformation("Data is already added");
                    MessageBox.Show("Data is already added", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Write data
                    for (int i = 0; i < data.Length; i++)
                    {
                        _currentCell.Offset[0, i].Value = data[i];
                    }

                    // Move to next row
                    _currentCell = _currentCell.Offset[1, 0];

                    // Restore original selection
                    originalSelection.Select();

                    _logger.LogInformation($"Data written successfully. Next position: Row {_currentCell.Row}, Column {_currentCell.Column}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to append data to Excel");
                throw;
            }
        }

        public void Cleanup()
        {
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
                _excelApp = new Excel.Application();
                _workbook = _excelApp.Workbooks.Open(filePath);
                _worksheet = (Excel.Worksheet)_workbook.Sheets[1];
                _currentCell = _worksheet.Cells[1, 1];
                _excelApp.Visible = true;
                _isExcelOwned = true;

                _logger.LogInformation($"Opened Excel file: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to open Excel file: {filePath}");
                throw;
            }
        }
    }
}