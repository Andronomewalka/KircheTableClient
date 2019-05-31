using Kirche_Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirche_Client.Utility.ChangeLog
{
    public class DeleteStep : IStep
    {

        public KircheElemEditable DeleteElem { get; }
        public EventHandler AfterValidationCallback { get; }
        public PropertyChangedEventHandlerLog PropertyChangedCallback { get; }


        public DeleteStep(KircheElemEditable elem,
            EventHandler afterValidationCallback,
            PropertyChangedEventHandlerLog propertyChangedCallback)
        {
            DeleteElem = elem;
            AfterValidationCallback = afterValidationCallback;
            PropertyChangedCallback = propertyChangedCallback;
        }

        public DeleteStep(NewStep newStep)
        {
            DeleteElem = newStep.NewElem;
            AfterValidationCallback = newStep.AfterValidationCallback;
            PropertyChangedCallback = newStep.PropertyChangedCallback;
        }

        public void MakeStep(ObservableCollection<KircheElemEditable> collection)
        {
            DeleteElem.AfterValidation -= AfterValidationCallback;
            DeleteElem.OldPropertyChanged -= PropertyChangedCallback;
            collection.Remove(DeleteElem);
        }
    }
}
