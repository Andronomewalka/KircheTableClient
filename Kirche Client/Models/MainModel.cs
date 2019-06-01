using Kirche_Client.Utility;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Data;
using TransitPackage;

namespace Kirche_Client.Models
{
    class MainModel
    {
        public static Client Client { get; private set; }

        private static ComboSource comboSource;
        public static ComboSource ComboSource
        {
            get => comboSource;
            private set => SetProperty(ref comboSource, value);
        }

        private static ListCollectionView elemsView;
        public static ListCollectionView ElemsView
        {
            get => elemsView;
            private set => SetProperty(ref elemsView, value);
        }

        static MainModel()
        {
            Client = new Client();
        }

        public static void SetElemsViewFromServer()
        {
            ElemsView = new ListCollectionView(Client.GetAllActionRequest());
            SaveLastCopy();
        }

        public static void SetElemsViewFromLastCopy()
        {
            ElemsView = new ListCollectionView(GetLastCopy());
        }

        public static void SetComboSource()
        {
            ComboSource = new ComboSource(Client.GetCategoriesActionRequest());
        }

        private static List<KircheElem> GetLastCopy()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\Last_Copy.dat", FileMode.OpenOrCreate))
            {
                return (List<KircheElem>)formatter.Deserialize(fs);
            }
        }

        public static void SaveLastCopy()
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "\\Last_Copy.dat", FileMode.Create))
            {
                formatter.Serialize(fs,
                    new List<KircheElem>(ElemsView.SourceCollection.Cast<KircheElem>().ToList()));
            }
        }

        public static event PropertyChangedEventHandler ModelChanged;
        private static void SetProperty<T>(ref T field, T newValue, [CallerMemberName]string propertyName = null)
        {
            field = newValue;
            ModelChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
