using Kirche_Client.Models;
using Kirche_Client.Utility;
using Kirche_Client.Views;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TransitPackage;

namespace Kirche_Client.ViewModels.EditTab
{
    class NewProjectDialogViewModel : Utility.ViewModelBase
    {
        private string newProjectTitle = "New project for\n" + MainModel.Client.District;

        public string NewProjectTitle
        {
            get => newProjectTitle;
            set => SetProperty(ref newProjectTitle, value);
        }

        public Item SelectedProjectType { get; set; }

        public ComboSource ComboSource { get; private set; }

        private KircheElemEditable newProjectElem;
        public KircheElemEditable NewProjectElem
        {
            get => newProjectElem;
            set => SetProperty(ref newProjectElem, value);
        }

        public NewProjectDialogViewModel()
        {
            NewProjectElem = new KircheElemEditable();
            NewProjectElem.AfterValidation += NewProjectElem_AfterValidation;
            cancelCommand = new DelegateCommand(CancelCommandExecute, CancelCommandCanExecute);
            acceptCommand = new DelegateCommand(AcceptCommandExecute, AcceptCommandCanExecute);
            removeComboItemCommand = new DelegateCommand(RemoveComboItemCommandExecute, RemoveComboItemCommandCanExecute);
            addComboItemCommand = new DelegateCommand(AddComboItemCommandExecute, AddComboItemCommandCanExecute);
            ComboSource = MainModel.ComboSource;
        }

        private int DefineNewProjectId()
        {
            if (MainModel.ElemsView
                    .Cast<KircheElem>()
                    .Where(elem => elem.Church_District == MainModel.Client.District).Count() == 0)
                return 1;

            else
                return MainModel.ElemsView
                    .Cast<KircheElem>()
                    .Where(elem => elem.Church_District == MainModel.Client.District)
                    .Max(elem => elem.Id) + 1;
        }

        private void NewProjectElem_AfterValidation(object sender, EventArgs e)
        {
            acceptCommand.InvokeCanExecuteChanged();
        }

        #region Commands

        #region CancelCommand
        private DelegateCommand cancelCommand;
        public ICommand CancelCommand => cancelCommand;

        private bool CancelCommandCanExecute(object arg)
        {
            return true;
        }

        public void CancelCommandExecute(object obj)
        {
            NewProjectElem = null;
            DialogManager.HideMetroDialogAsync(MainWindow.GetMainWindow, (BaseMetroDialog)obj);
        }
        #endregion

        #region AcceptCommand
        private DelegateCommand acceptCommand;
        public ICommand AcceptCommand => acceptCommand;

        private bool AcceptCommandCanExecute(object arg)
        {
            return NewProjectElem != null ? NewProjectElem.IsValid : false;
        }

        private async void AcceptCommandExecute(object obj)
        {
            NewProjectElem.Id = DefineNewProjectId();
            NewProjectElem.Church_District = MainModel.Client.District;

            await DialogManager.HideMetroDialogAsync(MainWindow.GetMainWindow, (BaseMetroDialog)obj);
        }
        #endregion

        #region RemoveComboItemCommand
        private DelegateCommand removeComboItemCommand;
        public ICommand RemoveComboItemCommand => removeComboItemCommand;

        private bool RemoveComboItemCommandCanExecute(object arg)
        {
            return true;
        }

        private void RemoveComboItemCommandExecute(object obj)
        {
            NewProjectElem.Project_Type.RemoveItem(SelectedProjectType);
            addComboItemCommand.InvokeCanExecuteChanged();
        }
        #endregion

        #region AddComboItemCommand
        private DelegateCommand addComboItemCommand;
        public ICommand AddComboItemCommand => addComboItemCommand;

        private bool AddComboItemCommandCanExecute(object arg)
        {
            if (NewProjectElem?.Project_Type?.Count > 4)
                return false;

            return true;
        }

        private void AddComboItemCommandExecute(object obj)
        {
            NewProjectElem.Project_Type.AddItem(new Item(""));
            addComboItemCommand.InvokeCanExecuteChanged();
        }
        #endregion

        #endregion
    }
}