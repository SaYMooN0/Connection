using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using static Connection.src.CommandManager;

namespace Connection.src
{
    public sealed class NetworkManager : IDisposable
    {
        private static readonly NetworkManager _instance = new NetworkManager();
        private Thread _listenerThread;
        public event EventHandler ConnectionEstablished ;
        //fields for UPD connection
        static public IPAddress myIP { get; private set; }
        private UdpClient _udpClient;
        static public bool receivingOffers { get; private set; }
        private const int _udpPort = 8015;

        //fields for TCP connection
        private TcpClient _tcpClient;
        private TcpListener _tcpListener;
        private NetworkStream _tcpStream;
        public bool _listeningTCP = true;
        private const int _tcpPort = 8016;

        static NetworkManager() { }

        private NetworkManager()
        {
            _udpClient = new UdpClient(_udpPort);
            _listenerThread = new Thread(ReceiveMessageByUDP);
            receivingOffers = true;
            //getting the ipV4 adress
            myIP = Dns.GetHostAddresses(Dns.GetHostName()).First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            CommandManager.GotConnectionApproved += RunTCPClient;
            _listenerThread.IsBackground = true;
            _listenerThread.Start();

        }

        public static NetworkManager Instance { get { return _instance; } }
        internal void SendAllOnLoaded()
        {
            Command cmd = new Command(IPAddress.Broadcast, myIP, CommandType.ToEveryOneLoading);
            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("255.255.255.255"), _udpPort);
            byte[] messageBytes = Encoding.UTF8.GetBytes(cmd.getStringToSend());
            try { udpClient.Send(messageBytes, messageBytes.Length, endPoint); }
            finally { udpClient.Close(); }
        }
        internal void SendCommandByUPD(Command cmd)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            IPEndPoint endPoint = new IPEndPoint(cmd.reciever, _udpPort);
            byte[] messageBytes = Encoding.UTF8.GetBytes(cmd.getStringToSend());
            try { udpClient.Send(messageBytes, messageBytes.Length, endPoint); }
            finally { udpClient.Close(); }
        }
        private void ReceiveMessageByUDP()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, _udpPort);
            while (receivingOffers)
            {
                byte[] receivedBytes = _udpClient.Receive(ref endPoint);
                string receivedMessage = Encoding.UTF8.GetString(receivedBytes);
                CommandManager.ProcessTheCommand(receivedMessage);
            }
        }
        internal void RunTCPServer(IPAddress clientIP)
        {
            try
            {
                _tcpListener = new TcpListener(myIP, _tcpPort);
                _tcpListener.Start();
                _tcpClient = _tcpListener.AcceptTcpClient();
                _listenerThread = new Thread(ReadingTCPStream);
                _listenerThread.Start();
                _tcpStream = _tcpClient.GetStream();
                CloseUDP();
                ConnectionEstablished?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + clientIP.ToString());
            }
        }
        internal void RunTCPClient(string serverIP)
        {
            _tcpClient= new TcpClient(serverIP, _tcpPort);
            try
            {
                _tcpStream = _tcpClient.GetStream();
                _listenerThread = new Thread(ReadingTCPStream);
                _listenerThread.Start();
                CloseUDP();
                ConnectionEstablished?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + serverIP.ToString());
            }
        }
        private void ReadingTCPStream(object stream)
        {
            const int bufferSize = 2048;
            byte[] data = new byte[bufferSize];
            int bytes;

            while (_listeningTCP)
            {
                bytes = _tcpStream.Read(data, 0, data.Length);
                if (bytes == 0)
                    break;
                string receivedMessage = Encoding.UTF8.GetString(data, 0, bytes);
                CommandManager.ProcessTheCommand(receivedMessage);
            }
        }
        internal void TCPSend(Command cmd)
        {
            if(_tcpStream==null)
            {
                MessageBox.Show("_tcpStream=null\nTCPSend метод");
                return;
            }
            byte[] data = Encoding.UTF8.GetBytes(cmd.getStringToSend());
            _tcpStream.Write(data, 0, data.Length);
        }

        private void CloseUDP(){receivingOffers = false;}
        private void DisposeTCP()
        {
            _listeningTCP = false;
            _tcpStream.Close();
            _tcpClient.Close();
            _tcpListener.Stop();
        }
        public void Dispose()
        {
            CloseUDP();
            DisposeTCP();
        }

    }
}
