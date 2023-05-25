using Connection.src;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace Connection
{
    public partial class MainWindow : Window
    {
        NetworkManager networkManager;
        public MainWindow()
        {
            InitializeComponent();
            networkManager = NetworkManager.Instance;
            CommandManager.AddToMsgEvent += MsgAdd;
            CommandManager.AddToCmndsEvent += CmdAdd;
            CommandManager.AddToUsersList += AddToUsers;
            CommandManager.ConnectionRequest += ConnectionRequestGot;
            networkManager.SendAllOnLoaded();
            networkManager.ConnectionEstablished += ConnectionEstablished;
        }

        private void ConnectionEstablished(object? sender, System.EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayBtnTCP.Visibility = Visibility.Visible;
                PlayBtn.Visibility = Visibility.Hidden;
            });
        }

        private void TCP_Click(object sender, RoutedEventArgs e)
        {
            Command command = new Command(IPAddress.Broadcast, NetworkManager.myIP, CommandType.NotCommand, "Hello from btn by " + NetworkManager.myIP.ToString());
            networkManager.TCPSend(command);
        }
        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            Command command = new Command(IPAddress.Broadcast, NetworkManager.myIP, CommandType.NotCommand, "Hello from btn by " + NetworkManager.myIP.ToString());
            networkManager.SendCommandByUPD(command);

        }
        public void MsgAdd(string s)
        {
            Dispatcher.Invoke(() => { Msgs.Text += s + "\n"; ; });
        }
        public void CmdAdd(string s)
        {
            Dispatcher.Invoke(() => { Cmnds.Text += s + "\n"; });
        }
        public void AddToUsers(string s)
        {
            Dispatcher.Invoke(() => { usersList.Items.Add(s); });
        }
        private void usersList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedItem = (sender as ListBox).SelectedItem;
            if (selectedItem != null)
            {
                Command command = new Command(IPAddress.Parse(selectedItem.ToString()), NetworkManager.myIP, CommandType.ConectionRequest);
                networkManager.SendCommandByUPD(command);
            }
        }
        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            if (requestsList.SelectedItem != null || requestsList.SelectedItems.Count > 1)
            {
                var user = requestsList.SelectedItem as string;
                Command cmd = new Command(IPAddress.Parse(user), NetworkManager.myIP, CommandType.RequestRejected);
                networkManager.SendCommandByUPD(cmd);
                requestsList.Items.Remove(requestsList.SelectedItem);

            }
            else
                MessageBox.Show("Choose 1 user");
        }
        private void Approved_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (requestsList.SelectedItem != null || requestsList.SelectedItems.Count > 1)
            {
                var user = requestsList.SelectedItem as string;
                MessageBox.Show("Connection with " + user);
                IPAddress ip = IPAddress.Parse(user);
                Command command = new Command(ip, NetworkManager.myIP, CommandType.RequestApproved);
                networkManager.SendCommandByUPD(command);
                networkManager.RunTCPServer(ip);
            }
            else
                MessageBox.Show("Choose 1 user");
        }
        private void ConnectionRequestGot(string s)
        {
            foreach (var item in usersList.Items)
            {
                if (item.ToString() == s)
                {
                    Dispatcher.Invoke(() => { usersList.Items.Remove(s); });
                    break;
                }
            }
            Dispatcher.Invoke(() =>
            {
                requestsList.Items.Add(s);
            });
        }
    }
}
