using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Resources;
using QRScannerService_GUI.Forms;
using System.Configuration;

namespace QRScannerService_GUI.Helpers
{
    public static class LanguageManager
    {
        // Available languages
        public enum Language
        {
            English,
            German
        }

        // Set the application language
        public static void SetLanguage(Language language)
        {
            CultureInfo culture = GetCultureInfo(language);
            Thread.CurrentThread.CurrentUICulture = culture;

            // Save the language preference
            Properties.Settings.Default.Language = (int)language;
            Properties.Settings.Default.Save();
        }

        // Get the current language
        public static Language GetCurrentLanguage()
        {
            try
            {
                // Try to get the language setting
                var languageSetting = Properties.Settings.Default.Language;
                return (Language)languageSetting;
            }
            catch (SettingsPropertyNotFoundException)
            {
                // If the setting doesn't exist, return English as default
                return Language.English;
            }
            catch (Exception)
            {
                // For any other error, return English as default
                return Language.English;
            }
        }

        // Get culture info for the specified language
        private static CultureInfo GetCultureInfo(Language language)
        {
            switch (language)
            {
                case Language.German:
                    return new CultureInfo("de-DE");
                case Language.English:
                default:
                    return new CultureInfo("en-US");
            }
        }

        // Apply language to a form
        public static void ApplyLanguageToForm(Form form)
        {
            // Force the form to refresh its resources
            ComponentResourceManager resources = new ComponentResourceManager(form.GetType());
            resources.ApplyResources(form, "$this");
            ApplyResourcesToControls(form.Controls, resources);
        }

        // Apply resources to all controls recursively
        private static void ApplyResourcesToControls(Control.ControlCollection controls, ComponentResourceManager resources)
        {
            foreach (Control control in controls)
            {
                resources.ApplyResources(control, control.Name);
                if (control.Controls.Count > 0)
                {
                    ApplyResourcesToControls(control.Controls, resources);
                }
            }
        }

        // Update the UI text based on the current language
        public static void UpdateUIText(MainForm form)
        {
            bool isGerman = Thread.CurrentThread.CurrentUICulture.Name.StartsWith("de", StringComparison.OrdinalIgnoreCase);

            // Update form title
            form.Text = isGerman ? "QR Scanner Dienst - Bereit" : "QR Scanner Service";

            // Update buttons - access controls directly
            if (form.Controls["btnStartService"] is Button btnStartService)
                btnStartService.Text = isGerman ? "Dienst starten" : "Start Service";

            if (form.Controls["btnStopService"] is Button btnStopService)
                btnStopService.Text = isGerman ? "Dienst stoppen" : "Stop Service";

            if (form.Controls["btnAddWorkflow"] is Button btnAddWorkflow)
                btnAddWorkflow.Text = isGerman ? "Workflow hinzufügen" : "Add Workflow";

            if (form.Controls["btnBrowseExcel"] is Button btnBrowseExcel)
                btnBrowseExcel.Text = "...";

            // Update labels - access controls directly
            if (form.Controls["lblPortName"] is Label lblPortName)
                lblPortName.Text = isGerman ? "Port Name:" : "Port Name:";

            if (form.Controls["lblBaudRate"] is Label lblBaudRate)
                lblBaudRate.Text = isGerman ? "Baudrate:" : "Baud Rate:";

            if (form.Controls["lblPrefix"] is Label lblPrefix)
                lblPrefix.Text = isGerman ? "Präfix:" : "Prefix:";

            if (form.Controls["lblExcelFile"] is Label lblExcelFile)
                lblExcelFile.Text = isGerman ? "Excel-Datei:" : "Excel File:";

            if (form.Controls["lblLanguage"] is Label lblLanguage)
                lblLanguage.Text = isGerman ? "Sprache:" : "Language:";

            // Update checkbox - access controls directly
            if (form.Controls["chkStartWithWindows"] is CheckBox chkStartWithWindows)
                chkStartWithWindows.Text = isGerman ? "Mit Windows starten" : "Start with Windows";

            // Update tray menu using the public method
            form.UpdateTrayMenuText(isGerman);
        }
    }
}