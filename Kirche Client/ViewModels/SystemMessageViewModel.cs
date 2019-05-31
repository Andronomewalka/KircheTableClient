using Kirche_Client.Models;
using Kirche_Client.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kirche_Client.ViewModels
{
    public class SystemMessageViewModel : ViewModelBase
    {
        private string internalSystemMessage = null;
        public string InternalSystemMessage
        {
            get => internalSystemMessage;
            set => SetProperty(ref internalSystemMessage, value);
        }

        private string externalSystemMessage = null;
        public string ExternalSystemMessage
        {
            get => externalSystemMessage;
            set => SetProperty(ref externalSystemMessage, value);
        }
        
        public SystemMessageViewModel()
        {
            closeFlyoutCommand = new DelegateCommand(CloseFlyoutCommandExecute, CloseFlyoutCommandCanExecute);
            SystemMessageModel.SystemMessageChanged += SystemMessageModel_SystemMessageChanged;
        }

        private void SystemMessageModel_SystemMessageChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "InternalMessage":
                    InternalSystemMessage = SystemMessageModel.InternalMessage;
                    break;

                case "ExternalMessage":
                    ExternalSystemMessage = SystemMessageModel.ExternalMessage;
                    break;
            }
        }


        #region Commands

        #region CloseFlyoutCommand
        private DelegateCommand closeFlyoutCommand;
        public ICommand CloseFlyoutCommand => closeFlyoutCommand;

        private bool CloseFlyoutCommandCanExecute(object arg)
        {
            return true;
        }

        private void CloseFlyoutCommandExecute(object obj)
        {
            switch ((string)obj)
            {
                case "Internal":
                    InternalSystemMessage = null;
                    break;

                case "External":
                    ExternalSystemMessage = null;
                    break;
            }
        }
        #endregion

        #endregion
    }
}
