namespace QRScannerService_GUI.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStartService = new System.Windows.Forms.Button();
            this.btnStopService = new System.Windows.Forms.Button();
            this.btnAddWorkflow = new System.Windows.Forms.Button();
            this.lblPortName = new System.Windows.Forms.Label();
            this.lblBaudRate = new System.Windows.Forms.Label();
            this.lblPrefix = new System.Windows.Forms.Label();
            this.lblExcelFile = new System.Windows.Forms.Label();
            this.txtBaudRate = new System.Windows.Forms.TextBox();
            this.txtPrefix = new System.Windows.Forms.TextBox();
            this.txtExcelFile = new System.Windows.Forms.TextBox();
            this.btnBrowseExcel = new System.Windows.Forms.Button();
            this.listWorkflows = new System.Windows.Forms.ListBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.cmbPortName = new System.Windows.Forms.ComboBox();
            this.chkStartWithWindows = new System.Windows.Forms.CheckBox();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.btnRefreshComPorts = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnStartService
            // 
            this.btnStartService.Location = new System.Drawing.Point(16, 15);
            this.btnStartService.Margin = new System.Windows.Forms.Padding(4);
            this.btnStartService.Name = "btnStartService";
            this.btnStartService.Size = new System.Drawing.Size(160, 28);
            this.btnStartService.TabIndex = 0;
            this.btnStartService.Text = "Start Service";
            this.btnStartService.UseVisualStyleBackColor = true;
            this.btnStartService.Click += new System.EventHandler(this.btnStartService_Click);
            // 
            // btnStopService
            // 
            this.btnStopService.Location = new System.Drawing.Point(184, 15);
            this.btnStopService.Margin = new System.Windows.Forms.Padding(4);
            this.btnStopService.Name = "btnStopService";
            this.btnStopService.Size = new System.Drawing.Size(160, 28);
            this.btnStopService.TabIndex = 1;
            this.btnStopService.Text = "Stop Service";
            this.btnStopService.UseVisualStyleBackColor = true;
            this.btnStopService.Click += new System.EventHandler(this.btnStopService_Click);
            // 
            // btnAddWorkflow
            // 
            this.btnAddWorkflow.Location = new System.Drawing.Point(16, 209);
            this.btnAddWorkflow.Margin = new System.Windows.Forms.Padding(4);
            this.btnAddWorkflow.Name = "btnAddWorkflow";
            this.btnAddWorkflow.Size = new System.Drawing.Size(160, 28);
            this.btnAddWorkflow.TabIndex = 2;
            this.btnAddWorkflow.Text = "Add Workflow";
            this.btnAddWorkflow.UseVisualStyleBackColor = true;
            // 
            // lblPortName
            // 
            this.lblPortName.AutoSize = true;
            this.lblPortName.Location = new System.Drawing.Point(16, 62);
            this.lblPortName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPortName.Name = "lblPortName";
            this.lblPortName.Size = new System.Drawing.Size(74, 16);
            this.lblPortName.TabIndex = 3;
            this.lblPortName.Text = "Port Name:";
            // 
            // lblBaudRate
            // 
            this.lblBaudRate.AutoSize = true;
            this.lblBaudRate.Location = new System.Drawing.Point(16, 98);
            this.lblBaudRate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblBaudRate.Name = "lblBaudRate";
            this.lblBaudRate.Size = new System.Drawing.Size(74, 16);
            this.lblBaudRate.TabIndex = 4;
            this.lblBaudRate.Text = "Baud Rate:";
            // 
            // lblPrefix
            // 
            this.lblPrefix.AutoSize = true;
            this.lblPrefix.Location = new System.Drawing.Point(16, 135);
            this.lblPrefix.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPrefix.Name = "lblPrefix";
            this.lblPrefix.Size = new System.Drawing.Size(43, 16);
            this.lblPrefix.TabIndex = 5;
            this.lblPrefix.Text = "Prefix:";
            // 
            // lblExcelFile
            // 
            this.lblExcelFile.AutoSize = true;
            this.lblExcelFile.Location = new System.Drawing.Point(16, 172);
            this.lblExcelFile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblExcelFile.Name = "lblExcelFile";
            this.lblExcelFile.Size = new System.Drawing.Size(68, 16);
            this.lblExcelFile.TabIndex = 6;
            this.lblExcelFile.Text = "Excel File:";
            // 
            // txtBaudRate
            // 
            this.txtBaudRate.Location = new System.Drawing.Point(157, 95);
            this.txtBaudRate.Margin = new System.Windows.Forms.Padding(4);
            this.txtBaudRate.Name = "txtBaudRate";
            this.txtBaudRate.Size = new System.Drawing.Size(185, 22);
            this.txtBaudRate.TabIndex = 8;
            this.txtBaudRate.Text = "9600";
            // 
            // txtPrefix
            // 
            this.txtPrefix.Location = new System.Drawing.Point(157, 132);
            this.txtPrefix.Margin = new System.Windows.Forms.Padding(4);
            this.txtPrefix.Name = "txtPrefix";
            this.txtPrefix.Size = new System.Drawing.Size(185, 22);
            this.txtPrefix.TabIndex = 9;
            // 
            // txtExcelFile
            // 
            this.txtExcelFile.Location = new System.Drawing.Point(157, 169);
            this.txtExcelFile.Margin = new System.Windows.Forms.Padding(4);
            this.txtExcelFile.Name = "txtExcelFile";
            this.txtExcelFile.Size = new System.Drawing.Size(185, 22);
            this.txtExcelFile.TabIndex = 10;
            // 
            // btnBrowseExcel
            // 
            this.btnBrowseExcel.Location = new System.Drawing.Point(352, 167);
            this.btnBrowseExcel.Margin = new System.Windows.Forms.Padding(4);
            this.btnBrowseExcel.Name = "btnBrowseExcel";
            this.btnBrowseExcel.Size = new System.Drawing.Size(40, 28);
            this.btnBrowseExcel.TabIndex = 11;
            this.btnBrowseExcel.Text = "...";
            this.btnBrowseExcel.UseVisualStyleBackColor = true;
            // 
            // listWorkflows
            // 
            this.listWorkflows.FormattingEnabled = true;
            this.listWorkflows.ItemHeight = 16;
            this.listWorkflows.Location = new System.Drawing.Point(16, 246);
            this.listWorkflows.Margin = new System.Windows.Forms.Padding(4);
            this.listWorkflows.Name = "listWorkflows";
            this.listWorkflows.Size = new System.Drawing.Size(375, 116);
            this.listWorkflows.TabIndex = 12;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(16, 375);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 16);
            this.lblStatus.TabIndex = 13;
            // 
            // cmbPortName
            // 
            this.cmbPortName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPortName.FormattingEnabled = true;
            this.cmbPortName.Location = new System.Drawing.Point(157, 58);
            this.cmbPortName.Margin = new System.Windows.Forms.Padding(4);
            this.cmbPortName.Name = "cmbPortName";
            this.cmbPortName.Size = new System.Drawing.Size(185, 24);
            this.cmbPortName.TabIndex = 7;
            // 
            // chkStartWithWindows
            // 
            this.chkStartWithWindows.AutoSize = true;
            this.chkStartWithWindows.Location = new System.Drawing.Point(184, 210);
            this.chkStartWithWindows.Margin = new System.Windows.Forms.Padding(4);
            this.chkStartWithWindows.Name = "chkStartWithWindows";
            this.chkStartWithWindows.Size = new System.Drawing.Size(139, 20);
            this.chkStartWithWindows.TabIndex = 14;
            this.chkStartWithWindows.Text = "Start with Windows";
            this.chkStartWithWindows.UseVisualStyleBackColor = true;
            // 
            // lblLanguage
            // 
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Location = new System.Drawing.Point(16, 375);
            this.lblLanguage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(71, 16);
            this.lblLanguage.TabIndex = 15;
            this.lblLanguage.Text = "Language:";
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Items.AddRange(new object[] {
            "English",
            "Deutsch"});
            this.cmbLanguage.Location = new System.Drawing.Point(157, 372);
            this.cmbLanguage.Margin = new System.Windows.Forms.Padding(4);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(185, 24);
            this.cmbLanguage.TabIndex = 16;
            // 
            // btnRefreshComPorts
            // 
            this.btnRefreshComPorts.Text = "⟳";
            this.btnRefreshComPorts.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefreshComPorts.Location = new System.Drawing.Point(352, 58);
            this.btnRefreshComPorts.Margin = new System.Windows.Forms.Padding(4);
            this.btnRefreshComPorts.Name = "btnRefreshComPorts";
            this.btnRefreshComPorts.Size = new System.Drawing.Size(40, 28);
            this.btnRefreshComPorts.TabIndex = 17;
            this.btnRefreshComPorts.UseVisualStyleBackColor = true;
            this.btnRefreshComPorts.Click += new System.EventHandler(this.btnRefreshComPorts_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 414);
            this.Controls.Add(this.btnRefreshComPorts);
            this.Controls.Add(this.cmbLanguage);
            this.Controls.Add(this.lblLanguage);
            this.Controls.Add(this.chkStartWithWindows);
            this.Controls.Add(this.cmbPortName);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.listWorkflows);
            this.Controls.Add(this.btnBrowseExcel);
            this.Controls.Add(this.txtExcelFile);
            this.Controls.Add(this.txtPrefix);
            this.Controls.Add(this.txtBaudRate);
            this.Controls.Add(this.lblExcelFile);
            this.Controls.Add(this.lblPrefix);
            this.Controls.Add(this.lblBaudRate);
            this.Controls.Add(this.lblPortName);
            this.Controls.Add(this.btnAddWorkflow);
            this.Controls.Add(this.btnStopService);
            this.Controls.Add(this.btnStartService);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "QR Scanner Service";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnStartService;
        private System.Windows.Forms.Button btnStopService;
        private System.Windows.Forms.Button btnAddWorkflow;
        private System.Windows.Forms.Label lblPortName;
        private System.Windows.Forms.Label lblBaudRate;
        private System.Windows.Forms.Label lblPrefix;
        private System.Windows.Forms.Label lblExcelFile;
        private System.Windows.Forms.TextBox txtBaudRate;
        private System.Windows.Forms.TextBox txtPrefix;
        private System.Windows.Forms.TextBox txtExcelFile;
        private System.Windows.Forms.Button btnBrowseExcel;
        private System.Windows.Forms.ListBox listWorkflows;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ComboBox cmbPortName;
        private System.Windows.Forms.CheckBox chkStartWithWindows;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.Button btnRefreshComPorts;
    }
}