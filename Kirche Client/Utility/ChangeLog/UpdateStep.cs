using Kirche_Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirche_Client.Utility.ChangeLog
{
    public class UpdateStep : IStep
    {
        int id;
        string property;
        object value;

        public UpdateStep(int id, string property, object value)
        {
            this.id = id;
            this.property = property;
            this.value = value;
        }

        public void MakeStep(ObservableCollection<KircheElemEditable> collection)
        {
            KircheElemEditable cur = collection.First(elem => elem.Id == id);
            UpdateElem(cur);
        }

        private void UpdateElem(KircheElemEditable elem)
        {

            switch (property)
            {
                case "Church":
                    elem.Church = (string)value;
                    break;

                case "Project_Type":
                    elem.Project_Type = (BindingListLog<Item>)value;
                    break;

                case "Year_Start":
                    elem.Year_Start = (DateTime?)value;
                    break;

                case "Year_End":
                    elem.Year_End = (DateTime?)value;
                    break;

                case "Price":
                    elem.Price = (int?)value;
                    break;

                case "Description":
                    elem.Description = (string)value;
                    break;
            }
        }
    }
}
