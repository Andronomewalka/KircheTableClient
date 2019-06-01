using Kirche_Client.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TransitPackage;

namespace Kirche_Client.ViewModels.MainTab
{
    public class FilterViewModel : ViewModelBase
    {
        private ListCollectionView parentCollection;
        public ListCollectionView ParentCollection
        {
            private get => parentCollection;
            set
            {
                parentCollection = value;
                parentCollection.Filter += FilterMethod;
            }
        }
        private KircheElem filterElem;

        public FilterViewModel(ListCollectionView collection)
        {
            filterElem = new KircheElem();
            ParentCollection = collection;
            ParentCollection.Filter += FilterMethod;
        }

        public void ResetFilter()
        {
            filterElem = new KircheElem();
            IdFilterText = "";
            DistrictFilterText = "";
            ChurchFilterText = "";
            ProjectFilterText = "";
            YearStartFilterText = "";
            YearEndFilterText = "";
            PriceFilterText = "";
            PriceSignFilterText = "=";
            YearStartSignFilterText = "=";
            YearEndSignFilterText = "=";
            DescriptionFilterText = "";
        }



        private string idFilterText;
        public string IdFilterText
        {
            get => idFilterText;
            set
            {
                SetProperty(ref idFilterText, value);
                FilterChanged("IdFilterText", value);
            }
        }

        private string districtFilterText;
        public string DistrictFilterText
        {
            get => districtFilterText;
            set
            {
                SetProperty(ref districtFilterText, value);
                FilterChanged("DistrictFilterText", value);
            }
        }

        private string churchFilterText;
        public string ChurchFilterText
        {
            get => churchFilterText;
            set
            {
                SetProperty(ref churchFilterText, value);
                FilterChanged("ChurchFilterText", value);
            }
        }

        private string projectFilterText;
        public string ProjectFilterText
        {
            get => projectFilterText;
            set
            {
                SetProperty(ref projectFilterText, value);
                FilterChanged("ProjectFilterText", value);
            }
        }

        private string yearStartFilterText;
        public string YearStartFilterText
        {
            get => yearStartFilterText;
            set
            {
                SetProperty(ref yearStartFilterText, value);
                FilterChanged("YearStartFilterText", value);
            }
        }

        private string yearStartSignFilterText = "=";
        public string YearStartSignFilterText
        {
            get => yearStartSignFilterText;
            set
            {
                SetProperty(ref yearStartSignFilterText, value);
                FilterChanged("", value);
            }
        }

        private string yearEndFilterText;
        public string YearEndFilterText
        {
            get => yearEndFilterText;
            set
            {
                SetProperty(ref yearEndFilterText, value);
                FilterChanged("YearEndFilterText", value);
            }
        }

        private string yearEndSignFilterText = "=";
        public string YearEndSignFilterText
        {
            get => yearEndSignFilterText;
            set
            {
                SetProperty(ref yearEndSignFilterText, value);
                FilterChanged("", value);
            }
        }

        private string priceFilterText;
        public string PriceFilterText
        {
            get => priceFilterText;
            set
            {
                SetProperty(ref priceFilterText, value);
                FilterChanged("PriceFilterText", value);
            }
        }

        private string priceSignFilterText = "=";
        public string PriceSignFilterText
        {
            get => priceSignFilterText;
            set
            {
                SetProperty(ref priceSignFilterText, value);
                FilterChanged("", value);
            }
        }

        private string descriptionFilterText;
        public string DescriptionFilterText
        {
            get => descriptionFilterText;
            set
            {
                SetProperty(ref descriptionFilterText, value);
                FilterChanged("DescriptionFilterText", value);
            }
        }

