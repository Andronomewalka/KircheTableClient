using Kirche_Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kirche_Client.Utility.ChangeLog
{
    public class ChangeLog
    {
        public ObservableCollection<KircheElemEditable> Collection { get; set; }
        Stack<IStep> steps;

        public ChangeLog(ObservableCollection<KircheElemEditable> collection)
        {
            steps = new Stack<IStep>();
            Collection = collection;
        }

        public void Record(IStep step)
        {
            steps.Push(step);
        }

        public void MakeStep()
        {
            steps.Pop().MakeStep(Collection);
        }

        public void Clear()
        {
            steps.Clear();
        }

        public bool Any()
        {
            return steps.Count > 0;
        }

        public IStep Last()
        {
            return steps.Peek();
        }
    }
}
