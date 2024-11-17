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
        private readonly static string name = Assembly.GetExecutingAssembly().GetName().Name;
        private readonly static string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private readonly static string link = $"https://github.com/Zgoly/{name}/";

        // Mutex names for Roblox and the app
        private readonly static string mutexName = "ROBLOX_singletonEvent";
        private readonly static string appMutexName = $"Global\\{name}_singletonEvent";

        // Mutex objects
        private static Mutex mutex = null;
        private static Mutex appMutex = null;

        // NotifyIcon and ContextMenu for the system tray
        private static NotifyIcon notifyIcon;
        private static MenuItem statusMenuItem;
        private static MenuItem pauseMenuItem;

        // Localization instance for translations
        private static Localization localization;

        [STAThread]
        static void Main()
        {
            // Initialize localization
            localization = new Localization();

            // Check if the application is already running
            appMutex = new Mutex(true, appMutexName, out bool createdNew);

            if (createdNew)
            {
                InitializeNotifyIcon();
                InitializeContextMenu();

                // Open the mutex unless paused on launch
                if (!Config.Get<bool>("PauseOnLaunch", false))
                {
                    OpenMutex();
                }

                // Run the application
                Application.Run();
                appMutex.ReleaseMutex();
            }
            else
            {
                ShowSingletonError();
            }
        }

        // Initialize the NotifyIcon and ContextMenu for system tray interaction
        private static void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Text = name,
                Visible = true
            };

            // Set initial icon (paused state)
            SetIcon(false);
        }

        // Initialize the ContextMenu with all menu items
        private static void InitializeContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu();

            // Add version info to the context menu
            MenuItem versionMenuItem = contextMenu.MenuItems.Add($"{name} {version}");
            versionMenuItem.Click += (sender, e) => Process.Start(link);

            contextMenu.MenuItems.Add("-");

            // Status item showing whether the app is running or paused
            statusMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.StatusMenuItem.Paused"));
            statusMenuItem.Enabled = false;

            contextMenu.MenuItems.Add("-");

            // Pause/Resume action
            pauseMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.PauseMenuItem.Resume"));
            pauseMenuItem.Click += (sender, e) => TogglePauseState();

            // Reload action
            MenuItem reloadMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.ReloadMenuItem.Reload"));
            reloadMenuItem.Click += (sender, e) => ReloadMutex();

            contextMenu.MenuItems.Add("-");

            // Start a new Roblox instance
            MenuItem startNewInstanceMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.StartNewInstanceMenuItem.StartNewInstance"));
            startNewInstanceMenuItem.Click += (sender, e) => Process.Start("roblox-player:");

            // Stop all Roblox instances
            MenuItem stopAllInstancesMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.StopAllInstancesMenuItem.StopAllInstances"));
            stopAllInstancesMenuItem.Click += (sender, e) => StopAllInstances();

            contextMenu.MenuItems.Add("-");

            // Show app location in explorer
            MenuItem showAppInExplorerMenuItem = contextMenu.MenuItems.Add(string.Format(localization.GetTranslation("ContextMenu.ShowInExplorerMenuItem.ShowInExplorer"), name));
            showAppInExplorerMenuItem.Click += (sender, e) => ShowAppInExplorer();

            contextMenu.MenuItems.Add("-");

            // Settings menu
            MenuItem settingsMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.SettingsMenuItem.Settings"));

            // Settings items
            MenuItem pauseOnLaunchMenuItem = settingsMenuItem.MenuItems.Add(localization.GetTranslation("ContextMenu.SettingsMenuItem.PauseOnLaunchMenuItem.PauseOnLaunch"));
            pauseOnLaunchMenuItem.Checked = Config.Get("PauseOnLaunch", false);
            pauseOnLaunchMenuItem.Click += (sender, e) => TogglePauseOnLaunch(pauseOnLaunchMenuItem);

            MenuItem resetRememberedMenuItem = settingsMenuItem.MenuItems.Add(localization.GetTranslation("ContextMenu.SettingsMenuItem.ResetRememberedMenuItem.ResetRemembered"));
            resetRememberedMenuItem.Click += (sender, e) => Config.Remove("MutexErrorAction");

            contextMenu.MenuItems.Add("-");

            // Exit action
            MenuItem exitMenuItem = contextMenu.MenuItems.Add(localization.GetTranslation("ContextMenu.ExitMenuItem.Exit"));
            exitMenuItem.Click += (sender, e) => Application.Exit();

            // Assign the context menu to the NotifyIcon
            notifyIcon.ContextMenu = contextMenu;
        }

        // Update the icon based on the mutex state (running or paused)
        private static void SetIcon(bool isOpen)
        {
            Bitmap bitmap = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Custom icon shape and color
                PointF[] points = { new PointF(0, 24), new PointF(7, 0), new PointF(31, 7), new PointF(24, 31) };
                GraphicsPath path = new GraphicsPath();
                path.AddPolygon(points);
                graphics.FillPath(isOpen ? Brushes.DodgerBlue : Brushes.Gray, path);

                using (Pen penInfinity = new Pen(Color.White, 2) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                {
                    graphics.DrawBezier(penInfinity, 7, 15.5f, 11, 4, 20, 27, 24, 15.5f);
                    graphics.DrawBezier(penInfinity, 7, 15.5f, 11, 27, 20, 4, 24, 15.5f);
                }
            }

            notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
        }

        // Start a new Roblox instance
        private static void StopAllInstances()
        {
            foreach (var process in Process.GetProcessesByName("RobloxPlayerBeta"))
            {
                process.Kill();
            }
        }

        // Open the mutex and update UI
        private static void OpenMutex()
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

        // Close the mutex
        private static void CloseMutex()
        {
            pauseMenuItem.Text = localization.GetTranslation("ContextMenu.PauseMenuItem.Resume");
            statusMenuItem.Text = localization.GetTranslation("ContextMenu.StatusMenuItem.Paused");
            SetIcon(false);

            if (mutex != null && mutex.WaitOne(0))
            {
                mutex.ReleaseMutex();
                mutex.Close();
                mutex = null;
            }
        }

        // Show mutex error dialog and handle user actions
        private static void ShowMutexErrorDialog()
        {
            // Handle the mutex error based on remembered actions
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
                        StopAllInstances();
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
                DisplayMutexErrorDialog();
            }
        }

        // Show a dialog for user to resolve mutex error
        private static void DisplayMutexErrorDialog()
        {
            // Code to display the error dialog
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

            MessageBox.Show(localization.GetTranslation("Error.Mutex.Message"));
        }

        // Reload the mutex for new instances
        private static void ReloadMutex()
        {
            CloseMutex();
            OpenMutex();
        }

        // Toggle pause on launch configuration setting
        private static void TogglePauseOnLaunch(MenuItem menuItem)
        {
            bool currentState = menuItem.Checked;
            menuItem.Checked = !currentState;
            Config.Set("PauseOnLaunch", !currentState);
        }

        // Open app folder in File Explorer
        private static void ShowAppInExplorer()
        {
            Process.Start("explorer", $"/select,\"{Assembly.GetExecutingAssembly().Location}\"");
        }

        // Show error message when the application is already running
        private static void ShowSingletonError()
        {
            MessageBox.Show(localization.GetTranslation("Error.Singleton.Message"));
        }

        // Toggle pause/resume state
        private static void TogglePauseState()
        {
            if (mutex == null)
            {
                OpenMutex();
            }
            else
            {
                CloseMutex();
            }
        }
    }
}