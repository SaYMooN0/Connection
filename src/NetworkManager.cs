using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Connection.src
{
    public sealed class NetworkManager : IDisposable
    {
        private static readonly NetworkManager _instance = new NetworkManager();
        private Thread _listenerThread;
        private UdpClient _udpClient;
        public bool _listening = true;
        private const int _port = 8015;
        static public IPAddress myIP { get; private set; }
        static public bool receivingOffers { get; private set; }
        
        static NetworkManager() { }
        
        private NetworkManager()
        {
            _udpClient = new UdpClient(_port);
            _listenerThread = new Thread(ReceiveMessage);
            receivingOffers = true;
            //getting the ipV4 adress
            myIP = Dns.GetHostAddresses(Dns.GetHostName()).First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            _listenerThread.IsBackground = true;
            _listenerThread.Start();

        }

        public static NetworkManager Instance { get { return _instance; } }
        public void SendAllOnLoaded()
        {
            Command cmd = new Command(IPAddress.Broadcast, myIP, CommandType.ToEveryOneLoading);
            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), _port);
            byte[] messageBytes = Encoding.UTF8.GetBytes(cmd.getStringToSend());
            try { udpClient.Send(messageBytes, messageBytes.Length, endPoint);}
            finally { udpClient.Close(); }
        }
        internal void SendCommand(Command cmd)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            IPEndPoint endPoint = new IPEndPoint(cmd.reciever, _port);
            byte[] messageBytes = Encoding.UTF8.GetBytes(cmd.getStringToSend());
            try { udpClient.Send(messageBytes, messageBytes.Length, endPoint); }
            finally { udpClient.Close(); }
        }

        private void ReceiveMessage()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _port);
            while (_listening)
            {
                byte[] receivedBytes = _udpClient.Receive(ref endPoint);
                string receivedMessage = Encoding.UTF8.GetString(receivedBytes);
                CommandManager.ProcessTheCommand(receivedMessage);
            }
        }

        public void Dispose()
        {
            _listening = false;
            _udpClient.Close();
            _listenerThread.Join();
        }

    }
}
