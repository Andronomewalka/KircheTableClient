using Kirche_Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirche_Client.Utility.ChangeLog
{
    public interface IStep
    {
        void MakeStep(ObservableCollection<KircheElemEditable> collection);
    }
}
