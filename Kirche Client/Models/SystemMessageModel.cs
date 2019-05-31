using Kirche_Client.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kirche_Client.Models
{
    public class SystemMessageModel
    {
        private static string internalMessage = null;
        public static string InternalMessage
        {
            get => internalMessage;
            set => SetProperty(ref internalMessage, value);
        }

        private static string externalMessage = null;
        public static string ExternalMessage
        {
            get => externalMessage;
            set => SetProperty(ref externalMessage, value);
        }


        public static event PropertyChangedEventHandler SystemMessageChanged;
        private static void SetProperty(ref string field, string newValue, [CallerMemberName]string propertyName = null)
        {
            field = newValue;
            SystemMessageChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
