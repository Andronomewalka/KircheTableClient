using Kirche_Client.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TransitPackage;

namespace Kirche_Client.Models
{
    public class KircheElemEditable : ViewModelBase, IDataErrorInfo
    {
        private int id;
        public int Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        private BindingListLog<Item> project_Type;
        public BindingListLog<Item> Project_Type
        {
            get => project_Type;
            set
            {
                SetOldProperty(ref project_Type, value);

                project_Type.OldListChanged -= Project_Type_ListBeforeChanged;
                project_Type.AddingNew -= Project_Type_AddingNew;
                project_Type.ListChanged -= Project_Type_ListChanged;

                SetProperty(ref project_Type, value);

                project_Type.OldListChanged += Project_Type_ListBeforeChanged;
                project_Type.AddingNew += Project_Type_AddingNew;
                project_Type.ListChanged += Project_Type_ListChanged;
            }
        }

        private string church;
        public string Church
        {
            get => church;
            set
            {
                SetOldProperty(ref church, value);
                SetProperty(ref church, value);
            }
        }

        private string church_District;
        public string Church_District
        {
            get => church_District;
            set => SetProperty(ref church_District, value);
        }

        private DateTime? year_Start;
        public DateTime? Year_Start
        {
            get => year_Start;
            set
            {
                SetOldProperty(ref year_Start, value);
                SetProperty(ref year_Start, value);
            }
        }

        private DateTime? year_End;
        public DateTime? Year_End
        {
            get => year_End;
            set
            {
                SetOldProperty(ref year_End, value);
                SetProperty(ref year_End, value);
            }
        }

        private int? price;
        public int? Price
        {
            get => price;
            set
            {
                SetOldProperty(ref price, value);
                SetProperty(ref price, value);
            }
        }


        private string description;
        public string Description
        {
            get => description;
            set
            {
                SetOldProperty(ref description, value);
                SetProperty(ref description, value);
            }
        }

        public KircheElemEditable()
        {
            project_Type = new BindingListLog<Item>();
            project_Type.OldListChanged += Project_Type_ListBeforeChanged;
            project_Type.AddingNew += Project_Type_AddingNew;
            project_Type.ListChanged += Project_Type_ListChanged;
        }

        public KircheElemEditable(KircheElem obj)
        {
            id = obj.Id;
            project_Type = new BindingListLog<Item>();

            foreach (var item in obj.Project_Type)
                project_Type.AddItem(new Item(item));
            project_Type.OldListChanged += Project_Type_ListBeforeChanged;
            project_Type.AddingNew += Project_Type_AddingNew;
            project_Type.ListChanged += Project_Type_ListChanged;

            church = obj.Church;
            church_District = obj.Church_District;
            year_Start = obj.Year_Start;
            year_End = obj.Year_End;
            price = obj.Price;
            description = obj.Description;
        }

        public void ApplyChanges(KircheElem obj)
        {
            id = obj.Id;
            project_Type = new BindingListLog<Item>();

            foreach (var item in obj.Project_Type)
                project_Type.AddItem(new Item(item));
            project_Type.OldListChanged += Project_Type_ListBeforeChanged;
            project_Type.AddingNew += Project_Type_AddingNew;
            project_Type.ListChanged += Project_Type_ListChanged;

            church = obj.Church;
            church_District = obj.Church_District;
            year_Start = obj.Year_Start;
            year_End = obj.Year_End;
            price = obj.Price;
            description = obj.Description;
        }

        public static implicit operator KircheElem(KircheElemEditable v)
        {
            return new KircheElem()
            {
                Id = v.Id,
                Church_District = v.Church_District,
                Project_Type = new List<string>(v.Project_Type.Select(elem => elem.Name)),
                Church = v.Church,
                Year_Start = v.Year_Start,
                Year_End = v.Year_End,
                Price = v.Price,
                Description = v.Description
            };
        }

        #region Validation

        public List<string> ValidationErrorLog { get; private set; } = new List<string>();
        public event EventHandler AfterValidation;
        public bool IsValid
        {
            get => ValidationErrorLog.Count == 0;
        }

        static readonly string[] ValidatedProperties =
        {
            "Price",
            "Project_Type",
            "Year_Start",
            "Year_End"
        };

        public string Error => null;
        public string this[string propertyName] => GetValidationError(propertyName);

        private string GetValidationError(string propertyName)
        {
            string error = null;
            switch (propertyName)
            {
                case "Price":
                    error = ValidatePrice();
                    break;

                case "Project_Type":
                    error = ValidateProjectType();
                    break;

                case "Year_Start":
                    error = ValidateYearStart();
                    break;

                case "Year_End":
                    error = ValidateYearEnd();
                    break;
            }

            AfterValidation?.Invoke(this, EventArgs.Empty);
            return error;
        }

        private string ValidateYearEnd()
        {
            if (Year_End < Year_Start)
            {
                if (!ValidationErrorLog.Contains("Year_End"))
                    ValidationErrorLog.Add("Year_End");

                return "End year can't be earlier than Start year";
            }

            if (ValidationErrorLog.Contains("Year_Start"))
                this.LaunchValidation("Year_Start");

            ValidationErrorLog.Remove("Year_End");
            return null;
        }

