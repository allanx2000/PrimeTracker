using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.IO;
using System.Windows.Input;

namespace PrimeTracker
{
    internal class SettingsWindowViewModel : Innouvous.Utils.Merged45.MVVM45.ViewModel
    {
        private SettingsWindow settingsWindow;

        private readonly Properties.Settings settings = AppContext.Settings;

        public SettingsWindowViewModel(SettingsWindow settingsWindow)
        {
            this.settingsWindow = settingsWindow;

            LoadSettings();
        }

        private void LoadSettings()
        {
            Username = settings.Username;
            DbPath = settings.DbPath;
            HideBrowser = settings.HideChrome;

            settingsWindow.PasswordBox.Password = settings.Password;
        }

        public string Username
        {
            get { return Get<string>(); }
            set
            {
                Set(value);
                RaisePropertyChanged();
            }
        }

        public string DbPath {
            get { return Get<string>(); }
            set
            {
                Set(value);
                RaisePropertyChanged();
            }
        }

        public bool HideBrowser {
            get { return Get<bool>(); }
            set
            {
                Set(value);
                RaisePropertyChanged();
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new CommandHelper(() => settingsWindow.Close());
            }
        }

        public ICommand SetCommand
        {
            get { return new CommandHelper(SaveSettings); }
        }

        public bool Changed { get; private set; }

        private void SaveSettings()
        {
            try
            {
                if (string.IsNullOrEmpty(DbPath) || !Path.IsPathRooted(DbPath))
                    throw new Exception("DB Path is empty or not valid.");

                settings.DbPath = DbPath;
                settings.Username = Username;
                settings.Password = settingsWindow.PasswordBox.Password;
                settings.HideChrome = HideBrowser;

                settings.Save();

                Changed = true;

                settingsWindow.Close();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }
    }
}