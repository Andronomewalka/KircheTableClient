using Kirche_Client.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TransitPackage;

namespace Kirche_Client.Models
{
    public enum ConnectionState
    {
        Connecting, Connected, Reconnecting, Disconnected, GetData
    }
    public class Client : ViewModelBase
    {
        private ConnectionState connectionState;
        public ConnectionState ConnectionState
        {
            get => connectionState;
            set => SetProperty(ref connectionState, value);
        }

        public string District { get; private set; }
        public string Key { get; private set; }
       // public SemaphoreSlim MainCollectionEditSemaphore { get; private set; }
       // public event Action SameDistrictUpdateReceive;
        TcpClient tcpClient;
        NetworkStream stream;
        Message message;
        bool streamIsOpen;
        DispatcherTimer connectionCheckTimer;
        Mutex NetworkStreamMutex; // флаг, разрешающий чтение в потоке ожидания обновления данных 
                                  // (иначе он может вмешиваться в работу NetworkStream при реквестах)

        public Client()
        {
            District = null;
            NetworkStreamMutex = new Mutex();
          //  MainCollectionEditSemaphore = new SemaphoreSlim(1);
            ConnectionState = ConnectionState.Disconnected;
            connectionCheckTimer = new DispatcherTimer();
            connectionCheckTimer.Tick += ConnectionCheckActionRequest;
            connectionCheckTimer.Interval = new TimeSpan(0, 0, 5);
        }

        public void StartListening()
        {
            Task.Run(() => FromServerReceiveTask());
        }

        private void FromServerReceiveTask()
        {
            while (streamIsOpen)
            {
                NetworkStreamMutex.WaitOne();
                if (streamIsOpen && stream.DataAvailable)
                {
                    ActionEnum request = message.ReadRequest();
                    Process(request);
                }
                NetworkStreamMutex.ReleaseMutex();
                Thread.Sleep(100);
            }
        }

        private void Process(ActionEnum action)
        {
            switch (action)
            {
                case ActionEnum.update_receive:
                    UpdateReceiveAction();
                    break;
                case ActionEnum.del_receive:
                    DeleteReceiveAction();
                    break;
            }
        }

