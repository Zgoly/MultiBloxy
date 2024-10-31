using System.Collections.Generic;
using System.Globalization;

namespace MultiBloxy
{
    public class Localization
    {
        private readonly Dictionary<string, Dictionary<string, string>> _translations;
        private string _currentLocale;

        public Localization()
        {
            _translations = new Dictionary<string, Dictionary<string, string>>();
            _currentLocale = CultureInfo.CurrentCulture.Name;
            LoadTranslations();
        }

        private void LoadTranslations()
        {
            _translations["en"] = new Dictionary<string, string>
            {
                { "ContextMenu.StatusMenuItem.Running", "Status: Running" },
                { "ContextMenu.StatusMenuItem.Paused", "Status: Paused" },
                { "ContextMenu.StatusMenuItem.Error", "Status: Error creating Mutex" },
                { "ContextMenu.PauseMenuItem.Pause", "Pause" },
                { "ContextMenu.PauseMenuItem.Resume", "Resume" },
                { "ContextMenu.ReloadMenuItem.Reload", "Reload" },
                { "ContextMenu.StartNewInstanceMenuItem.StartNewInstance", "Start New Roblox Instance" },
                { "ContextMenu.StopAllInstancesMenuItem.StopAllInstances", "Stop All Roblox Instances" },
                { "ContextMenu.ShowInExplorerMenuItem.ShowInExplorer", "Show In Explorer" },
                { "ContextMenu.SettingsMenuItem.Settings", "Settings" },
                { "ContextMenu.SettingsMenuItem.PauseOnLaunchMenuItem.PauseOnLaunch", "Pause on Launch" },
                { "ContextMenu.SettingsMenuItem.ResetRememberedMenuItem.ResetRemembered", "Reset Remembered Options" },
                { "ContextMenu.ExitMenuItem.Exit", "Exit" },
                { "Error.Mutex.Caption", "Failed to Create Mutex" },
                { "Error.Mutex.Message", "An error occurred while creating the Mutex. This likely happened because when {0} was launched, Roblox was already running and had registered its handle. You can do the following:" },
                { "Error.Mutex.Action.Fix", "Close the handle for all instances of Roblox" },
                { "Error.Mutex.Action.Abort", "Stop all Roblox instances" },
                { "Error.Mutex.Action.Retry", "Try again" },
                { "Error.Mutex.Action.Ignore", "Ignore the error and continue" },
                { "Error.Mutex.Action.Remember", "Remember this choice" },
                { "Error.Mutex.Action.Confirm", "Confirm" },
                { "Error.Singleton.Caption", "Singleton Error" },
                { "Error.Singleton.Message", "{0} is already running. Try looking in the system tray." }
            };

            _translations["ru"] = new Dictionary<string, string>
            {
                { "ContextMenu.StatusMenuItem.Running", "Статус: Работает" },
                { "ContextMenu.StatusMenuItem.Paused", "Статус: Приостановлено" },
                { "ContextMenu.StatusMenuItem.Error", "Статус: Ошибка создания Mutex" },
                { "ContextMenu.PauseMenuItem.Pause", "Приостановить" },
                { "ContextMenu.PauseMenuItem.Resume", "Возобновить" },
                { "ContextMenu.ReloadMenuItem.Reload", "Перезагрузить" },
                { "ContextMenu.StartNewInstanceMenuItem.StartNewInstance", "Запустить новый экземпляр Roblox" },
                { "ContextMenu.StopAllInstancesMenuItem.StopAllInstances", "Закрыть все экземпляры Roblox" },
                { "ContextMenu.ShowInExplorerMenuItem.ShowInExplorer", "Показать в проводнике" },
                { "ContextMenu.SettingsMenuItem.Settings", "Настройки" },
                { "ContextMenu.SettingsMenuItem.PauseOnLaunchMenuItem.PauseOnLaunch", "Приостановить при запуске" },
                { "ContextMenu.SettingsMenuItem.ResetRememberedMenuItem.ResetRemembered", "Сбросить запомненные параметры" },
                { "ContextMenu.ExitMenuItem.Exit", "Выход" },
                { "Error.Mutex.Caption", "Не удалось создать Mutex" },
                { "Error.Mutex.Message", "Произошла ошибка при создании Mutex. Скорее всего, это связано с тем, что при запуске {0} Roblox уже был запущен и успел зарегистрировать свой дескриптор. Вы можете сделать следующее:" },
                { "Error.Mutex.Action.Fix", "Закрыть дескриптор для всех экземпляров Roblox" },
                { "Error.Mutex.Action.Abort", "Закрыть все экземпляры Roblox" },
                { "Error.Mutex.Action.Retry", "Попробовать снова" },
                { "Error.Mutex.Action.Ignore", "Игнорировать ошибку и продолжить" },
                { "Error.Mutex.Action.Remember", "Запомнить этот выбор" },
                { "Error.Mutex.Action.Confirm", "Подтвердить" },
                { "Error.Singleton.Caption", "Ошибка одиночного экземпляра" },
                { "Error.Singleton.Message", "{0} уже запущен. Попробуйте поискать в области уведомлений." }
            };
        }

        public string GetTranslation(string key)
        {
            string locale = GetLocaleWithoutRegion(_currentLocale);

            if (_translations.ContainsKey(locale) && _translations[locale].ContainsKey(key))
            {
                return _translations[locale][key];
            }

            // Fallback to English if the translation is not found
            if (_translations.ContainsKey("en") && _translations["en"].ContainsKey(key))
            {
                return _translations["en"][key];
            }

            // Fallback to the key if the translation is not found
            return key;
        }

        private string GetLocaleWithoutRegion(string locale)
        {
            int index = locale.IndexOf('-');
            if (index != -1)
            {
                return locale.Substring(0, index);
            }
            return locale;
        }
    }
}
