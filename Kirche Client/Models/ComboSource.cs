using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitPackage;

namespace Kirche_Client.Models
{
    public class ComboSource
    {
        public ObservableCollection<Item> ProjectTypeCombo { get; set; }
        public ObservableCollection<string> ChurchDistrictCombo { get; set; }
        public ObservableCollection<string> SignFilterCombo { get; set; }

        public ComboSource(CategoryCollections collection)
        {
            ProjectTypeCombo = new ObservableCollection<Item>();
            ChurchDistrictCombo = new ObservableCollection<string>(collection.ChurchDistrict);
            SignFilterCombo = new ObservableCollection<string>();
            FillOtheCombos(collection);
        }

        private void FillOtheCombos(CategoryCollections collection)
        {
            foreach (var item in collection.ProjectType)
                ProjectTypeCombo.Add(new Item(item));

            SignFilterCombo.Add("=");
            SignFilterCombo.Add(">");
            SignFilterCombo.Add(">=");
            SignFilterCombo.Add("<");
            SignFilterCombo.Add("<=");
        }
    }
}
