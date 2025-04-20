using System;
using System.Windows.Forms;
using System.IO.Ports;
using QRScannerService_Core.Interfaces;
using QRScannerService_Core.Models;
using System.Linq;
using Microsoft.Win32;
using QRScannerService_GUI.Helpers;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;

namespace QRScannerService_GUI.Forms
{
    public partial class MainForm : Form
    {
        private readonly ISerialPortService _serialPortService;
        private readonly IExcelService _excelService;
        private readonly IWorkflowService _workflowService;
        private WorkflowConfig _currentWorkflow;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private const string StartupKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private bool _isHeadlessMode = false;

        // Add this at the class level
        private static string _lastUsedPrefix = string.Empty;

        public MainForm(ISerialPortService serialPortService, IWorkflowService workflowService, IExcelService excelService)
        {
            // Set the current UI culture before initializing components
            LanguageManager.SetLanguage(LanguageManager.GetCurrentLanguage());

            InitializeComponent();

            // Set "Don't open Excel" checkbox to checked by default
            chkNoExcel.Checked = true;
            _isHeadlessMode = true;

            _serialPortService = serialPortService ?? throw new ArgumentNullException(nameof(serialPortService));
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));

            // Initialize language dropdown
            InitializeLanguageDropdown();

            // Load saved prefix
            LoadSavedPrefix();

            PopulateComPorts();
            btnStopService.Enabled = false;
            cmbPortName.Enabled = true; // Ensure the COM port field is enabled
            _serialPortService.DataReceived += SerialPortService_DataReceived;
            btnAddWorkflow.Click += btnAddWorkflow_Click;
            btnBrowseExcel.Click += btnBrowseExcel_Click;

            // Load existing workflows
            UpdateWorkflowList();

            // Initialize system tray icon and menu
            InitializeSystemTray();

            // Handle form closing event
            this.FormClosing += MainForm_FormClosing;

            // Initialize auto-start checkbox
            chkStartWithWindows.Checked = StartupManager.IsStartWithWindowsEnabled();
            chkStartWithWindows.CheckedChanged += chkStartWithWindows_CheckedChanged;

            // Apply language to UI
            LanguageManager.UpdateUIText(this);

            // Update the "Don't open Excel" checkbox text based on language
            UpdateNoExcelCheckboxText();

