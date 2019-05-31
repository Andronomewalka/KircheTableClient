using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Kirche_Client.Models
{
    public static class AccentModel
    {
        public static string MainColor { get; set; } = "Steel";
        public static string EditColor { get; set; } = "Orange";

        public static void SetMainAccent()
        {
            Application.Current.Dispatcher
                .BeginInvoke(new Action<Application, Accent, AppTheme>(ThemeManager.ChangeAppStyle),
                Application.Current, 
                ThemeManager.GetAccent(MainColor), 
                ThemeManager.GetAppTheme("BaseLight"));
        }
        public static void SetEditAccent()
        {
            Application.Current.Dispatcher
                .BeginInvoke(new Action<Application, Accent, AppTheme>(ThemeManager.ChangeAppStyle),
                Application.Current, 
                ThemeManager.GetAccent(EditColor),
                ThemeManager.GetAppTheme("BaseLight"));
        }
    }
}
