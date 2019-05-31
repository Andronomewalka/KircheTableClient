using Kirche_Client.Models;
using Kirche_Client.Utility;
using Kirche_Client.Utility.ChangeLog;
using Kirche_Client.Views;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TransitPackage;

namespace Kirche_Client.ViewModels.EditTab
{
    class DataGridEditViewModel : ViewModelBase
    {
        private EditTabViewModel parent;

        public DataGridColumn SelectedProperty { get; set; }
        public KircheElemEditable SelectedElem { get; set; }
        public Item SelectedListItemElemProperty { get; set; }


        private ChangeLog undoChangeLog;
        private ChangeLog redoChangeLog;

        bool inUndoCommand;
        bool inRedoCommand;

        public DataGridEditViewModel(EditTabViewModel parent)
        {
            this.parent = parent;
            parent.PropertyChanged += Parent_PropertyChanged;
            foreach (var item in parent.CopyElemsView)
                item.OldPropertyChanged += KircheElem_PropertyChanged;

            undoChangeLog = new ChangeLog(parent.CopyElemsView);
            redoChangeLog = new ChangeLog(parent.CopyElemsView);

            undoCommand = new DelegateCommand(UndoCommandExecute, UndoCommandCanExecute);
            redoCommand = new DelegateCommand(RedoCommandExecute, RedoCommandCanExecute);
            removeComboItemCommand = new DelegateCommand(RemoveComboItemCommandExecute, RemoveComboItemCommandCanExecute);
            addComboItemCommand = new DelegateCommand(AddComboItemCommandExecute, AddComboItemCommandCanExecute);
            newProjectCommand = new DelegateCommand(NewProjectCommandExecute, NewProjectCommandCanExecute);
            removeRowCommand = new DelegateCommand(RemoveRowCommandExecute, RemoveRowCommandCanExecute);
            editTabClosedCommand = new DelegateCommand(EditTabClosedCommandExecute, EditTabClosedCommandCanExecute);
        }

        private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CopyElemsView")
            {
                foreach (var item in parent.CopyElemsView)
                    item.OldPropertyChanged += KircheElem_PropertyChanged;

                undoChangeLog.Collection = parent.CopyElemsView;
                redoChangeLog.Collection = parent.CopyElemsView;

                ClearChangeLogs();
            }
        }


        private void KircheElem_PropertyChanged(int id, PropertyChangedEventArgs e, object value)
        {
            if (inUndoCommand)
                redoChangeLog.Record(new UpdateStep(id, e.PropertyName, value));

            else if (inRedoCommand)
                undoChangeLog.Record(new UpdateStep(id, e.PropertyName, value));

            else
            {
                undoChangeLog.Record(new UpdateStep(id, e.PropertyName, value));
                ResetRedoChangeLog();
                undoCommand.InvokeCanExecuteChanged();
            }
        }

        private void ResetRedoChangeLog()
        {
            redoChangeLog.Clear();
            redoCommand.InvokeCanExecuteChanged();
        }

        public void Rollback()
        {
            object temp = new object();
            while (undoCommand.CanExecute(temp))
                UndoCommand.Execute(temp);

            ResetRedoChangeLog();
        }

        internal void ClearChangeLogs()
        {
            undoChangeLog.Clear();
            redoChangeLog.Clear();

            undoCommand.InvokeCanExecuteChanged();
            redoCommand.InvokeCanExecuteChanged();
        }

        #region Commands

        #region UndoCommand
        private DelegateCommand undoCommand;
        public ICommand UndoCommand => undoCommand;

        private bool UndoCommandCanExecute(object arg)
        {
            return undoChangeLog.Any();
        }

        private void UndoCommandExecute(object obj)
        {
            inUndoCommand = true;

            if (undoChangeLog.Last() is DeleteStep)
                redoChangeLog.Record(new NewStep((DeleteStep)undoChangeLog.Last()));

            else if (undoChangeLog.Last() is NewStep)
                redoChangeLog.Record(new DeleteStep((NewStep)undoChangeLog.Last()));

            undoChangeLog.MakeStep();

            undoCommand.InvokeCanExecuteChanged();
            redoCommand.InvokeCanExecuteChanged();

            inUndoCommand = false;
        }
        #endregion

        #region RedoCommand
        private DelegateCommand redoCommand;
        public ICommand RedoCommand => redoCommand;

        private bool RedoCommandCanExecute(object arg)
        {
            return redoChangeLog.Any();
        }

