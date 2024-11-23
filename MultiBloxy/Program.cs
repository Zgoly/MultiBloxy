using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MultiBloxy
{
    public static class DwmHelper
    {
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, uint cbAttribute);

        public static void SetWindowRounded(IntPtr hwnd)
        {
            try
            {
                int cornerPreference = DWMWCP_ROUND;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, (uint)sizeof(int));
            }
            catch
            {
                // Most likely the program was not launched on Windows 11, so ignore any errors
            }
        }
    }

    public class Program
    {
        // Assembly information
        private readonly static string name = Assembly.GetExecutingAssembly().GetName().Name;
        private readonly static string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private readonly static string link = $"https://github.com/Zgoly/{name}/";

        // Mutex name for Roblox
        private readonly static string mutexName = "ROBLOX_singletonEvent";

        // Custom mutex name for app
        private readonly static string appMutexName = $"Global\\{name}_singletonEvent";

        // Mutex objects
        private static Mutex mutex = null;
        private static Mutex appMutex = null;

        // NotifyIcon and ContextMenuStrip for the system tray
        private static NotifyIcon notifyIcon;
        private static ToolStripMenuItem statusMenuItem;
        private static ToolStripMenuItem pauseMenuItem;
        private static ToolStripMenuItem languageMenuItem;
        private static ContextMenuStrip contextMenu;

        // Localization instance for translations
        private static Localization localization;

        private static bool isOpen = false;

        [STAThread]
        private static void Main()
        {
            // Initializes Localization
            localization = new Localization();
            localization.CurrentLocale = Config.Get("Language", localization.CurrentLocale);

            Assembly assembly = Assembly.GetExecutingAssembly();

            // Checks if the application is already running
            appMutex = new Mutex(true, appMutexName, out bool createdNew);

            if (createdNew)
            {
                // Initializes NotifyIcon
                notifyIcon = new NotifyIcon
                {
                    Text = name,
                    Visible = true
                };

                // Initializes ContextMenuStrip
                contextMenu = new ContextMenuStrip();

                // Version
                ToolStripMenuItem versionMenuItem = new ToolStripMenuItem($"{name} {version}")
                {
                    Image = LoadIconFromResource("icon").ToBitmap()
                };
                versionMenuItem.Click += (sender, e) => Process.Start(link);
                contextMenu.Items.Add(versionMenuItem);

                contextMenu.Items.Add(new ToolStripSeparator());

                // Status
                statusMenuItem = new ToolStripMenuItem
                {
                    Image = LoadImageFromResource("info"),
                    Enabled = false
                };
                contextMenu.Items.Add(statusMenuItem);

                contextMenu.Items.Add(new ToolStripSeparator());

                // Pause
                pauseMenuItem = new ToolStripMenuItem();
                pauseMenuItem.Click += (sender, e) =>
                {
                    ToggleMutex();
                };
                contextMenu.Items.Add(pauseMenuItem);

                // Update info
                UpdateInfo(false);

                // Reload
                ToolStripMenuItem reloadMenuItem = new ToolStripMenuItem
                {
                    Image = LoadImageFromResource("refresh-ccw"),
                    Tag = "ContextMenu.ReloadMenuItem.Reload"
                };
                reloadMenuItem.Click += (sender, e) =>
                {
                    CloseMutex();
                    OpenMutex();
                };
                contextMenu.Items.Add(reloadMenuItem);

                contextMenu.Items.Add(new ToolStripSeparator());

                // Start New Instance
                ToolStripMenuItem startNewInstanceMenuItem = new ToolStripMenuItem
                {
                    Image = LoadImageFromResource("plus"),
                    Tag = "ContextMenu.StartNewInstanceMenuItem.StartNewInstance"
                };
                startNewInstanceMenuItem.Click += (sender, e) => Process.Start("roblox-player:");
                contextMenu.Items.Add(startNewInstanceMenuItem);

                // Stop All Instances
                ToolStripMenuItem stopAllInstancesMenuItem = new ToolStripMenuItem
                {
                    Image = LoadImageFromResource("minus"),
                    Tag = "ContextMenu.StopAllInstancesMenuItem.StopAllInstances"
                };
                stopAllInstancesMenuItem.Click += (sender, e) => StopRobloxInstances();
                contextMenu.Items.Add(stopAllInstancesMenuItem);

                contextMenu.Items.Add(new ToolStripSeparator());

                // Show in Explorer
                ToolStripMenuItem showAppInExplorerMenuItem = new ToolStripMenuItem
                {
                    Image = LoadImageFromResource("folder"),
                    Tag = "ContextMenu.ShowInExplorerMenuItem.ShowInExplorer"
                };
                showAppInExplorerMenuItem.Click += (sender, e) =>
                {
                    Process.Start("explorer.exe", $"/select,\"{Application.ExecutablePath}\"");
                };
                contextMenu.Items.Add(showAppInExplorerMenuItem);

                contextMenu.Items.Add(new ToolStripSeparator());

                // Settings
                ToolStripMenuItem settingsMenuItem = new ToolStripMenuItem
                {
                    Image = LoadImageFromResource("settings"),
                    Tag = "ContextMenu.SettingsMenuItem.Settings"
                };
                contextMenu.Items.Add(settingsMenuItem);

                // Settings -> Pause on Launch
                ToolStripMenuItem pauseOnLaunchMenuItem = new ToolStripMenuItem
                {
                    Image = LoadImageFromResource("pause"),
                    Tag = "ContextMenu.SettingsMenuItem.PauseOnLaunchMenuItem.PauseOnLaunch",
                    Checked = Config.Get("PauseOnLaunch", false)
                };
                pauseOnLaunchMenuItem.Click += (sender, e) =>
                {
                    pauseOnLaunchMenuItem.Checked = !pauseOnLaunchMenuItem.Checked;
                    if (pauseOnLaunchMenuItem.Checked)
                    {
                        Config.Set("PauseOnLaunch", true);
                    }
                    else
                    {
                        Config.Remove("PauseOnLaunch");
                    }
                };
                settingsMenuItem.DropDownItems.Add(pauseOnLaunchMenuItem);

                // Settings -> Reset Remembered
                ToolStripMenuItem resetRememberedMenuItem = new ToolStripMenuItem
                {
                    Image = LoadImageFromResource("list-restart"),
                    Tag = "ContextMenu.SettingsMenuItem.ResetRememberedMenuItem.ResetRemembered"
                };
                resetRememberedMenuItem.Click += (sender, e) =>
                {
                    Config.Remove("MutexErrorAction");
                };
                settingsMenuItem.DropDownItems.Add(resetRememberedMenuItem);

                // Settings -> Language
                languageMenuItem = new ToolStripMenuItem
                {
                    Image = LoadImageFromResource("globe"),
                    Tag = "ContextMenu.SettingsMenuItem.LanguageMenuItem.Language"
                };
                settingsMenuItem.DropDownItems.Add(languageMenuItem);

                // Settings -> Language -> Auto Detect
                ToolStripMenuItem autoDetectLanguageMenuItem = new ToolStripMenuItem
                {
                    Tag = "ContextMenu.SettingsMenuItem.LanguageMenuItem.AutoDetectMenuItem.AutoDetect"
                };
                autoDetectLanguageMenuItem.Click += (sender, e) =>
                {
                    Config.Remove("Language");
                    localization.AutoCurrentLocale();
                    SetLanguageMenuCheckedState(autoDetectLanguageMenuItem);
                    UpdateMenuLocalization();
                };
                if (!Config.Has("Language"))
                {
                    autoDetectLanguageMenuItem.Checked = true;
                }
                languageMenuItem.DropDownItems.Add(autoDetectLanguageMenuItem);

                languageMenuItem.DropDownItems.Add(new ToolStripSeparator());

                // Settings -> Language -> (Locale)
                foreach (var locale in localization.Locales.Keys)
                {
                    ToolStripMenuItem localeMenuItem = new ToolStripMenuItem
                    {
                        Text = localization.GetStylizedLocaleName(locale)
                    };
                    localeMenuItem.Click += (sender, e) =>
                    {
                        Config.Set("Language", locale);
                        localization.CurrentLocale = locale;
                        SetLanguageMenuCheckedState(localeMenuItem);
                        UpdateMenuLocalization();
                    };
                    if (Config.Has("Language") && localization.CurrentLocale == locale)
                    {
                        localeMenuItem.Checked = true;
                    }
                    languageMenuItem.DropDownItems.Add(localeMenuItem);
                }

                languageMenuItem.DropDown.Closing += CancelContextMenuClosing;
                DwmHelper.SetWindowRounded(languageMenuItem.DropDown.Handle);

                contextMenu.Items.Add(new ToolStripSeparator());

                settingsMenuItem.DropDown.Closing += CancelContextMenuClosing;
                DwmHelper.SetWindowRounded(settingsMenuItem.DropDown.Handle);

                // Exit
                ToolStripMenuItem exitMenuItem = new ToolStripMenuItem
                {
                    Image = LoadImageFromResource("x"),
                    Tag = "ContextMenu.ExitMenuItem.Exit"
                };
                exitMenuItem.Click += (sender, e) =>
                {
                    Application.Exit();
                };
                contextMenu.Items.Add(exitMenuItem);

                notifyIcon.ContextMenuStrip = contextMenu;
                notifyIcon.MouseClick += (sender, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        ToggleMutex();
                    }
                };

                contextMenu.Closing += CancelContextMenuClosing;
                DwmHelper.SetWindowRounded(contextMenu.Handle);

                // Opens the mutex
                if (!pauseOnLaunchMenuItem.Checked)
                {
                    OpenMutex();
                }

                UpdateMenuLocalization();

                // Runs the application
                Application.Run();

                appMutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show(
                    string.Format(localization.GetLocaleString("Error.Singleton.Message"), name),
                    localization.GetLocaleString("Error.Singleton.Caption"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // Retrieves a resource stream from the assembly
        private static Stream GetResourceStream(string resourceName, string extension)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Resources.{resourceName}.{extension}");
        }

        // Loads an image from a resource
        private static Image LoadImageFromResource(string resourceName)
        {
            using (Stream imageStream = GetResourceStream(resourceName, "png"))
            {
                return imageStream != null ? Image.FromStream(imageStream) : new Bitmap(1, 1);
            }
        }

        // Loads an icon from a resource
        private static Icon LoadIconFromResource(string resourceName)
        {
            using (Stream iconStream = GetResourceStream(resourceName, "ico"))
            {
                return iconStream != null ? new Icon(iconStream) : SystemIcons.Error;
            }
        }

        // Prevents the ContextMenuStrip from closing when an item is clicked
        private static void CancelContextMenuClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                e.Cancel = true;
            }
        }

        // Updates the localization of the menu items
        private static void UpdateMenuLocalization()
        {
            foreach (ToolStripItem menuItem in contextMenu.Items)
            {
                UpdateMenuItemLocalization(menuItem as ToolStripMenuItem);
            }
        }

        // Updates the localization of a single menu item and its sub-items
        private static void UpdateMenuItemLocalization(ToolStripMenuItem menuItem)
        {
            if (menuItem != null && menuItem.Tag != null)
            {
                menuItem.Text = localization.GetLocaleString(menuItem.Tag.ToString());
            }

            if (menuItem != null && menuItem.HasDropDownItems)
            {
                foreach (ToolStripItem subMenuItem in menuItem.DropDownItems)
                {
                    UpdateMenuItemLocalization(subMenuItem as ToolStripMenuItem);
                }
            }
        }

        // Sets the checked state of the language menu items
        private static void SetLanguageMenuCheckedState(ToolStripMenuItem checkedMenuItem)
        {
            foreach (ToolStripItem item in languageMenuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem menuItem)
                {
                    menuItem.Checked = (menuItem == checkedMenuItem);
                }
            }
        }

        // Updates the UI based on the mutex state
        private static void UpdateInfo(bool _isOpen)
        {
            isOpen = _isOpen;
            notifyIcon.Icon = LoadIconFromResource(isOpen ? "icon" : "icon-disabled");
            pauseMenuItem.Image = LoadImageFromResource(isOpen ? "pause" : "play");
            pauseMenuItem.Tag = isOpen ? "ContextMenu.PauseMenuItem.Pause" : "ContextMenu.PauseMenuItem.Resume";
            statusMenuItem.Tag = isOpen ? "ContextMenu.StatusMenuItem.Running" : "ContextMenu.StatusMenuItem.Paused";
        }

        // Stops all Roblox instances
        private static void StopRobloxInstances()
        {
            foreach (var process in Process.GetProcessesByName("RobloxPlayerBeta"))
            {
                process.Kill();
            }
        }

        // Shows a dialog to handle mutex errors
        private static void ShowMutexErrorDialog()
        {
            string rememberedAction = Config.Get<string>("MutexErrorAction");

            // Exit the method if the remembered action was successfully handled
            if (PerformAction(rememberedAction)) return;

            Form form = new Form
            {
                Text = localization.GetLocaleString("Error.Mutex.Caption"),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                Icon = SystemIcons.Error
            };

            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10)
            };

            int currentRow = 0;

            void AddControlToNextRow(Control control)
            {
                tableLayoutPanel.Controls.Add(control, 0, currentRow);
                currentRow++;
                tableLayoutPanel.RowCount = currentRow;
            }

            var label = new Label
            {
                Text = string.Format(localization.GetLocaleString("Error.Mutex.Message"), name),
                AutoSize = true,
                MaximumSize = new Size(380, 0),
                Dock = DockStyle.Fill
            };
            AddControlToNextRow(label);

            RadioButton fixRadioButton = new RadioButton
            {
                Text = localization.GetLocaleString("Error.Mutex.Action.Fix"),
                AutoSize = true,
                Dock = DockStyle.Fill,
                Checked = true
            };
            AddControlToNextRow(fixRadioButton);

            RadioButton abortRadioButton = new RadioButton
            {
                Text = localization.GetLocaleString("Error.Mutex.Action.Abort"),
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            AddControlToNextRow(abortRadioButton);

            RadioButton retryRadioButton = new RadioButton
            {
                Text = localization.GetLocaleString("Error.Mutex.Action.Retry"),
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            AddControlToNextRow(retryRadioButton);

            RadioButton ignoreRadioButton = new RadioButton
            {
                Text = localization.GetLocaleString("Error.Mutex.Action.Ignore"),
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            AddControlToNextRow(ignoreRadioButton);

            CheckBox rememberCheckBox = new CheckBox
            {
                Text = localization.GetLocaleString("Error.Mutex.Action.Remember"),
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            AddControlToNextRow(rememberCheckBox);

            Button confirmButton = new Button
            {
                Text = localization.GetLocaleString("Error.Mutex.Action.Confirm"),
                Dock = DockStyle.Fill
            };
            confirmButton.Click += (sender, e) =>
            {
                HandleUserAction(fixRadioButton, abortRadioButton, retryRadioButton, ignoreRadioButton, rememberCheckBox);
                form.Dispose();
            };
            AddControlToNextRow(confirmButton);

            form.Controls.Add(tableLayoutPanel);

            SystemSounds.Beep.Play();
            form.ShowDialog();
        }

        // Handles the user action selected in the mutex error dialog
        private static void HandleUserAction(RadioButton fixRadioButton, RadioButton abortRadioButton, RadioButton retryRadioButton, RadioButton ignoreRadioButton, CheckBox rememberCheckBox)
        {
            string selectedAction = "";
            if (fixRadioButton.Checked) selectedAction = "Fix";
            else if (abortRadioButton.Checked) selectedAction = "Abort";
            else if (retryRadioButton.Checked) selectedAction = "Retry";
            else if (ignoreRadioButton.Checked) selectedAction = "Ignore";

            PerformAction(selectedAction);

            if (rememberCheckBox.Checked)
            {
                Config.Set("MutexErrorAction", selectedAction);
            }
        }

        // Performs the action based on the given action string
        private static bool PerformAction(string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                return false;
            }

            switch (action)
            {
                case "Fix":
                    HandleCloser.CloseAllHandles();
                    OpenMutex();
                    return true;
                case "Abort":
                    StopRobloxInstances();
                    Thread.Sleep(500);
                    OpenMutex();
                    return true;
                case "Retry":
                    OpenMutex();
                    return true;
                case "Ignore":
                    // Handle ignore case
                    return true;
                default:
                    return false;
            }
        }

        // Opens the mutex and updates the UI
        private static void OpenMutex()
        {
            try
            {
                mutex = new Mutex(false, mutexName);
                UpdateInfo(true);
            }
            catch
            {
                statusMenuItem.Tag = "ContextMenu.StatusMenuItem.Error";
                ShowMutexErrorDialog();
            }

            UpdateMenuLocalization();
        }

        // Closes the mutex and updates the UI
        private static void CloseMutex()
        {
            UpdateInfo(false);
            UpdateMenuLocalization();

            if (mutex != null)
            {
                if (mutex.WaitOne(0))
                {
                    mutex.ReleaseMutex();
                }
                mutex.Close();
                mutex = null;
            }
        }

        // Toggles the mutex state between open and closed
        private static void ToggleMutex()
        {
            if (isOpen)
            {
                CloseMutex();
            }
            else
            {
                OpenMutex();
            }
        }
    }
}