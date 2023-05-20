using Connection.src;
using System.Net;
using System.Windows;

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
            networkManager.SendAllOnLoaded();
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            Command command = new Command(IPAddress.Broadcast, NetworkManager.myIP, CommandType.NotCommand, "Hello from btn by " + NetworkManager.myIP.ToString());
            networkManager.SendCommand(command);

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
    }
}
