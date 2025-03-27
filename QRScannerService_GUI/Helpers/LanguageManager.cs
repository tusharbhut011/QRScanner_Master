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
            ((Button)form.Controls["btnStartService"]).Text = isGerman ? "Dienst starten" : "Start Service";
            ((Button)form.Controls["btnStopService"]).Text = isGerman ? "Dienst stoppen" : "Stop Service";
            ((Button)form.Controls["btnAddWorkflow"]).Text = isGerman ? "Workflow hinzufügen" : "Add Workflow";
            ((Button)form.Controls["btnBrowseExcel"]).Text = "...";

            // Update labels - access controls directly
            ((Label)form.Controls["lblPortName"]).Text = isGerman ? "Port Name:" : "Port Name:";
            ((Label)form.Controls["lblBaudRate"]).Text = isGerman ? "Baudrate:" : "Baud Rate:";
            ((Label)form.Controls["lblPrefix"]).Text = isGerman ? "Präfix:" : "Prefix:";
            ((Label)form.Controls["lblExcelFile"]).Text = isGerman ? "Excel-Datei:" : "Excel File:";
            ((Label)form.Controls["lblLanguage"]).Text = isGerman ? "Sprache:" : "Language:";

            // Update checkbox - access controls directly
            ((CheckBox)form.Controls["chkStartWithWindows"]).Text = isGerman ? "Mit Windows starten" : "Start with Windows";

            // Update tray menu using the public method
            form.UpdateTrayMenuText(isGerman);
        }
    }
}

