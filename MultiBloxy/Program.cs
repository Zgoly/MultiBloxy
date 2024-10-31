using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace MultiBloxy
{
    public class Program
    {
        // Assembly information
        readonly static string name = Assembly.GetExecutingAssembly().GetName().Name;
        readonly static string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        readonly static string link = $"https://github.com/Zgoly/{name}/";

        // Mutex name for Roblox
        readonly static string mutexName = "ROBLOX_singletonEvent";

        // Custom mutex name for app
        readonly static string appMutexName = $"Global\\{name}_singletonEvent";

        // Mutex objects
        static Mutex mutex = null;
        static Mutex appMutex = null;

        // NotifyIcon and ContextMenu for the system tray
        static NotifyIcon notifyIcon;
        static MenuItem statusMenuItem;
        static MenuItem pauseMenuItem;

        // Localization instance
        static Localization localization;

        [STAThread]
        static void Main()
        {
            // Initialize Localization
            localization = new Localization();

            // Check if the application is already running
            appMutex = new Mutex(true, appMutexName, out bool createdNew);

            if (createdNew)
            {
                // Initialize NotifyIcon
                notifyIcon = new NotifyIcon
                {
                    Text = name,
                    Visible = true
                };

                // Set the initial icon
                SetIcon(false);

                // Initialize ContextMenu
                ContextMenu contextMenu = new ContextMenu();
                MenuItem versionMenuItem = contextMenu.MenuItems.Add($"{name} {version}");
                versionMenuItem.Click += (sender, e) => Process.Start(link);

                contextMenu.MenuItems.Add("-");

                statusMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.StatusMenuItem.Paused"));
                statusMenuItem.Enabled = false;

                contextMenu.MenuItems.Add("-");

                pauseMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.PauseMenuItem.Resume"));
                pauseMenuItem.Click += (sender, e) =>
                {
                    if (pauseMenuItem.Text == localization.GetTranslation("ContextMenu.PauseMenuItem.Pause"))
                    {
                        CloseMutex();
                    }
                    else
                    {
                        OpenMutex();
                    }
                };

                MenuItem reloadMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.ReloadMenuItem.Reload"));
                reloadMenuItem.Click += (sender, e) =>
                {
                    CloseMutex();
                    OpenMutex();
                };

                contextMenu.MenuItems.Add("-");

                MenuItem startNewInstanceMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.StartNewInstanceMenuItem.StartNewInstance"));
                startNewInstanceMenuItem.Click += (sender, e) => Process.Start("roblox-player:");

                MenuItem stopAllInstancesMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.StopAllInstancesMenuItem.StopAllInstances"));
                stopAllInstancesMenuItem.Click += (sender, e) => StopRobloxInstances();

                contextMenu.MenuItems.Add("-");

                MenuItem showAppInExplorerMenuItem = contextMenu.MenuItems.Add(string.Format(localization.GetTranslation("ContextMenu.ShowInExplorerMenuItem.ShowInExplorer"), name));
                showAppInExplorerMenuItem.Click += (sender, e) =>
                {
                    Process.Start("explorer.exe", $"/select,\"{Application.ExecutablePath}\"");
                };

                contextMenu.MenuItems.Add("-");

                MenuItem settingsMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.SettingsMenuItem.Settings"));

                MenuItem pauseOnLaunchMenuItem = settingsMenuItem.MenuItems.Add(localization.GetTranslation("ContextMenu.SettingsMenuItem.PauseOnLaunchMenuItem.PauseOnLaunch"));
                pauseOnLaunchMenuItem.Checked = Config.Get("PauseOnLaunch", false);
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

                MenuItem resetRememberedMenuItem = settingsMenuItem.MenuItems.Add(localization.GetTranslation("ContextMenu.SettingsMenuItem.ResetRememberedMenuItem.ResetRemembered"));
                resetRememberedMenuItem.Click += (sender, e) =>
                {
                    Config.Remove("MutexErrorAction");
                };

                contextMenu.MenuItems.Add("-");

                MenuItem exitMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.ExitMenuItem.Exit"));
                exitMenuItem.Click += (sender, e) =>
                {
                    Application.Exit();
                };

                notifyIcon.ContextMenu = contextMenu;

                // Open the mutex
                if (!pauseOnLaunchMenuItem.Checked)
                {
                    OpenMutex();
                }

                // Run the application
                Application.Run();

                appMutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show(
                    string.Format(localization.GetTranslation("Error.Singleton.Message"), name),
                    localization.GetTranslation("Error.Singleton.Caption"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // Set the icon based on the mutex state
        static void SetIcon(bool isOpen)
        {
            Bitmap bitmap = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                PointF[] points =
                {
                    new PointF(0, 24),
                    new PointF(7, 0),
                    new PointF(31, 7),
                    new PointF(24, 31)
                };
                GraphicsPath path = new GraphicsPath();
                path.AddPolygon(points);
                graphics.FillPath(isOpen ? Brushes.DodgerBlue : Brushes.Gray, path);

                using (Pen penInfinity = new Pen(Color.White, 2)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                })
                {
                    graphics.DrawBezier(penInfinity, 7, 15.5f, 11, 4, 20, 27, 24, 15.5f);
                    graphics.DrawBezier(penInfinity, 7, 15.5f, 11, 27, 20, 4, 24, 15.5f);
                }
            }

            notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
        }

        // Stop all Roblox instances
        static void StopRobloxInstances()
        {
            foreach (var process in Process.GetProcessesByName("RobloxPlayerBeta"))
            {
                process.Kill();
            }
        }

        // Open the mutex
        static void OpenMutex()
        {
            try
            {
                mutex = new Mutex(false, mutexName);
                pauseMenuItem.Text = localization.GetTranslation("ContextMenu.PauseMenuItem.Pause");
                statusMenuItem.Text = localization.GetTranslation("ContextMenu.StatusMenuItem.Running");
                SetIcon(true);
            }
            catch
            {
                statusMenuItem.Text = localization.GetTranslation("ContextMenu.StatusMenuItem.Error");
                ShowMutexErrorDialog();
            }
        }

        // Show the mutex error dialog
        static void ShowMutexErrorDialog()
        {
            string rememberedAction = Config.Get<string>("MutexErrorAction");
            if (!string.IsNullOrEmpty(rememberedAction))
            {
                switch (rememberedAction)
                {
                    case "Fix":
                        HandleCloser.CloseAllHandles();
                        OpenMutex();
                        break;
                    case "Abort":
                        StopRobloxInstances();
                        Thread.Sleep(500);
                        OpenMutex();
                        break;
                    case "Retry":
                        OpenMutex();
                        break;
                }
            }
            else
            {
                Form form = new Form
                {
                    Text = localization.GetTranslation("Error.Mutex.Caption"),
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
                    Text = string.Format(localization.GetTranslation("Error.Mutex.Message"), name),
                    AutoSize = true,
                    MaximumSize = new Size(380, 0),
                    Dock = DockStyle.Fill
                };
                AddControlToNextRow(label);

                RadioButton fixRadioButton = new RadioButton
                {
                    Text = localization.GetTranslation("Error.Mutex.Action.Fix"),
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    Checked = true
                };
                AddControlToNextRow(fixRadioButton);

                RadioButton abortRadioButton = new RadioButton
                {
                    Text = localization.GetTranslation("Error.Mutex.Action.Abort"),
                    AutoSize = true,
                    Dock = DockStyle.Fill
                };
                AddControlToNextRow(abortRadioButton);

                RadioButton retryRadioButton = new RadioButton
                {
                    Text = localization.GetTranslation("Error.Mutex.Action.Retry"),
                    AutoSize = true,
                    Dock = DockStyle.Fill
                };
                AddControlToNextRow(retryRadioButton);

                RadioButton ignoreRadioButton = new RadioButton
                {
                    Text = localization.GetTranslation("Error.Mutex.Action.Ignore"),
                    AutoSize = true,
                    Dock = DockStyle.Fill
                };
                AddControlToNextRow(ignoreRadioButton);

                CheckBox rememberCheckBox = new CheckBox
                {
                    Text = localization.GetTranslation("Error.Mutex.Action.Remember"),
                    AutoSize = true,
                    Dock = DockStyle.Fill
                };
                AddControlToNextRow(rememberCheckBox);

                Button confirmButton = new Button
                {
                    Text = localization.GetTranslation("Error.Mutex.Action.Confirm"),
                    Dock = DockStyle.Fill
                };
                confirmButton.Click += (sender, e) =>
                {
                    if (fixRadioButton.Checked)
                    {
                        HandleCloser.CloseAllHandles();
                        OpenMutex();
                    }
                    else if (abortRadioButton.Checked)
                    {
                        StopRobloxInstances();
                        Thread.Sleep(500);
                        OpenMutex();
                    }
                    else if (retryRadioButton.Checked)
                    {
                        OpenMutex();
                    }

                    if (rememberCheckBox.Checked)
                    {
                        string selectedAction = "";
                        if (fixRadioButton.Checked) selectedAction = "Fix";
                        else if (abortRadioButton.Checked) selectedAction = "Abort";
                        else if (retryRadioButton.Checked) selectedAction = "Retry";
                        else if (ignoreRadioButton.Checked) selectedAction = "Ignore";

                        Config.Set("MutexErrorAction", selectedAction);
                    }

                    form.Dispose();
                };
                AddControlToNextRow(confirmButton);

                form.Controls.Add(tableLayoutPanel);

                SystemSounds.Beep.Play();
                form.ShowDialog();
            }
        }

        // Close the mutex
        static void CloseMutex()
        {
            pauseMenuItem.Text = localization.GetTranslation("ContextMenu.PauseMenuItem.Resume");
            statusMenuItem.Text = localization.GetTranslation("ContextMenu.StatusMenuItem.Paused");
            SetIcon(false);
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
    }
}