using Kirche_Client.Models;
using Kirche_Client.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using static Kirche_Client.Models.Client;

namespace Kirche_Client.ViewModels
{
    class LoginViewModel : ViewModelBase, IDataErrorInfo
    {
        private ComboSource comboSource;
        public ComboSource ComboSource
        {
            get => comboSource;
            set => SetProperty(ref comboSource, value);
        }

        private bool loginPanelEnabled = true;
        public bool LoginPanelEnabled
        {
            get => loginPanelEnabled;
            set => SetProperty(ref loginPanelEnabled, value);
        }

        private bool progressBarState = false;
        public bool ProgressBarState
        {
            get => progressBarState;
            set => SetProperty(ref progressBarState, value);
        }

        private string selectedDistrict;
        public string SelectedDistrict
        {
            get => selectedDistrict;
            set
            {
                selectedDistrict = value;
                loginTryCommand.InvokeCanExecuteChanged();
                this.LaunchValidation("Key");
            }
        }


        private string key;
        public string Key
        {
            get => key;
            set
            {
                SetProperty(ref key, value);
                loginTryCommand.InvokeCanExecuteChanged();
            }
        }

        bool loginOperation = false;
        public LoginViewModel()
        {
            loginTryCommand = new DelegateCommand(LoginTryCommandExecute, LoginTryCommandCanExecute);
            offlineModeCommand = new DelegateCommand(OfflineModeCommandExecute, OfflineModeCommandCanExecute);
            MainModel.Client.PropertyChanged += Client_PropertyChanged;
            MainModel.ModelChanged += MainModel_ModelChanged1;
            ComboSource = MainModel.ComboSource;
            SelectedDistrict = MainModel.Client.District;
            Key = MainModel.Client.Key;
        }

        private void MainModel_ModelChanged1(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ComboSource")
                ComboSource = MainModel.ComboSource;
        }

        private void Client_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (MainModel.Client.ConnectionState == ClientState.GettingAll)
            {
                LoginPanelEnabled = false;
                ProgressBarState = true;
            }

            Application.Current.Dispatcher
                .BeginInvoke(new Action(loginTryCommand.InvokeCanExecuteChanged));
        }

        #region Commands

        #region LoginTryCommand
        private DelegateCommand loginTryCommand;
        public ICommand LoginTryCommand => loginTryCommand;

        private bool LoginTryCommandCanExecute(object arg)
        {
            return SelectedDistrict != null
                && !string.IsNullOrWhiteSpace(key)
                && MainModel.Client.ConnectionState == ClientState.Connected;
        }

        private async void LoginTryCommandExecute(object obj)
        {
            loginOperation = true;

            if (await Task.Run(() => MainModel.Client.LoginActionRequest(selectedDistrict, Key)))
                await Task.Run(() => MainModel.SetElemsViewFromServer());

            else
                LaunchValidation("Key");

            loginOperation = false;
        }
        #endregion

        #region OfflineModeCommand
        private DelegateCommand offlineModeCommand;
        public ICommand OfflineModeCommand => offlineModeCommand;

        private bool OfflineModeCommandCanExecute(object arg)
        {
            return true;
        }

        private async void OfflineModeCommandExecute(object obj)
        {
            MainModel.Client.ConnectionState = ClientState.GettingAll;

            await Task.Run(() =>
                Application.Current.Dispatcher.Invoke(() =>
                MainModel.SetElemsViewFromLastCopy()));

            MainModel.Client.ConnectionState = ClientState.GetAllOffline;
            await Task.Run(() => MainModel.Client.LogoutActionRequest());
        }
        #endregion

        #endregion

        #region Validation
        public string Error => null;
        public string this[string propertyName]
        {
            get
            {
                return loginOperation ? "Wrong key" : null;
            }
        }
        #endregion
    }
}
