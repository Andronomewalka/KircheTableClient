using Kirche_Client.Models;
using Kirche_Client.Utility;
using Kirche_Client.Views;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TransitPackage;

namespace Kirche_Client.ViewModels.EditTab
{
    class EditTabViewModel : Utility.ViewModelBase
    {
        private ObservableCollection<KircheElemEditable> copyElemsView = null;
        public ObservableCollection<KircheElemEditable> CopyElemsView
        {
            get => copyElemsView;
            set => SetProperty(ref copyElemsView, value);
        }
        public ComboSource ComboSource { get; private set; }
        public DataGridEditViewModel Editing { get; private set; }
        public StartCancelViewModel StartCancel { get; private set; }

        private bool isCollectionValid = true;
        public bool IsCollectionValid
        {
            get => isCollectionValid;
            set => SetProperty(ref isCollectionValid, value);
        }

        public EditTabViewModel()
        {
            CopyElemsView = SetCopyElemsView();

            foreach (var item in CopyElemsView)
                item.AfterValidation += Item_AfterValidation;

            ComboSource = MainModel.ComboSource;
            Editing = new DataGridEditViewModel(this);
            StartCancel = new StartCancelViewModel(this, Editing);
          //  MainModel.Client.SameDistrictUpdateReceive += Client_SameDistrictUpdateReceive;
        }

        public void Item_AfterValidation(object sender, EventArgs e)
        {
            foreach (var item in CopyElemsView)
                if (!item.IsValid)
                {
                    IsCollectionValid = false;
                    return;
                }

            IsCollectionValid = true;
        }


        //private void Client_SameDistrictUpdateReceive()
        //{
        //    CopyElemsView = SetCopyElemsView();
        //
        //    foreach (var item in CopyElemsView)
        //        item.AfterValidation += Item_AfterValidation;
        //}

        public ObservableCollection<KircheElemEditable> SetCopyElemsView()
        {
            var res = new ObservableCollection<KircheElemEditable>();

            foreach (var item in MainModel.ElemsView.SourceCollection)
                if ((item as KircheElem).Church_District == MainModel.Client.District)
                    res.Add(new KircheElemEditable((KircheElem)item));

            return res;
        }
    }
}
