using Kirche_Client.Models;
using Kirche_Client.Utility;
using Kirche_Client.ViewModels.EditTab;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kirche_Client.ViewModels
{
    public class TabControlViewModel : ViewModelBase
    {
        public TabControlViewModel() { }

        public TabItem SelectedTab
        {
            set
            {
                if ((string)value?.Header == "Main Tab")
                    AccentModel.SetMainAccent();
                else
                    StartCancelViewModel.ChangeAccentColor();
            }
        }
    }
}