        private async void DeleteReceiveAction()
        {
            SystemMessageModel.ExternalMessage = "External update received";
           // SystemMessageModel.ExternalMessage =
           //     "External update received\n" +
           //     "(Projects will be updated after editing done)";

           // bool SameDistrictElemsUpdated = false;
            try
            {
                byte[] forDeleteBytes = message.Read();

                var formatter = new BinaryFormatter();
                Dictionary<int, string> forDelete = null;

                using (MemoryStream stream = new MemoryStream())
                {
                    stream.Write(forDeleteBytes, 0, forDeleteBytes.Length);
                    stream.Position = 0;
                    forDelete = (Dictionary<int, string>)formatter.Deserialize(stream);
                }

                Console.WriteLine("Deleted recived");

                //await MainCollectionEditSemaphore.WaitAsync();

                foreach (var item in forDelete)
                {
                    KircheElem cur = MainModel
                                        .ElemsView
                                        .SourceCollection
                                        .Cast<KircheElem>()
                                        .ToList()
                                        .Find(elem => elem.Id == item.Key
                                        && elem.Church_District == item.Value);
                    await Application.Current.Dispatcher
                        .BeginInvoke(new Action<object>(MainModel.ElemsView.Remove), cur);

                   // if (cur.Church_District == District)
                   //     SameDistrictElemsUpdated = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

           // if (SameDistrictElemsUpdated)
           //     SameDistrictUpdateReceive?.Invoke();
        }

        private async void UpdateReceiveAction()
        {

            SystemMessageModel.ExternalMessage =
                "External update received\n" +
                "(Projects will be updated after editing done)";

            // bool SameDistrictElemsUpdated = false;
            try
            {
                byte[] data = message.Read();

                List<KircheElem> updatedElems = KircheElem.DeserializationList(data);
                Console.WriteLine("Update recived");

               // await MainCollectionEditSemaphore.WaitAsync();

                foreach (var item in updatedElems)
                {
                    KircheElem cur = null;
                    if (MainModel
                        .ElemsView
                        .SourceCollection
                        .Cast<KircheElem>()
                        .ToList()
                        .Exists(elem => elem.Id == item.Id
                        && elem.Church_District == item.Church_District))
                    {
                        cur = MainModel
                                    .ElemsView
                                    .SourceCollection
                                    .Cast<KircheElem>()
                                    .ToList()
                                    .Find(elem => elem.Id == item.Id
                                    && elem.Church_District == item.Church_District);
                        cur.ApplyChanges(item);
                    }
                    else
                    {
                        cur = item;
                        await Application.Current.Dispatcher
                            .BeginInvoke(new Func<object, object>(MainModel.ElemsView.AddNewItem), cur);
                        await Application.Current.Dispatcher
                            .BeginInvoke(new Action(MainModel.ElemsView.CommitNew));
                    }

                    //if (cur.Church_District == District)
                    //    SameDistrictElemsUpdated = true;
                }

                await Application.Current.Dispatcher
                    .BeginInvoke(new Action(MainModel.ElemsView.Refresh));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

           // if (SameDistrictElemsUpdated)
           //     SameDistrictUpdateReceive?.Invoke();
        }

        public bool DeleteSendActionRequest(Dictionary<int, string> forDelete)
        {
            NetworkStreamMutex.WaitOne();

            bool res = true;
            try
            {
                var formatter = new BinaryFormatter();
                byte[] forDeleteBytes = null;
                using (MemoryStream stream = new MemoryStream())
                {
                    formatter.Serialize(stream, forDelete);
                    forDeleteBytes = stream.ToArray();
                }

                message.WriteRequest(ActionEnum.del_receive);
                message.Write(forDeleteBytes);

                if (message.ReadResult() == OperationResult.Good)
                    Console.WriteLine("For delete send");
                else
                    res = false;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                res = false;
            }

            NetworkStreamMutex.ReleaseMutex();
            return res;
        }

        public bool UpdateSendActionRequest(List<KircheElem> changes)
        {
            NetworkStreamMutex.WaitOne();

            bool res = true;
            try
            {
                byte[] updatedElems = KircheElem.SerializationList(changes);

                message.WriteRequest(ActionEnum.update_receive);

                message.Write(updatedElems);

                if (message.ReadResult() == OperationResult.Good)
                    Console.WriteLine("Update send");
                else
                    res = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                res = false;
            }

            NetworkStreamMutex.ReleaseMutex();
            return res;
        }

        public List<KircheElem> GetAllActionRequest()
        {
            NetworkStreamMutex.WaitOne();

            ConnectionState = ConnectionState.GetData;

            List<KircheElem> res = new List<KircheElem>();
            try
            {
                message.WriteRequest(ActionEnum.get_all);

                byte[] data = message.Read();
                res = KircheElem.DeserializationList(data);

                Console.WriteLine("Get all received");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            ConnectionState = ConnectionState.Connected;

            NetworkStreamMutex.ReleaseMutex();
            return res;
        }

        public CategoryCollections GetCategoriesActionRequest()
        {
            NetworkStreamMutex.WaitOne();

            CategoryCollections res = new CategoryCollections();
            try
            {
                message.WriteRequest(ActionEnum.get_categories);

                byte[] data = message.Read();

                res = CategoryCollections.Deserialization(data);
                Console.WriteLine("Get Categories received");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            NetworkStreamMutex.ReleaseMutex();
            return res;
        }

        private void ConnectionCheckActionRequest(object sender, EventArgs e)
        {
            NetworkStreamMutex.WaitOne();

            try
            {
                message.WriteRequest(ActionEnum.connection_check);
                message.Read();

                Console.WriteLine("ConnectionCheck - ok");
            }
            catch
            {
                Disconnect();
                connectionCheckTimer.Stop();
                Console.WriteLine("ConnectionCheck - bad");
            }

            NetworkStreamMutex.ReleaseMutex();
        }


        private bool inConnect;
        public bool Connect()
        {
            if (!inConnect)
                return TryToConnect();

            return false;
        }
        private bool TryToConnect(int connectionTry = 0)
        {
            inConnect = true;

            try
            {
                if (connectionTry == 0)
                    ConnectionState = ConnectionState.Connecting;
                else
                    ConnectionState = ConnectionState.Reconnecting;

                tcpClient = new TcpClient();
                tcpClient.Connect("178.251.104.48", 1815);
                tcpClient.ReceiveTimeout = 10000;
                tcpClient.SendTimeout = 10000;
                stream = tcpClient.GetStream();
                message = new Message(stream);
                streamIsOpen = true;
                connectionCheckTimer.Start();
                StartListening();

                ConnectionState = ConnectionState.Connected;

                inConnect = false;
                return true;

            }
            catch (Exception e)
            {
                if (connectionTry < 3)
                {
                    connectionTry++;
                    Console.WriteLine(e.Message);
                    TryToConnect(connectionTry);
                }
                else
                {
                    connectionTry = 0;
                    Disconnect();
                }
            }

            //NetworkStreamMutex.ReleaseMutex();
            inConnect = false;
            return false;
        }

        public bool LoginActionRequest(string district, string key)
        {
            NetworkStreamMutex.WaitOne();

            bool res = false;

            try
            {
                message.WriteRequest(ActionEnum.login);

                byte[] data = Encoding.Unicode.GetBytes(district);
                message.Write(data);

                data = Encoding.Unicode.GetBytes(key);
                message.Write(data);

                data = message.Read();
                res = Convert.ToBoolean(data[0]);

                if (res)
                {
                    District = district;
                    Key = key;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            NetworkStreamMutex.ReleaseMutex();
            return res;

        }

        private void Disconnect()
        {
            if (tcpClient != null)
            {
                ConnectionState = ConnectionState.Disconnected;
                connectionCheckTimer.Stop();
                tcpClient.Close();
                tcpClient = null;
                streamIsOpen = false;
            }
        }

        public void LogoutActionRequest()
        {
            NetworkStreamMutex.WaitOne();

            try
            {
                if (message != null)
                {
                    message.WriteRequest(ActionEnum.logout_receive);
                    Disconnect();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            NetworkStreamMutex.ReleaseMutex();
        }
    }
}

