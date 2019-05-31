using Kirche_Client.Utility;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Data;
using TransitPackage;

namespace Kirche_Client.Models
{
    class MainModel
    {
        public static Client Client { get; private set; }
        public static ComboSource ComboSource { get; private set; }

        public static ListCollectionView ElemsView { get; private set; }

        public static event CustomEvent ModelChanged;
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
            ModelChanged?.Invoke();
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

        public delegate void CustomEvent();
    }
}
