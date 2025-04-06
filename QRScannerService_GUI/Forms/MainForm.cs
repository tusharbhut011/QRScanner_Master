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

        public MainForm(ISerialPortService serialPortService, IWorkflowService workflowService, IExcelService excelService)
        {
            // Set the current UI culture before initializing components
            LanguageManager.SetLanguage(LanguageManager.GetCurrentLanguage());

            InitializeComponent();

            _serialPortService = serialPortService ?? throw new ArgumentNullException(nameof(serialPortService));
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));

            // Initialize language dropdown
            InitializeLanguageDropdown();

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

                // Append data to Excel and check for duplicates
                string[] dataArray = data.Split(','); // Assuming data is comma-separated
                _excelService.AppendToExcel(dataArray);
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

                // Open Excel file if not already open
                _excelService.OpenExcelFile(_currentWorkflow.ExcelFile);

                // Initialize Excel first
                _excelService.Initialize();

                // Then initialize and start the serial port service
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

                bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);
                string message = isGerman
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
            if (trayMenu.Items["trayShow"] is ToolStripMenuItem trayShow)
                trayShow.Text = isGerman ? "Anzeigen" : "Show";

            if (trayMenu.Items["trayStartService"] is ToolStripMenuItem trayStartService)
                trayStartService.Text = isGerman ? "Dienst starten" : "Start Service";

            if (trayMenu.Items["trayStopService"] is ToolStripMenuItem trayStopService)
                trayStopService.Text = isGerman ? "Dienst stoppen" : "Stop Service";

            if (trayMenu.Items["trayExit"] is ToolStripMenuItem trayExit)
                trayExit.Text = isGerman ? "Beenden" : "Exit";
        }
    }
}