using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;


namespace Remote_Control
{
    public partial class LED_control : ContentPage
    {
        private Button[] buttons;
        private string[] commands_buttons = { null, null, null, null };

        public LED_control()
        {
            InitializeComponent();

            buttons = new Button[] { btn1, btn2, btn3, btn4 };

            enable_buttons(false);

            // Connection Events
            App.onEveryRun = () =>
            {
                lbConnect.Text = App.HAS_NETWORK_ACCESS ? (App.HAS_CONNECTION ? "Connected" : "Connecting ...") : "Disconnected";
            };

            App.onNewConnection = () =>
            {
                _ = read_button_status();
                enable_buttons(true);
            };

            App.onNewDisconnection = () =>
            {
                reset_buttons();
                enable_buttons(false);
            };

            App.onWhileConnected = () =>
            {
                _ = send_one_button_request();
            };

            App.onWhileDisconnected = () =>
            {
            };
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
                    string answer = App.send_code(commands_buttons[i]);
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
                answer = App.send_code("000" + (i * 2));

                if (answer == "On" || answer == "Off") status[i] = answer;
                else return false;
            }

            for (int i = 0; i < buttons.Length; ++i) buttons[i].Text = "LED " + i + " " + status[i];
            return true;
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
