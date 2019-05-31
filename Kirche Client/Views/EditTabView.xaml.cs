using Kirche_Client.ViewModels;
using Kirche_Client.ViewModels.EditTab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kirche_Client.Views
{
    /// <summary>
    /// Логика взаимодействия для EditableTabView.xaml
    /// </summary>
    public partial class EditTabView : UserControl
    {
        public EditTabView()
        {
            InitializeComponent();

            DataContext = new EditTabViewModel();
        }
    }
}
