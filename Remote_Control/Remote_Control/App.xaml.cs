using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Remote_Control
{
    public partial class App : Application
    {
        private const int PORT = 80;
        private const int TIMEOUT = 500;
        private const string SERVER_IP = "192.168.0.100";

        public static bool IS_PAGE_OPEN = false;
        public static bool HAS_NETWORK_ACCESS = false;
        public static bool HAS_CONNECTION = false;

        public static Action onEveryRun = () => { };
        public static Action onNewConnection = () => { };
        public static Action onNewDisconnection = () => { };
        public static Action onWhileConnected = () => { };
        public static Action onWhileDisconnected = () => { };

        public App()
        {
            InitializeComponent();

            Current.PageAppearing += OnPageAppearing;
            MainPage = new NavigationPage(new MainPage());
        }

        public static async void scheduler()
        {
            bool was_connected = false;

            while (IS_PAGE_OPEN)
            {
                // check connection status
                HAS_NETWORK_ACCESS = Connectivity.NetworkAccess == NetworkAccess.Internet;
                HAS_CONNECTION = HAS_NETWORK_ACCESS && send_code("XXXX") == "YYYY";

                // every run
                onEveryRun();

                // new connection
                if (HAS_CONNECTION && !was_connected)
                {
                    onNewConnection();
                    was_connected = true;
                }

                // new disconnection
                if (!HAS_CONNECTION && was_connected)
                {
                    onNewDisconnection();
                    was_connected = false;
                }

                // while connected
                if (HAS_CONNECTION) onWhileConnected();

                // while disconnected
                if (!HAS_CONNECTION) onWhileDisconnected();


                // short delay
                await Task.Delay(250);
            }

        }

        public static string send_code(string action_id)
        {
            string answer = null;

            try
            {
                IPAddress ip = IPAddress.Parse(SERVER_IP);
                EndPoint endPoint = new IPEndPoint(ip, PORT);
                Socket socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveTimeout = TIMEOUT;
                socket.SendTimeout = TIMEOUT;

                Task connect = socket.ConnectAsync(endPoint);
                if (connect.Wait(TIMEOUT))
                {
                    byte[] message_sent = Encoding.ASCII.GetBytes(action_id);
                    int bytes_sent = socket.Send(message_sent);

                    byte[] message_received = new byte[4];
                    int bytes_received = socket.Receive(message_received);

                    answer = Encoding.ASCII.GetString(message_received, 0, 4);
                }

                socket.Close();

                return simplify(answer);
            }
            catch (Exception)
            {
                return answer;
            }
        }

        private static string simplify(string answer)
        {
            if (answer == "__ON") return "On";
            if (answer == "_OFF") return "Off";

            return answer;
        }

        private void OnPageAppearing(object sender, Page e)
        {
            if (e is MainPage)
            {
                // stop loop
                IS_PAGE_OPEN = false;

                // reset functions
                onEveryRun = () => { };
                onNewConnection = () => { };
                onNewDisconnection = () => { };
                onWhileConnected = () => { };
                onWhileDisconnected = () => { };
            }
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
