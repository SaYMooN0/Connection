using Connection.src;
using System.Threading;
using System.Windows;

namespace Connection
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CommandManager.AddToMsgEvent += MsgAdd;
            CommandManager.AddToCmndsEvent += CmdAdd;
            

        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            NetworkManager networkManager = NetworkManager.Instance;
            networkManager.SendAllOnLoaded();

        }
        public void MsgAdd(string s)
        {
            Dispatcher.Invoke(() => { Msgs.Text += s + "\n"; ; });
        }
        public void CmdAdd(string s)
        {
            Dispatcher.Invoke(() => { Cmnds.Text += s + "\n"; });
        }
    }
}
