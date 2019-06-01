using Kirche_Client.Models;
using Kirche_Client.Utility;
using MahApps.Metro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TransitPackage;
using static Kirche_Client.Models.Client;

namespace Kirche_Client.ViewModels.EditTab
{
    class StartCancelViewModel : ViewModelBase
    {
        private EditTabViewModel parent;
        private DataGridEditViewModel parentDataGridEdit;

        private bool startEdititngToggled;
        public bool StartEdititngToggled
        {
            get => startEdititngToggled;
            set
            {
                SetProperty(ref startEdititngToggled, value);
                staticStartEdititngToggled = value;
                ChangeAccentColor();
                cancelCommand.InvokeCanExecuteChanged();
            }
        }

        private static bool staticStartEdititngToggled;
        public static void ChangeAccentColor()
        {
            if (staticStartEdititngToggled)
                AccentModel.SetEditAccent();
            else
                AccentModel.SetMainAccent();
        }

        ClientState connectionState;
        List<KircheElem> updatedElems;
        Dictionary<int, string> deletedElems;

        public StartCancelViewModel
            (EditTabViewModel parentEditTab,
            DataGridEditViewModel parentDataGridEdit)
        {
            this.parent = parentEditTab;
            this.parentDataGridEdit = parentDataGridEdit;

            updatedElems = new List<KircheElem>();
            deletedElems = new Dictionary<int, string>();
            cancelCommand = new DelegateCommand(CancelCommandExecute, CancelCommandCanExecute);
            saveCommand = new DelegateCommand(SaveCommandExecute, SaveCommandCanExecute);
            MainModel.Client.PropertyChanged += Client_PropertyChanged;
            connectionState = MainModel.Client.ConnectionState;
            AccentModel.EditColor = "Orange";
        }

        private void Client_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            connectionState = MainModel.Client.ConnectionState;
            if (connectionState == ClientState.Connected
                || connectionState == ClientState.Disconnected)
                Application.Current.Dispatcher
                    .BeginInvoke(new Action(saveCommand.InvokeCanExecuteChanged));
        }


        #region Commands

        #region CancelCommand
        private DelegateCommand cancelCommand;
        public ICommand CancelCommand => cancelCommand;

        private bool CancelCommandCanExecute(object arg)
        {
            return StartEdititngToggled;
        }

        private void CancelCommandExecute(object obj)
        {
            StartEdititngToggled = false;
            parentDataGridEdit.Rollback();
            //MainModel.Client.MainCollectionEditSemaphore.Release();
        }
        #endregion

        #region SaveCommand
        private DelegateCommand saveCommand;
        public ICommand SaveCommand => saveCommand;

        private bool SaveCommandCanExecute(object arg)
        {
            return connectionState != ClientState.Connected ? false : true;
        }

        enum OperationResult { Ok, Bad, NoElems }
        private async void SaveCommandExecute(object obj)
        {
            if (!StartEdititngToggled) // safe editing
            {
                //MainModel.Client.MainCollectionEditSemaphore.Release();
                //Thread.Sleep(100);
                //await MainModel.Client.MainCollectionEditSemaphore.WaitAsync();
                await SaveChanges();
                ClearLogs();
            }
            //else // start edititng
            //    await MainModel.Client.MainCollectionEditSemaphore.WaitAsync();
        }

        #endregion

        #endregion

        private async Task SaveChanges()
        {
            List<KircheElem> MainColelction = MainModel.ElemsView
                .Cast<KircheElem>()
                .Where(elem => elem.Church_District == MainModel.Client.District)
                .ToList();

            DefineChangedElems(MainColelction);

            OperationResult changedResult;
            OperationResult deletedResult;

            changedResult = await AcceptChangedElemsAsync(MainColelction);
            deletedResult = await AcceptDeletedElemsAsync(MainColelction);

            if (changedResult == OperationResult.Ok
                || deletedResult == OperationResult.Ok
                && (changedResult != OperationResult.Bad
                    && deletedResult != OperationResult.Bad))
            {
                SystemMessageModel.InternalMessage = "Successfully sent";
                await Task.Run(() => MainModel.SaveLastCopy());
            }
            else if (changedResult == OperationResult.Bad
                    || deletedResult == OperationResult.Bad)
                SystemMessageModel.InternalMessage = "Error occurred";

        }

        private async Task<OperationResult> AcceptDeletedElemsAsync(List<KircheElem> MainColelction)
        {
            if (deletedElems.Count != 0)
            {
                bool requestResult =
                    await Task.Run(() => MainModel.Client.DeleteSendActionRequest(deletedElems));

                if (requestResult)
                {
                    foreach (var item in deletedElems)
                    {
                        KircheElem cur = null;
                        if (MainColelction.Exists(elem => elem.Id == item.Key))
                        {
                            cur = MainColelction.Find(elem => elem.Id == item.Key);
                            await Application.Current.Dispatcher
                                .BeginInvoke(new Action<object>(MainModel.ElemsView.Remove), cur);
                        }
                    }
                    return OperationResult.Ok;
                }
                return OperationResult.Bad;
            }
            return OperationResult.NoElems;
        }

        private async Task<OperationResult> AcceptChangedElemsAsync(List<KircheElem> MainColelction)
        {
            if (updatedElems.Count != 0)
            {
                bool requestResult =
                   await Task.Run(() => MainModel.Client.UpdateSendActionRequest(updatedElems));

                if (requestResult)
                {
                    foreach (var item in updatedElems)
                    {
                        if (MainColelction.Exists(elem => elem.Id == item.Id))
                            MainColelction.Find(elem => elem.Id == item.Id).ApplyChanges(item);

                        else
                        {
                            KircheElem newElem = new KircheElem();
                            newElem.ApplyChanges(item);
                            await Application.Current.Dispatcher
                                .BeginInvoke(new Func<object, object>(MainModel.ElemsView.AddNewItem), newElem);
                            await Application.Current.Dispatcher
                                .BeginInvoke(new Action(MainModel.ElemsView.CommitNew));
                        }
                    }
                    await Application.Current.Dispatcher
                        .BeginInvoke(new Action(MainModel.ElemsView.Refresh));
                    return OperationResult.Ok;
                }
                return OperationResult.Bad;
            }
            return OperationResult.NoElems;
        }

        private void ClearLogs()
        {
            parentDataGridEdit.ClearChangeLogs();
            updatedElems.Clear();
            deletedElems.Clear();
        }

        private void DefineChangedElems(List<KircheElem> MainColelction)
        {
            foreach (var item in MainColelction)
            {
                try
                {
                    KircheElemEditable match =
                        parent.CopyElemsView.Single(elem => elem.Id == item.Id);

                    if (!item.IsEqual(match))
                        updatedElems.Add(match);

                }
                catch
                {
                    deletedElems.Add(item.Id, item.Church_District);
                }
            }

            int maxMainDistrictCollectionId;

            if (MainColelction.Count == 0)
                maxMainDistrictCollectionId = 0;
            else
                maxMainDistrictCollectionId = MainColelction.Max(elem => elem.Id);

            IEnumerable<KircheElemEditable> neweElems = parent.CopyElemsView.Where(elem => elem.Id > maxMainDistrictCollectionId);
            updatedElems.AddRange(neweElems.Select(x => (KircheElem)x).ToList());
        }
    }
}