        private void FilterChanged(string propertyName, string value)
        {
            switch (propertyName)
            {
                case "IdFilterText":
                    int res;
                    if (int.TryParse(value, out res))
                        filterElem.Id = res;
                    else
                        filterElem.Id = 0;
                    break;

                case "DistrictFilterText":
                    if (!string.IsNullOrWhiteSpace(value))
                        filterElem.Church_District = value.ToLower();
                    else
                        filterElem.Church_District = null;
                    break;

                case "ChurchFilterText":
                    if (!string.IsNullOrWhiteSpace(value))
                        filterElem.Church = value.ToLower();
                    else
                        filterElem.Church = null;
                    break;

                case "ProjectFilterText":
                    filterElem.Project_Type.Clear();
                    if (!string.IsNullOrWhiteSpace(value))
                        filterElem.Project_Type.Add(value.ToLower());
                    break;

                case "YearStartFilterText":
                    DateTime dateRes;
                    if (DateTime.TryParse(value, out dateRes))
                        filterElem.Year_Start = dateRes;
                    else
                        filterElem.Year_Start = null;
                    break;

                case "YearEndFilterText":
                    if (DateTime.TryParse(value, out dateRes))
                        filterElem.Year_End = dateRes;
                    else
                        filterElem.Year_End = null;
                    break;

                case "PriceFilterText":
                    if (int.TryParse(value, out res))
                        filterElem.Price = res;
                    else
                        filterElem.Price = null;
                    break;

                case "DescriptionFilterText":
                    if (!string.IsNullOrWhiteSpace(value))
                        filterElem.Description = value.ToLower();
                    else
                        filterElem.Description = null;
                    break;
            }

            ParentCollection.Refresh();
        }

        private bool FilterMethod(object sender)
        {
            KircheElem cur = sender as KircheElem;

            if (filterElem.Id != 0 && !cur.Id.ToString().StartsWith(filterElem.Id.ToString()))
                return false;

            if (!string.IsNullOrWhiteSpace(filterElem.Church_District) &&
                !(cur.Church_District == null ? false : cur.Church_District.ToLower().Contains(filterElem.Church_District)))
                return false;

            if (!string.IsNullOrWhiteSpace(filterElem.Church) &&
                    !cur.Church.ToLower().StartsWith(filterElem.Church))
                return false;

            if (filterElem.Project_Type.Count != 0)
            {
                if (cur.Project_Type.Count == 0)
                    return false;

                for (int i = 0; i < cur.Project_Type.Count; i++)
                {
                    if (cur.Project_Type[i].ToLower().StartsWith(filterElem.Project_Type[0]))
                        break;

                    else if (i == cur.Project_Type.Count - 1)
                        return false;
                }
            }

            if (filterElem.Year_Start != null &&
                    !YearSatisfiesFilter(filterElem.Year_Start, cur.Year_Start, YearStartSignFilterText))
                return false;

            if (filterElem.Year_End != null &&
                    !YearSatisfiesFilter(filterElem.Year_End, cur.Year_End, YearEndSignFilterText))
                return false;

            if (filterElem.Price != null &&
                    !PriceSatisfiesFilter(filterElem.Price, cur.Price, PriceSignFilterText))
                return false;

            if (!string.IsNullOrWhiteSpace(filterElem.Description) &&
                    !(cur.Description == null ? false : cur.Description.ToLower().Contains(filterElem.Description)))
                return false;

            return true;
        }

        private bool PriceSatisfiesFilter(int? condition, int? value, string sign)
        {
            if (sign == "=")
                return condition == value;
            else if (sign == ">")
                return value > condition;
            else if (sign == ">=")
                return value >= condition;
            else if (sign == "<")
                return value < condition;
            else if (sign == "<=")
                return value <= condition;

            return false;
        }

        private bool YearSatisfiesFilter(DateTime? condition, DateTime? value, string sign)
        {
            if (sign == "=")
                return condition == value;
            else if (sign == ">")
                return value > condition;
            else if (sign == ">=")
                return value >= condition;
            else if (sign == "<")
                return value < condition;
            else if (sign == "<=")
                return value <= condition;

            return false;
        }
    }
}
