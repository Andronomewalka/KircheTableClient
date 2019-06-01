using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitPackage;
using Kirche_Client.Models;
using System.ComponentModel;
using System.Windows.Data;
using Kirche_Client.Utility;
using System.Windows.Input;
using ControlzEx;
using MahApps;
using MahApps.Metro.Controls;
using MahApps.Metro;

namespace Kirche_Client.ViewModels.MainTab
{
    public class MainTabViewModel : Utility.ViewModelBase
    {
        private ListCollectionView elemsView;
        public ListCollectionView ElemsView
        {
            get => elemsView;
            set => SetProperty(ref elemsView, value);
        }

        private ComboSource comboSource;
        public ComboSource ComboSource
        {
            get => comboSource;
            set => SetProperty(ref comboSource, value);
        }

        private ObservableCollection<ColumnVisibility> columnsVisibilityList;
        public ObservableCollection<ColumnVisibility> ColumnsVisibilityList
        {
            get => columnsVisibilityList;
            set => SetProperty(ref columnsVisibilityList, value);
        }

        private bool searchButtonChecked;
        public bool SearchButtonChecked
        {
            get => searchButtonChecked;
            set => SetProperty(ref searchButtonChecked, value);
        }
        
        public FilterViewModel Filter { get; private set; }

        public MainTabViewModel()
        {
            ElemsView = MainModel.ElemsView;
            ElemsView.SortDescriptions.Add(new SortDescription("Church_District", ListSortDirection.Ascending));
            MainModel.ModelChanged += MainModel_ModelChanged;
            ComboSource = MainModel.ComboSource;
            Filter = new FilterViewModel(ElemsView);
            ColumnsVisibilityList = DefineColumnsList();
            searchButtonCommand = new DelegateCommand(SearchButtonCommandExecute, SearchButtonCommandCanExecute);
            colVisibilityItemCommand = new DelegateCommand(ColVisibilityItemCommandExecute, ColVisibilityItemCommandCanExecute);
        }

        private void MainModel_ModelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ElemsView")
            {
                ElemsView = MainModel.ElemsView;
                Filter.ParentCollection = ElemsView;
            }
            else if (e.PropertyName == "ComboSource")
                ComboSource = MainModel.ComboSource;
        }

        private ObservableCollection<ColumnVisibility> DefineColumnsList()
        {
            ObservableCollection<ColumnVisibility> res = new ObservableCollection<ColumnVisibility>();
            res.Add(new ColumnVisibility() { IsVisible = true, Name = "Id" });
            res.Add(new ColumnVisibility() { IsVisible = true, Name = "Church District" });
            res.Add(new ColumnVisibility() { IsVisible = true, Name = "Church" });
            res.Add(new ColumnVisibility() { IsVisible = true, Name = "Project" });
            res.Add(new ColumnVisibility() { IsVisible = true, Name = "Year Start" });
            res.Add(new ColumnVisibility() { IsVisible = true, Name = "Year End" });
            res.Add(new ColumnVisibility() { IsVisible = true, Name = "Price" });
            res.Add(new ColumnVisibility() { IsVisible = true, Name = "Description" });
            return res;
        }

        #region Commands

        #region SearchButtonCommand
        private DelegateCommand searchButtonCommand;
        public ICommand SearchButtonCommand => searchButtonCommand;

        private bool SearchButtonCommandCanExecute(object arg)
        {
            return true;
        }

        private void SearchButtonCommandExecute(object obj)
        {
            if (SearchButtonChecked == false)
                Filter.ResetFilter();
        }
        #endregion

        #region ColVisibilityItemCommand
        private DelegateCommand colVisibilityItemCommand;
        public ICommand ColVisibilityItemCommand => colVisibilityItemCommand;

        private bool ColVisibilityItemCommandCanExecute(object arg)
        {
            return true;
        }

        private void ColVisibilityItemCommandExecute(object obj)
        {
            string curCollumn = obj as string;
            switch (curCollumn)
            {
                case "Id":
                    Filter.IdFilterText = "";
                    break;

                case "Church District":
                    Filter.DistrictFilterText = "";
                    break;

                case "Church":
                    Filter.ChurchFilterText = "";
                    break;

                case "Project":
                    Filter.ProjectFilterText = "";
                    break;

                case "Year Start":
                    Filter.YearStartFilterText = "";
                    break;

                case "Year End":
                    Filter.YearEndFilterText = "";
                    break;

                case "Price":
                    Filter.PriceFilterText = "";
                    break;

                case "Description":
                    Filter.DescriptionFilterText = "";
                    break;
            }
        }
        #endregion

        #endregion
    }
}