        private void RedoCommandExecute(object obj)
        {
            inRedoCommand = true;

            if (redoChangeLog.Last() is DeleteStep)
                undoChangeLog.Record(new NewStep((DeleteStep)redoChangeLog.Last()));

            else if (redoChangeLog.Last() is NewStep)
                undoChangeLog.Record(new DeleteStep((NewStep)redoChangeLog.Last()));

            redoChangeLog.MakeStep();

            undoCommand.InvokeCanExecuteChanged();
            redoCommand.InvokeCanExecuteChanged();

            inRedoCommand = false;
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
            switch (SelectedProperty.Header)
            {
                case "Project Type":
                    RemoveProjectType();
                    break;
            }

            undoCommand.InvokeCanExecuteChanged();
            redoCommand.InvokeCanExecuteChanged();
        }

        private void RemoveProjectType()
        {
            parent.CopyElemsView
                 .FirstOrDefault(elem => elem.Id == SelectedElem.Id)
                 .Project_Type.RemoveItem(SelectedListItemElemProperty);
        }

        private void RemoveDistrict()
        {
            parent.CopyElemsView
                .FirstOrDefault(elem => elem.Id == SelectedElem.Id).Church_District = null;
        }
        #endregion

        #region AddComboItemCommand
        private DelegateCommand addComboItemCommand;
        public ICommand AddComboItemCommand => addComboItemCommand;

        private bool AddComboItemCommandCanExecute(object arg)
        {
            return true;
        }

        private void AddComboItemCommandExecute(object obj)
        {
            switch (SelectedProperty.Header)
            {
                case "Project Type":
                    SelectedElem.Project_Type.AddItem(new Item(""));
                    break;
            }

            undoCommand.InvokeCanExecuteChanged();
            redoCommand.InvokeCanExecuteChanged();
        }
        #endregion

        #region NewProjectCommand
        private DelegateCommand newProjectCommand;
        public ICommand NewProjectCommand => newProjectCommand;

        private bool NewProjectCommandCanExecute(object arg)
        {
            return true;
        }

        NewProjectDialogView newProjectDialogView;
        private async void NewProjectCommandExecute(object obj)
        {
            newProjectDialogView = new NewProjectDialogView();
            var newProjectDialogViewModel = new NewProjectDialogViewModel();
            newProjectDialogView.DataContext = newProjectDialogViewModel;
            DialogManager.ShowMetroDialogAsync(MainWindow.GetMainWindow, newProjectDialogView);
            await newProjectDialogView.WaitUntilUnloadedAsync();

            if (newProjectDialogViewModel.NewProjectElem != null)
                RegisterNewElem(newProjectDialogViewModel.NewProjectElem);

            newProjectDialogView = null;
        }

        private void RegisterNewElem(KircheElemEditable newElem)
        {
            undoChangeLog.Record(
                new DeleteStep(newElem, parent.Item_AfterValidation, KircheElem_PropertyChanged));

            parent.CopyElemsView.Add(newElem);
            newElem.AfterValidation += parent.Item_AfterValidation;
            newElem.OldPropertyChanged += KircheElem_PropertyChanged;

            ResetRedoChangeLog();
            undoCommand.InvokeCanExecuteChanged();
        }
        #endregion

        #region RemoveRowCommand
        private DelegateCommand removeRowCommand;
        public ICommand RemoveRowCommand => removeRowCommand;

        private bool RemoveRowCommandCanExecute(object arg)
        {
            return true;
        }

        private void RemoveRowCommandExecute(object obj)
        {

            undoChangeLog.Record(
                new NewStep(SelectedElem, parent.Item_AfterValidation, KircheElem_PropertyChanged));

            SelectedElem.AfterValidation -= parent.Item_AfterValidation;
            SelectedElem.OldPropertyChanged -= KircheElem_PropertyChanged;
            parent.CopyElemsView.Remove(SelectedElem);

            ResetRedoChangeLog();
            undoCommand.InvokeCanExecuteChanged();
        }
        #endregion

        #region EditTabClosedCommand
        private DelegateCommand editTabClosedCommand;
        public ICommand EditTabClosedCommand => editTabClosedCommand;

        private bool EditTabClosedCommandCanExecute(object arg)
        {
            return true;
        }

        private void EditTabClosedCommandExecute(object obj)
        {
            (newProjectDialogView?.DataContext as NewProjectDialogViewModel)
                ?.CancelCommandExecute(newProjectDialogView);
        }
        #endregion

        #endregion
    }
}
