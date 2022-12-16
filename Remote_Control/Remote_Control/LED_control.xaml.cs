using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Remote_Control
{
    public partial class LED_control : ContentPage
    {
        private Button[] buttons;

        private const int PORT = 80;
        private const int TIMEOUT = 500;
        private const string SERVER_IP = "192.168.0.100";

        /*
         für mehrere Seiten public machen
         bei Seiten-Start zurücksetzen und buttons neu initialisieren
         send_on_button_request() anpassen 
        */
        private string[] commands_buttons = { null, null, null, null };

        public LED_control()
        {
            InitializeComponent();

            buttons = new Button[] { btn1, btn2, btn3, btn4 };

            scheduler();
        }

        private async void scheduler()
        {
            bool has_network_access;
            bool has_connection;
            bool was_connected = false;
            enable_buttons(false);

            while (true)
            {
                // check connection status
                has_network_access = Connectivity.NetworkAccess == NetworkAccess.Internet;
                has_connection = has_network_access && send_code("XXXX") == "YYYY";

                // set text of lbConnected
                lbConnect.Text = has_network_access ? (has_connection ? "Connected" : "Connecting ...") : "Disconnected";

                // new connection
                if (has_connection && !was_connected)
                {
                    _ = read_button_status();
                    enable_buttons(true);
                    was_connected = true;
                }

                // new disconnection
                if (!has_connection && was_connected)
                {
                    reset_buttons();
                    enable_buttons(false);
                    was_connected = false;
                }

                // while connected
                if (has_connection)
                {
                    _ = send_one_button_request();
                }

                // while disconnected
                if (!has_connection)
                {

                }

                // short delay
                await Task.Delay(250);
            }

        }

        private async void request_status_change(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    enable_buttons(false);
                    int i = Array.IndexOf(buttons, sender);
                    commands_buttons[i] = "000" + (i * 2 + 1);
                });

            });
        }

        private bool send_one_button_request()
        {
            for (int i = 0; i < buttons.Length; ++i)
            {
                if (commands_buttons[i] != null)
                {
                    string answer = send_code(commands_buttons[i]);
                    commands_buttons[i] = null;
                    enable_buttons(true);

                    if (answer == "On" || answer == "Off")
                    {
                        buttons[i].Text = "LED " + i + " " + answer;
                        break;
                    }
                    else return false;
                }
            }
            return true;
        }

        private bool read_button_status()
        {
            string answer;
            string[] status = new string[buttons.Length];

            for (int i = 0; i < buttons.Length; ++i)
            {
                answer = send_code("000" + (i * 2));

                if (answer == "On" || answer == "Off") status[i] = answer;
                else return false;
            }

            for (int i = 0; i < buttons.Length; ++i) buttons[i].Text = "LED " + i + " " + status[i];
            return true;
        }

        private string send_code(string action_id)
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

        private string simplify(string answer)
        {
            if (answer == "__ON") return "On";
            if (answer == "_OFF") return "Off";

            return answer;
        }

        private void reset_buttons() { for (int i = 0; i < buttons.Length; ++i) buttons[i].Text = "LED " + i + " Off"; }

        private void enable_buttons(bool enable) { foreach (Button button in buttons) button.IsEnabled = enable; }
    }
}

/*
    Sendeprotokoll:
    - 4 Byte: Aktions-ID
        - "XXXX"    : Verbindungsstatus
        - "0000"    : LED 1 Status               
        - "0001"    : LED 1 Toggle + Status
        - "0002"    : LED 2 Status
        - "0003"    : LED 2 Toggle + Status
        - "0004"    : LED 3 Status
        - "0005"    : LED 3 Toggle + Status
        - "0006"    : LED 4 Status
        - "0007"    : LED 4 Toggle + Status
        - ...
    - Antwort für Verbindungsanfrage: YYYY
    - Antwort für LED (Toggle): __ON oder _OFF
*/