            chkNoExcel.CheckedChanged += ChkNoExcel_CheckedChanged;
        }

        private void UpdateNoExcelCheckboxText()
        {
            bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
            chkNoExcel.Text = isGerman ? "Excel nicht öffnen" : "Don't open Excel";
        }

        private void btnRefreshComPorts_Click(object sender, EventArgs e)
        {
            PopulateComPorts();
        }

        private void PopulateComPorts()
        {
            string selectedPort = cmbPortName.SelectedItem?.ToString();
            cmbPortName.Items.Clear();
            foreach (string port in SerialPort.GetPortNames())
            {
                cmbPortName.Items.Add(port);
            }
            if (cmbPortName.Items.Count > 0)
            {
                if (selectedPort != null && cmbPortName.Items.Contains(selectedPort))
                {
                    cmbPortName.SelectedItem = selectedPort;
                }
                else
                {
                    cmbPortName.SelectedIndex = 0;
                }
            }
        }

        private void SerialPortService_DataReceived(object sender, string data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(UpdateDataReceived), data);
            }
            else
            {
                UpdateDataReceived(data);

                // Parse the QR code data using the custom parser
                string[] parsedData = ParseQRCodeData(data);

                if (_isHeadlessMode)
                {
                    _excelService.StoreDataWithoutExcel(parsedData, _currentWorkflow.ExcelFile);
                }
                else
                {
                    _excelService.AppendToExcel(parsedData);
                }
            }
        }

        private string[] ParseQRCodeData(string data)
        {
            try
            {
                // Log the raw data for debugging
                Debug.WriteLine("Raw QR Code Data: " + data);

                // Split by single quote and filter out empty entries
                string[] parts = data.Split('\'')
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToArray();

                // Create an array to hold our parsed data in the correct order for Excel columns
                // The order should match the column headers in ExcelService
                string[] parsedData = new string[17]; // 17 columns excluding timestamp

                // Map the parts to the correct positions in our parsedData array
                // This mapping is based on the observed format of the QR code data
                if (parts.Length >= 1) parsedData[0] = parts[0].Trim(); // Examiner (Bockisch)
                if (parts.Length >= 2) parsedData[1] = parts[1].Trim(); // Second Reviewer (Papenbrock)
                if (parts.Length >= 3) parsedData[8] = parts[2].Trim(); // Student ID (123456)
                if (parts.Length >= 4) parsedData[2] = parts[3].Trim(); // Surname (Doe)
                if (parts.Length >= 5) parsedData[1] = parts[4].Trim(); // First Name (John)
                if (parts.Length >= 6) parsedData[6] = parts[5].Trim(); // Alumni Agreement (Computer Science)
                if (parts.Length >= 7) parsedData[7] = parts[6].Trim(); // Preferred Language (Master)
                if (parts.Length >= 8) parsedData[8] = parts[7].Trim(); // Student ID (20181)
                if (parts.Length >= 9) parsedData[14] = parts[8].Trim(); // Title (Prof.)
                if (parts.Length >= 10) parsedData[15] = parts[9].Trim(); // Start Date (2025-04-15)
                if (parts.Length >= 11) parsedData[9] = parts[10].Trim(); // Uni Account (jd123@students.uni-marburg.de)
                if (parts.Length >= 12) parsedData[3] = parts[11].Trim(); // Private Email (johndoe@students.uni-marburg.de)
                if (parts.Length >= 13) parsedData[4] = parts[12].Trim(); // Phone (01761234567)
                if (parts.Length >= 14) parsedData[6] = parts[13].Trim(); // Alumni Agreement (TRUE)
                if (parts.Length >= 15) parsedData[5] = parts[14].Trim(); // Postal Address (Musterstrasse 12, 35037 Marburg, Germany)
                if (parts.Length >= 16) parsedData[7] = parts[15].Trim(); // Preferred Language (en)
                if (parts.Length >= 17) parsedData[10] = parts[16].Trim(); // Birthdate (1988-04-01)
                if (parts.Length >= 18) parsedData[11] = parts[17].Trim(); // Birthplace (Berlin, Germany)
                if (parts.Length >= 19) parsedData[16] = parts[18].Trim(); // CP Requirement (zugelassen)
                if (parts.Length >= 20) parsedData[16] = parts[19].Trim(); // Comment (The applicant has submitted...)

                // Log the parsed data for debugging
                for (int i = 0; i < parsedData.Length; i++)
                {
                    Debug.WriteLine($"Parsed Data [{i}]: {parsedData[i]}");
                }

                return parsedData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing QR code data: {ex.Message}");
                // Return empty array in case of error
                return new string[17];
            }
        }

        private void UpdateDataReceived(string data)
        {
            txtDataReceived.AppendText(data + Environment.NewLine);
        }

        private void UpdateStatus(string message)
        {
            lblStatus.Text = message;

            // Also update the tray icon tooltip with the status
            if (trayIcon != null)
            {
                bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                string prefix = isGerman ? "QR Dienst - " : "QR Service - ";
                string fullMessage = $"{prefix}{message}";

                // Ensure the text length does not exceed 64 characters
                if (fullMessage.Length > 63)
                {
                    fullMessage = fullMessage.Substring(0, 63);
                }

                trayIcon.Text = fullMessage;
            }
        }

        private bool ValidateWorkflowInputs()
        {
            if (string.IsNullOrWhiteSpace(txtPrefix.Text))
            {
                bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                string title = isGerman ? "Fehler" : "Error";
                string message = isGerman
                    ? "Bitte geben Sie ein Präfix für den Workflow ein."
                    : "Please enter a prefix for the workflow.";

                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtExcelFile.Text))
            {
                bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                string title = isGerman ? "Fehler" : "Error";
                string message = isGerman
                    ? "Bitte wählen Sie eine Excel-Datei aus."
                    : "Please select an Excel file.";

                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void SaveCurrentWorkflow()
        {
            _currentWorkflow = new WorkflowConfig
            {
                Prefix = txtPrefix.Text,
                ExcelFile = txtExcelFile.Text
            };
        }

        private void btnStartService_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbPortName.SelectedItem == null)
                {
                    bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                    string title = isGerman ? "Fehler" : "Error";
                    string message = isGerman
                        ? "Bitte wählen Sie einen COM-Port aus."
                        : "Please select a COM port.";

                    MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!ValidateWorkflowInputs())
                {
                    return;
                }

                SaveCurrentWorkflow();

                string selectedPort = cmbPortName.SelectedItem.ToString();
                int baudRate = int.Parse(txtBaudRate.Text);

                // Set headless mode based on checkbox
                _isHeadlessMode = chkNoExcel.Checked;

                if (_isHeadlessMode)
                {
                    // In headless mode, we don't need to initialize Excel
                    // Just store the target file path
                    _excelService.StoreDataWithoutExcel(new string[0], _currentWorkflow.ExcelFile);

                    bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                    string infoTitle = isGerman ? "Information" : "Information";
                    string infoMessage = isGerman
                        ? "Excel wird nicht geöffnet. Daten werden im Speicher gesammelt und später in die Excel-Datei geschrieben."
                        : "Excel will not be opened. Data will be collected in memory and written to the Excel file later.";

                    MessageBox.Show(infoMessage, infoTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Clear any headless mode data before switching to Excel mode
                    _excelService.ClearHeadlessMode();

                    // Open Excel file if not already open
                    _excelService.OpenExcelFile(_currentWorkflow.ExcelFile);

                    // Initialize Excel
                    _excelService.Initialize();
                }

                // Initialize and start the serial port service
                _serialPortService.Initialize(selectedPort, baudRate);
                _serialPortService.Start();

                bool isGermanStatus = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                string statusMessage = isGermanStatus
                    ? "Dienst gestartet. Bereit zum Scannen."
                    : "Service started. Ready to scan.";

                UpdateStatus(statusMessage);
                btnStartService.Enabled = false;
                btnStopService.Enabled = true;

                // Update tray menu
                trayMenu.Items[1].Enabled = false; // Start Service
                trayMenu.Items[2].Enabled = true;  // Stop Service

                // Add the workflow if it doesn't exist
                if (!_workflowService.GetAllWorkflows().Any(w => w.Prefix == _currentWorkflow.Prefix))
                {
                    _workflowService.AddWorkflow(_currentWorkflow);
                    UpdateWorkflowList();
                }
            }
            catch (Exception ex)
            {
                bool isGermanErr = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                string errTitle = isGermanErr ? "Fehler" : "Error";
                string errMessage = isGermanErr
                    ? $"Fehler beim Starten des Dienstes: {ex.Message}"
                    : $"Error starting service: {ex.Message}";

                MessageBox.Show(errMessage, errTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStartService.Enabled = true;
                btnStopService.Enabled = false;
            }
        }

        private void btnStopService_Click(object sender, EventArgs e)
        {
            try
            {
                _serialPortService.Stop();

                // If in headless mode, save the collected data to Excel
                if (_isHeadlessMode)
                {
                    bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                    string confirmTitle = isGerman ? "Daten speichern" : "Save Data";
                    string confirmMessage = isGerman
                        ? "Möchten Sie die gesammelten Daten jetzt in Excel speichern?"
                        : "Do you want to save the collected data to Excel now?";

                    DialogResult result = MessageBox.Show(confirmMessage, confirmTitle,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            _excelService.SaveCollectedDataToExcel();

                            string successTitle = isGerman ? "Erfolg" : "Success";
                            string successMessage = isGerman
                                ? "Daten wurden erfolgreich in Excel gespeichert."
                                : "Data was successfully saved to Excel.";

                            MessageBox.Show(successMessage, successTitle,
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            string errorTitle = isGerman ? "Fehler" : "Error";
                            string errorMessage = isGerman
                                ? $"Fehler beim Speichern der Daten: {ex.Message}"
                                : $"Error saving data: {ex.Message}";

                            MessageBox.Show(errorMessage, errorTitle,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                bool isGermanStatus = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                string message = isGermanStatus
                    ? "Dienst erfolgreich gestoppt"
                    : "Service stopped successfully";

                UpdateStatus(message);
                btnStartService.Enabled = true;
                btnStopService.Enabled = false;

                // Update tray menu
                trayMenu.Items[1].Enabled = true;  // Start Service
                trayMenu.Items[2].Enabled = false; // Stop Service
            }
            catch (Exception ex)
            {
                bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                string title = isGerman ? "Fehler" : "Error";
                string message = isGerman
                    ? $"Fehler beim Stoppen des Dienstes: {ex.Message}"
                    : $"Error stopping service: {ex.Message}";

                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddWorkflow_Click(object sender, EventArgs e)
        {
            if (!ValidateWorkflowInputs())
            {
                return;
            }

            try
            {
                var workflow = new WorkflowConfig
                {
                    Prefix = txtPrefix.Text,
                    ExcelFile = txtExcelFile.Text
                };

                // Save the prefix to settings
                SaveCurrentPrefix();

                if (!_workflowService.GetAllWorkflows().Any(w => w.Prefix == workflow.Prefix))
                {
                    _workflowService.AddWorkflow(workflow);
                    UpdateWorkflowList();

                    bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                    string title = isGerman ? "Erfolg" : "Success";
                    string message = isGerman
                        ? "Workflow erfolgreich hinzugefügt."
                        : "Workflow added successfully.";

                    MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                    string title = isGerman ? "Warnung" : "Warning";
                    string message = isGerman
                        ? "Ein Workflow mit diesem Präfix existiert bereits."
                        : "A workflow with this prefix already exists.";

                    MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                string title = isGerman ? "Fehler" : "Error";
                string message = isGerman
                    ? $"Fehler beim Hinzufügen des Workflows: {ex.Message}"
                    : $"Error adding workflow: {ex.Message}";

                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateWorkflowList()
        {
            listWorkflows.Items.Clear();
            foreach (var workflow in _workflowService.GetAllWorkflows())
            {
                listWorkflows.Items.Add($"{workflow.Prefix} - {workflow.ExcelFile}");
            }
        }

        private void btnBrowseExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
                openFileDialog.Title = isGerman ? "Excel-Datei auswählen" : "Select an Excel File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtExcelFile.Text = openFileDialog.FileName;
                }
            }
        }

        private void InitializeLanguageDropdown()
        {
            // Set the selected language in the dropdown
            cmbLanguage.SelectedIndex = (int)LanguageManager.GetCurrentLanguage();

            // Add event handler for language change
            cmbLanguage.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;
        }

        private void CmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get the selected language
            LanguageManager.Language selectedLanguage = (LanguageManager.Language)cmbLanguage.SelectedIndex;

            // If the language has changed
            if (selectedLanguage != LanguageManager.GetCurrentLanguage())
            {
                // Set the new language
                LanguageManager.SetLanguage(selectedLanguage);

                // Update the UI text
                LanguageManager.UpdateUIText(this);

                // Update the "Don't open Excel" checkbox text
                UpdateNoExcelCheckboxText();

                // Show message about the language change
                string message = selectedLanguage == LanguageManager.Language.German
                    ? "Die Sprache wurde geändert."
                    : "The language has been changed.";

                string title = selectedLanguage == LanguageManager.Language.German
                    ? "Sprache geändert"
                    : "Language Changed";

                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeSystemTray()
        {
            bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);

            // Create tray menu
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add(isGerman ? "Anzeigen" : "Show", null, OnTrayShowClick);
            trayMenu.Items.Add(isGerman ? "Dienst starten" : "Start Service", null, OnTrayStartServiceClick);
            trayMenu.Items.Add(isGerman ? "Dienst stoppen" : "Stop Service", null, OnTrayStopServiceClick);
            trayMenu.Items.Add("-"); // Separator
            trayMenu.Items.Add(isGerman ? "Beenden" : "Exit", null, OnTrayExitClick);

            // Create tray icon
            trayIcon = new NotifyIcon();
            trayIcon.Text = isGerman ? "QR Scanner Dienst" : "QR Scanner Service";
            trayIcon.Icon = SystemIcons.Application; // You can replace with your own icon
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += OnTrayShowClick;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If the user clicks the X button, minimize to tray instead of closing
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                trayIcon.Visible = true;

                bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                string title = isGerman ? "QR Scanner Dienst" : "QR Scanner Service";
                string message = isGerman
                    ? "Die Anwendung läuft weiterhin im Hintergrund."
                    : "The application is still running in the background.";

                trayIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
            }
            else
            {
                // Clean up tray icon
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
        }

        private void OnTrayShowClick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.Show();
            this.BringToFront();
        }

        private void OnTrayStartServiceClick(object sender, EventArgs e)
        {
            if (btnStartService.Enabled)
            {
                btnStartService_Click(sender, e);
            }
        }

        private void OnTrayStopServiceClick(object sender, EventArgs e)
        {
            if (btnStopService.Enabled)
            {
                btnStopService_Click(sender, e);
            }
        }

        private void OnTrayExitClick(object sender, EventArgs e)
        {
            // Stop the service if it's running
            if (btnStopService.Enabled)
            {
                try
                {
                    _serialPortService.Stop();

                    // Save collected data to Excel
                    _excelService.SaveCollectedDataToExcel();
                }
                catch (Exception ex)
                {
                    bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                    string title = isGerman ? "Fehler" : "Error";
                    string message = isGerman
                        ? $"Fehler beim Speichern der Daten: {ex.Message}"
                        : $"Error saving data: {ex.Message}";

                    MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Actually close the application
            trayIcon.Visible = false;
            trayIcon.Dispose();
            Application.Exit();
        }

        private void chkStartWithWindows_CheckedChanged(object sender, EventArgs e)
        {
            Debug.WriteLine($"Start with Windows checkbox changed to: {chkStartWithWindows.Checked}");
            StartupManager.SetStartWithWindows(chkStartWithWindows.Checked);

            // Verify the change was successful
            bool verifyEnabled = StartupManager.IsStartWithWindowsEnabled();
            if (verifyEnabled != chkStartWithWindows.Checked)
            {
                Debug.WriteLine("Registry change verification failed");
                chkStartWithWindows.Checked = verifyEnabled;
            }
        }

        // Add this method to the MainForm class
        public void UpdateTrayMenuText(bool isGerman)
        {
            if (trayMenu.Items.Count >= 5)
            {
                trayMenu.Items[0].Text = isGerman ? "Anzeigen" : "Show";
                trayMenu.Items[1].Text = isGerman ? "Dienst starten" : "Start Service";
                trayMenu.Items[2].Text = isGerman ? "Dienst stoppen" : "Stop Service";
                trayMenu.Items[4].Text = isGerman ? "Beenden" : "Exit";
            }
        }

        private void LoadSavedPrefix()
        {
            try
            {
                // Load from static variable
                if (!string.IsNullOrEmpty(_lastUsedPrefix))
                {
                    txtPrefix.Text = _lastUsedPrefix;
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't show a message to the user
                Debug.WriteLine($"Error loading saved prefix: {ex.Message}");
            }
        }

        private void SaveCurrentPrefix()
        {
            try
            {
                // Save to static variable
                if (!string.IsNullOrEmpty(txtPrefix.Text))
                {
                    _lastUsedPrefix = txtPrefix.Text;
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't show a message to the user
                Debug.WriteLine($"Error saving prefix: {ex.Message}");
            }
        }

        private void ChkNoExcel_CheckedChanged(object sender, EventArgs e)
        {
            if (btnStopService.Enabled) // Service is running
            {
                bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                string title = isGerman ? "Modus ändern" : "Mode Change";
                string message = isGerman
                    ? "Der Dienst muss neu gestartet werden, um den Excel-Modus zu ändern. Möchten Sie den Dienst jetzt neu starten?"
                    : "The service needs to be restarted to change the Excel mode. Do you want to restart the service now?";

                DialogResult result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // Store the new mode setting before stopping the service
                    bool newHeadlessMode = chkNoExcel.Checked;

                    // Stop the service
                    btnStopService_Click(sender, e);

                    // If we were in headless mode and now switching to Excel mode, save the data
                    if (!newHeadlessMode && _isHeadlessMode && _currentWorkflow != null)
                    {
                        try
                        {
                            _excelService.SaveCollectedDataToExcel();

                            string successTitle = isGerman ? "Erfolg" : "Success";
                            string successMessage = isGerman
                                ? "Daten wurden erfolgreich in Excel gespeichert."
                                : "Data was successfully saved to Excel.";

                            MessageBox.Show(successMessage, successTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            string errorTitle = isGerman ? "Fehler" : "Error";
                            string errorMessage = isGerman
                                ? $"Fehler beim Speichern der Daten: {ex.Message}"
                                : $"Error saving data: {ex.Message}";

                            MessageBox.Show(errorMessage, errorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    // Update the headless mode flag
                    _isHeadlessMode = newHeadlessMode;

                    // Start the service again
                    btnStartService_Click(sender, e);
                }
                else
                {
                    // Revert the checkbox to its previous state
                    chkNoExcel.CheckedChanged -= ChkNoExcel_CheckedChanged;
                    chkNoExcel.Checked = _isHeadlessMode;
                    chkNoExcel.CheckedChanged += ChkNoExcel_CheckedChanged;
                }
            }
            else
            {
                // If service is not running, just update the headless mode flag
                _isHeadlessMode = chkNoExcel.Checked;
            }
        }
    }
}
