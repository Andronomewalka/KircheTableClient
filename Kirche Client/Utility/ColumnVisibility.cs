using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirche_Client.Utility
{
    public class ColumnVisibility : ViewModelBase
    {
        private bool isVisible;
        public bool IsVisible
        {
            get => isVisible;
            set => SetProperty(ref isVisible, value);
        }

        private string name;
        public string Name
        {
            get => name; 
            set => SetProperty(ref name, value);
        }
    }
}
