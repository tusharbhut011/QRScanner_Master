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
        // Removed unused field
        // private static readonly string AppName = "QRScannerService";

        public MainForm(ISerialPortService serialPortService, IWorkflowService workflowService, IExcelService excelService)
        {
            InitializeComponent();
            _serialPortService = serialPortService ?? throw new ArgumentNullException(nameof(serialPortService));
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));

            PopulateComPorts();
            btnStopService.Enabled = false;
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
        }

        private void InitializeSystemTray()
        {
            // Create tray menu
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Show", null, OnTrayShowClick);
            trayMenu.Items.Add("Start Service", null, OnTrayStartServiceClick);
            trayMenu.Items.Add("Stop Service", null, OnTrayStopServiceClick);
            trayMenu.Items.Add("-"); // Separator
            trayMenu.Items.Add("Exit", null, OnTrayExitClick);

            // Create tray icon
            trayIcon = new NotifyIcon();
            trayIcon.Text = "QR Scanner Service";
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
                trayIcon.ShowBalloonTip(3000, "QR Scanner Service", "The application is still running in the background.", ToolTipIcon.Info);
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
                    MessageBox.Show($"Error stopping service: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void PopulateComPorts()
        {
            cmbPortName.Items.Clear();
            foreach (string port in SerialPort.GetPortNames())
            {
                cmbPortName.Items.Add(port);
            }
            if (cmbPortName.Items.Count > 0)
            {
                cmbPortName.SelectedIndex = 0;
            }
        }

        private void SerialPortService_DataReceived(object sender, string data)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(UpdateStatus), $"Data received and written to Excel: {data}");
            }
            else
            {
                UpdateStatus($"Data received and written to Excel: {data}");
            }
        }

        private void UpdateStatus(string message)
        {
            lblStatus.Text = message;

            // Also update the tray icon tooltip with the status
            if (trayIcon != null)
            {
                trayIcon.Text = $"QR Scanner Service - {message}";
            }
        }

        private bool ValidateWorkflowInputs()
        {
            if (string.IsNullOrWhiteSpace(txtPrefix.Text))
            {
                MessageBox.Show("Please enter a prefix for the workflow.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("Please select a COM port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!ValidateWorkflowInputs())
                {
                    return;
                }

                // Show instruction message
                MessageBox.Show(
                    "Please ensure Excel is open and the cursor is positioned where you want to insert the data.",
                    "Excel Preparation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                SaveCurrentWorkflow();

                string selectedPort = cmbPortName.SelectedItem.ToString();
                int baudRate = int.Parse(txtBaudRate.Text);

                // Initialize Excel first
                _excelService.Initialize();

                // Then initialize and start the serial port service
                _serialPortService.Initialize(selectedPort, baudRate);
                _serialPortService.Start();

                UpdateStatus("Service started. Ready to scan QR codes.");
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
            catch (InvalidOperationException ex) when (ex.Message.Contains("Excel is not open"))
            {
                MessageBox.Show(
                    "Excel is not open. Please open Excel and position the cursor where you want to insert data.",
                    "Excel Not Open",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                btnStartService.Enabled = true;
                btnStopService.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting service: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStartService.Enabled = true;
                btnStopService.Enabled = false;
            }
        }

        private void btnStopService_Click(object sender, EventArgs e)
        {
            try
            {
                _serialPortService.Stop();
                UpdateStatus("Service stopped successfully");
                btnStartService.Enabled = true;
                btnStopService.Enabled = false;

                // Update tray menu
                trayMenu.Items[1].Enabled = true;  // Start Service
                trayMenu.Items[2].Enabled = false; // Stop Service
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping service: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("Workflow added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("A workflow with this prefix already exists.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding workflow: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
                openFileDialog.Title = "Select an Excel File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtExcelFile.Text = openFileDialog.FileName;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {

        }
    }
}