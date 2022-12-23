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
        private const int PORT = 12345;
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

        private static MainPage mainPage;

        public App()
        {
            InitializeComponent();

            Current.PageAppearing += OnPageAppearing;
            mainPage = new MainPage();
            MainPage = new NavigationPage(mainPage);
        }

        public static async void scheduler()
        {
            await Task.Run(async () =>
            {
                bool was_connected = false;
                int fail_counter = 0;

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
                        fail_counter = 0;
                    }

                    // new disconnection
                    if (!HAS_CONNECTION && was_connected)
                    {
                        onNewDisconnection();
                        was_connected = false;
                    }

                    // while connected
                    if (HAS_CONNECTION)
                    {
                        onWhileConnected();
                    }

                    // while disconnected
                    if (!HAS_CONNECTION)
                    {
                        onWhileDisconnected();
                        if (HAS_NETWORK_ACCESS) fail_counter++;
                    }

                    // kick - no connection
                    if (fail_counter >= 3)
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            mainPage.openHint("Reconnection Error", "The connection to the Server could not be reastablished.");
                            App.Current.MainPage.Navigation.PopAsync();
                        });
                    }

                    // short delay
                    await Task.Delay(500);
                }
            });
        }
        
        public static string send_code(string action_id)
        {
            string answer = null;
            bool go_on = true;

            try
            {
                for (int i = 0; i < (HAS_CONNECTION ? 5 : 2) && go_on; ++i)
                {
                    IPAddress ip = IPAddress.Parse(SERVER_IP);
                    EndPoint endPoint = new IPEndPoint(ip, PORT);
                    Socket socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.ReceiveTimeout = TIMEOUT;
                    socket.SendTimeout = TIMEOUT;
                    socket.Blocking = true;

                    try
                    {
                        // CONNECT
                        socket.Connect(endPoint);

                        // SEND
                        byte[] message_sent = Encoding.ASCII.GetBytes(action_id);
                        _ = socket.Send(message_sent);

                        // RECEIVE
                        byte[] message_received = new byte[4];
                        _ = socket.Receive(message_received);

                        answer = Encoding.ASCII.GetString(message_received, 0, 4);

                        go_on = false;
                    }
                    catch (Exception) {}

                    socket.Close();
                }

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
            if (e is MainPage && IS_PAGE_OPEN)
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

        protected async override void OnSleep()
        {
            if (IS_PAGE_OPEN)
            {
                await App.Current.MainPage.Navigation.PopAsync();
                mainPage.openHint("Inactivity Error", "You have been moved to the Main Page due to inactivity.");
            }
        }

        protected override void OnResume()
        {

        }
    }
}
