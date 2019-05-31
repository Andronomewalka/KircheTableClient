using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kirche_Client.Models;

namespace Kirche_Client.Utility.ChangeLog
{
    public class NewStep : IStep
    {
        public KircheElemEditable NewElem { get; }
        public EventHandler AfterValidationCallback { get; }
        public PropertyChangedEventHandlerLog PropertyChangedCallback { get; }


        public NewStep(KircheElemEditable elem,
            EventHandler afterValidationCallback,
            PropertyChangedEventHandlerLog propertyChangedCallback)
        {
            NewElem = elem;
            AfterValidationCallback = afterValidationCallback;
            PropertyChangedCallback = propertyChangedCallback;
        }

        public NewStep(DeleteStep deleteStep)
        {
            NewElem = deleteStep.DeleteElem;
            AfterValidationCallback = deleteStep.AfterValidationCallback;
            PropertyChangedCallback = deleteStep.PropertyChangedCallback;
        }

        public void MakeStep(ObservableCollection<KircheElemEditable> collection)
        {
            collection.Add(NewElem);
            NewElem.AfterValidation += AfterValidationCallback;
            NewElem.OldPropertyChanged += PropertyChangedCallback;
            collection.Sort(
                (a, b) =>
                {
                    if (a.Id < b.Id)
                        return -1;
                    else if (a.Id > b.Id)
                        return 1;
                    else
                        return 0;
                });
        }
    }

    public static class ObservableCollectionExtension
    {
        public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
        {
            var sortableList = new List<T>(collection);
            sortableList.Sort(comparison);

            for (int i = 0; i < sortableList.Count; i++)
            {
                collection.Move(collection.IndexOf(sortableList[i]), i);
            }
        }
    }
}
