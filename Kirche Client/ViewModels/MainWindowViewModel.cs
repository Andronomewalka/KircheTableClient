using Kirche_Client.Models;
using Kirche_Client.Utility;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Kirche_Client.Models.Client;

namespace Kirche_Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string title = "NordKirche";
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private bool loggedIn = false;
        public bool LoggedIn
        {
            get => loggedIn;
            set
            {
                SetProperty(ref loggedIn, value);
                reLoginCommand.InvokeCanExecuteChanged();
            }
        }

        private string connectionState;
        public string ConnectionState
        {
            get => connectionState;
            set => SetProperty(ref connectionState, value);
        }

        public SystemMessageViewModel SystemMessageViewModel { get; private set; }
        public MainWindowViewModel()
        {
            SystemMessageViewModel = new SystemMessageViewModel();
            reLoginCommand = new DelegateCommand(ReLoginCommandExecute, ReLoginCommandCanExecute);
            reconnectCommand = new DelegateCommand(ReconnectCommandExecute, ReconnectCommandCanExecute);
            MainModel.Client.PropertyChanged += Client_PropertyChanged;
            LoginViewModel.LoggedInNotification += LoginViewModel_LoggedInNotification;
            Connect();
        }

        private async void Connect()
        {
            await Task.Run(() => MainModel.Client.Connect());
            await Task.Run(() => MainModel.SetComboSource());
        }

        private void LoginViewModel_LoggedInNotification(object sender, LoginEventArgs e)
        {
            LoggedIn = true;
            if (e.Mode == LoginMode.online)
                Title = "NordKirche - " + MainModel.Client.District;
            else
                Title = "NordKirche - Offline";
        }

        private void Client_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ConnectionState cur = MainModel.Client.ConnectionState;

            if (cur == Models.ConnectionState.Connecting)
                ConnectionState = "Connecting";

            else if (cur == Models.ConnectionState.Reconnecting)
                ConnectionState = "Reconnecting";

            else if (cur == Models.ConnectionState.Connected)
            {
                ConnectionState = "Connected";
                AccentModel.MainColor = "Blue";
                AccentModel.SetMainAccent();
            }

            else if (cur == Models.ConnectionState.Disconnected)
            {
                ConnectionState = "Disconnected";
                AccentModel.MainColor = "Steel";
                AccentModel.SetMainAccent();
            }

            Application.Current.Dispatcher
                .BeginInvoke(new Action(reconnectCommand.InvokeCanExecuteChanged));
        }

        #region Commands

        #region ReLoginCommand
        private DelegateCommand reLoginCommand;
        public ICommand ReLoginCommand => reLoginCommand;

        private bool ReLoginCommandCanExecute(object arg)
        {
            return LoggedIn;
        }

        private async void ReLoginCommandExecute(object obj)
        {
            await Task.Run(() => MainModel.Client.LogoutActionRequest());
            await Task.Run(() => MainModel.Client.Connect());
            LoggedIn = false;
        }
        #endregion

        #region ReconnectCommand
        private DelegateCommand reconnectCommand;
        public ICommand ReconnectCommand => reconnectCommand;

        private bool ReconnectCommandCanExecute(object arg)
        {
            return ConnectionState == "Disconnected";
        }

        private async void ReconnectCommandExecute(object obj)
        {
            Connect();
            if (MainModel.Client.District != null)
                await Task.Run(() =>
                MainModel.Client.LoginActionRequest(MainModel.Client.District, MainModel.Client.Key));
        }
        #endregion

        #endregion
    }
}