        private string ValidateYearStart()
        {
            if (Year_Start > Year_End)
            {
                if (!ValidationErrorLog.Contains("Year_Start"))
                    ValidationErrorLog.Add("Year_Start");

                return "Start year can't be later than End year";
            }

            if (ValidationErrorLog.Contains("Year_End"))
                this.LaunchValidation("Year_End");

            ValidationErrorLog.Remove("Year_Start");
            return null;
        }

        private string ValidateProjectType()
        {
            IEnumerable<string> uniqueProjects = Project_Type.Select(x => x.Name).Distinct();

            if (uniqueProjects.Count() != Project_Type.Count)
            {
                if (!ValidationErrorLog.Contains("Project_Type"))
                    ValidationErrorLog.Add("Project_Type");

                return "Projects must be unique";
            }

            ValidationErrorLog.Remove("Project_Type");
            return null;
        }

        private string ValidatePrice()
        {
            if (Price < 0)
            {
                if (!ValidationErrorLog.Contains("Price"))
                    ValidationErrorLog.Add("Price");

                return "Price can't be negative";
            }

            ValidationErrorLog.Remove("Price");
            return null;
        }

        #endregion

        #region EventCallBacks

        private void Project_Type_AddingNew(object sender, AddingNewEventArgs e)
        {
            Project_Type_ListBeforeChanged(this, null);
        }

        private void Project_Type_ListChanged(object sender, ListChangedEventArgs e)
        {
            this.LaunchValidation("Project_Type");
        }

        private void Project_Type_ListBeforeChanged(object sender, ListChangedEventArgs e)
        {
            var newProjectType = new BindingListLog<Item>();
            foreach (var item in project_Type)
                newProjectType.AddItem(new Item(item.Name));

            SetOldProperty(ref newProjectType, null, "Project_Type");
        }

        public event PropertyChangedEventHandlerLog OldPropertyChanged;
        private void SetOldProperty<T>(ref T field, T newValue, [CallerMemberName]string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
                OldPropertyChanged?.Invoke(Id, new PropertyChangedEventArgs(propertyName), field);
        }

        #endregion
    }

    public delegate void PropertyChangedEventHandlerLog(int id, PropertyChangedEventArgs e, object value);

    public class Item : Utility.ViewModelBase, IEquatable<Item>
    {
        private string name;
        public event EventHandler OldValueChanged;
        public string Name
        {
            get => name;
            set
            {
                OldValueChanged.Invoke(this, EventArgs.Empty);
                SetProperty(ref name, value);
            }
        }

        public Item(string name)
        {
            this.name = name;
        }

        public Item()
        {
            name = "";
        }

        public bool Equals(Item other)
        {
            return Name == other.Name;
        }

        public override string ToString()
        {
            return name;
        }
    }

    public class BindingListLog<T> : BindingList<T>
    {
        public event ListChangedEventHandler OldListChanged;

        public void AddItem(T item)
        {
            (item as Item).OldValueChanged += Item_OldValueChanged;
            OnAddingNew(new AddingNewEventArgs(item));
            Add(item);
        }
        public void RemoveItem(T item)
        {
            Item_OldValueChanged(this, null);
            Remove(item);
        }

        private void Item_OldValueChanged(object sender, EventArgs e)
        {
            OldListChanged?.Invoke(this, null);
        }
    }



    public static class KircheElemExtension
    {
        public static void ApplyChanges(this KircheElem mainObj, KircheElemEditable editObj)
        {
            mainObj.Id = editObj.Id;
            mainObj.Church_District = editObj.Church_District;
            mainObj.Project_Type = new List<string>(editObj.Project_Type.Select(elem => elem.Name));
            mainObj.Church = editObj.Church;
            mainObj.Year_Start = editObj.Year_Start;
            mainObj.Year_End = editObj.Year_End;
            mainObj.Price = editObj.Price;
            mainObj.Description = editObj.Description;
        }

        public static void ApplyChanges(this KircheElem mainObj, KircheElem editObj)
        {
            mainObj.Id = editObj.Id;
            mainObj.Church_District = editObj.Church_District;
            mainObj.Project_Type = new List<string>(editObj.Project_Type.Select(elem => elem));
            mainObj.Church = editObj.Church;
            mainObj.Year_Start = editObj.Year_Start;
            mainObj.Year_End = editObj.Year_End;
            mainObj.Price = editObj.Price;
            mainObj.Description = editObj.Description;
        }

        public static bool IsEqual(this KircheElem mainObj, KircheElemEditable editObj)
        {
            if (mainObj.Church != editObj.Church
                || mainObj.Description != editObj.Description
                || mainObj.Price != editObj.Price
                || mainObj.Year_End != editObj.Year_End
                || mainObj.Year_Start != editObj.Year_Start
                || !ProjectTypeIsEqual(mainObj.Project_Type, editObj.Project_Type))
            {
                return false;
            }
            return true;

            bool ProjectTypeIsEqual(List<string> first, BindingListLog<Item> second)
            {
                if (first.Count != second.Count)
                    return false;

                for (int i = 0; i < first.Count; i++)
                    if (first[i] != second[i].Name)
                        return false;

                return true;
            }
        }
    }
}
