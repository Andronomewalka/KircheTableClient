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

        private LoginMode loginMode = LoginMode.LoggedOut;
        public LoginMode LoginMode
        {
            get => loginMode;
            set
            {
                SetProperty(ref loginMode, value);
                TitleChange();
                Application.Current.Dispatcher
               .BeginInvoke(new Action(reLoginCommand.InvokeCanExecuteChanged));
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
            MainModel.Client.LoggedInStatusChanged += Client_LoggedInStatusChanged;
            Connect();
        }

        private void TitleChange()
        {
            if (loginMode == LoginMode.LoggedIn)
                Title = "NordKirche - " + MainModel.Client.District;
            else if (loginMode == LoginMode.Offline)
                Title = "NordKirche - Offline";
            else
                Title = "NordKirche";
        }

        private void Client_LoggedInStatusChanged(object sender, LoginEventArgs e)
        {
            //LoginMode = e.Mode;
            //if (e.Mode == LoginMode.LoggedIn)
            //    Title = "NordKirche - " + MainModel.Client.District;
            //else
            //    Title = "NordKirche - Offline";
        }

        private async Task<bool> Connect()
        {
            bool res = await Task.Run(() => MainModel.Client.Connect());
            await Task.Run(() => MainModel.SetComboSource());
            return res;
        }

        private void Client_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ClientState cur = MainModel.Client.ConnectionState;

            if (cur == Models.ClientState.Connecting)
            {
                ConnectionState = "Connecting";
                AccentModel.MainColor = "Steel";
                AccentModel.SetMainAccent();
            }

            else if (cur == Models.ClientState.Reconnecting)
                ConnectionState = "Reconnecting";

            else if (cur == Models.ClientState.Connected)
            {
                ConnectionState = "Connected";
                AccentModel.MainColor = "Blue";
                AccentModel.SetMainAccent();
            }

            else if (cur == Models.ClientState.Disconnected)
            {
                ConnectionState = "Disconnected";
               // LoginMode = LoginMode.LoggedOut;
                AccentModel.MainColor = "Steel";
                AccentModel.SetMainAccent();
            }

            else if (cur == Models.ClientState.GetAll)
                LoginMode = LoginMode.LoggedIn;

            else if (cur == ClientState.GetAllOffline)
                LoginMode = LoginMode.Offline;

            Application.Current.Dispatcher
                .BeginInvoke(new Action(reconnectCommand.InvokeCanExecuteChanged));
        }

        #region Commands

        #region ReLoginCommand
        private DelegateCommand reLoginCommand;
        public ICommand ReLoginCommand => reLoginCommand;

        private bool ReLoginCommandCanExecute(object arg)
        {
            return LoginMode == LoginMode.LoggedIn || LoginMode == LoginMode.Offline;
        }

        private async void ReLoginCommandExecute(object obj)
        {
            await Task.Run(() => MainModel.Client.LogoutActionRequest());
            LoginMode = LoginMode.LoggedOut;
            await Task.Run(() => MainModel.Client.Connect());
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
            if (await Connect() && MainModel.Client.District != null)
            {
                await Task.Run(() =>
                MainModel.Client.LoginActionRequest(MainModel.Client.District, MainModel.Client.Key));
                await Task.Run(() => MainModel.SetElemsViewFromServer());
                //await Application.Current.Dispatcher.BeginInvoke(
                //    new Action(MainModel.ElemsView.Refresh));
            }
        }
        #endregion

        #endregion
    }
}
