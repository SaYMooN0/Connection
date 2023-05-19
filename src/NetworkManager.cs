using System;
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
        private IPAddress _ip;
        public delegate void EventStringContainer(string msg);
        public event EventStringContainer AddToMsgEvent;
        public event EventStringContainer AddToCmndsEvent;
        static NetworkManager() { }
        
        private NetworkManager()
        {
            _udpClient = new UdpClient(_port);
            _listenerThread = new Thread(ReceiveMessage);
            _ip = Dns.GetHostByName(Dns.GetHostName()).AddressList[0];
            _listenerThread.IsBackground = true;
            _listenerThread.Start();

        }

        public static NetworkManager Instance { get { return _instance; } }
        public void SendAllOnLoaded()
        {
            Command cmd = new Command(IPAddress.Broadcast, _ip, CommandType.ToEveryOneLoading);
            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), _port);
            byte[] messageBytes = Encoding.UTF8.GetBytes(cmd.getStringToSend());
            try { udpClient.Send(messageBytes, messageBytes.Length, endPoint); AddToCmndsEvent?.Invoke($"Sended: {cmd.ToString()}"); }
            catch (Exception ex) {AddToCmndsEvent?.Invoke($"Error in send all on load\n{ex.Message}\n{cmd.getStringToSend()}"); }
            finally { udpClient.Close(); }
        }

        private void ReceiveMessage()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _port);
            AddToCmndsEvent?.Invoke($"my ip {_ip}");
            AddToCmndsEvent?.Invoke($"listening");
            while (_listening)
            {
                byte[] receivedBytes = _udpClient.Receive(ref endPoint);
                string receivedMessage = Encoding.UTF8.GetString(receivedBytes);
                AddToCmndsEvent?.Invoke($"-=-=-=-=-=--=--=-=-=-=\nReceived message: {receivedMessage}");
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
