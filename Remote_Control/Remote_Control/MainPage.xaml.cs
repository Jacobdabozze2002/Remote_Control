using System;
using Xamarin.Forms;

namespace Remote_Control
{
    public partial class MainPage : ContentPage
    {
        private Button[] buttons;
        private bool[] led_status;
        private bool timer_activ;

        public MainPage()
        {
            InitializeComponent();

            buttons = new Button[] { btn1, btn2, btn3, btn4 };
            led_status = new bool[4];
            timer_activ = false;

            initialize_connection();
        }

        private void initialize_connection()
        {
            foreach (Button button in buttons) button.IsEnabled = false;
            stop_timer();
            lbConnect.Text = "Not Connected";

            // auf wifi-verbindung warten
            // verbindung auf lbConnect anzeigen
            // probeanfrage senden
            // auf antwort warten ...
            // led_status mit _led_status überschreiben

            for (int i = 0; i < buttons.Length; ++i)
            {
                buttons[i].Text = "LED " + (i+1) + (led_status[i] ? " ON" : " OFF");
                buttons[i].IsEnabled = true;
            }
            start_timer(250);
        }

        private void request_status_change(object sender, EventArgs e)
        {
            int i = Array.IndexOf(buttons, sender);
            led_status[i] = !led_status[i];
            buttons[i].IsEnabled = false;
        }

        private void start_timer(int millis)
        {
            timer_activ = true;
            Device.StartTimer(TimeSpan.FromMilliseconds(millis), () =>
            {
                timer_event();
                return timer_activ;
            });
        }

        private void stop_timer() => timer_activ = false;

        private void timer_event()
        {
            // sende led_status an Mikrokontroller
            // Mikrokontroller verarbeitet led_status
            // Mikrokontroller sendet _led_status zurück ...
            // led_status wird mit _led_status überschrieben

            for (int i = 0; i < buttons.Length; ++i)
            {
                buttons[i].Text = "LED " + (i + 1) + (led_status[i] ? " ON" : " OFF");
                buttons[i].IsEnabled = true;
            }
        }
    }
}


/*
    Mikrokontroller > Mobiles Gerät:
    - Mobiles Gerät sendet Statuswunsch
    - Mikrokontroller bearbeitet diesen, entscheidet aber final über Umsetzung

    Init:
    - sperrt alle Buttons
    - setzt Timer zurück
    - setzt lbConnect zurück
    - wartet auf Wifi-Verbindung 
    - zeigt Verbindung auf lbConnect an
    - sendet Probeanfrage -> erwartet Rückgabe von _led_status
    - wartet auf Antwort:
        - !!! wenn (nach Timeout) keine oder falsche Antwort, Init aufrufen!!!
    - überschreibt led_status mit _led_status
    - led_status wird auf Buttons angezeigt
    - entsperrt alle Buttons
    - startet Timer

    Timer:
    - sende led_status an Mikrokontroller (ausreichend sicher?)
    - Mikrokontroller verarbeitet led_status
    - Mikrokontroller sendet _led_status zurück
        - !!! wenn (nach Timeout) keine Antwort, Init aufrufen !!!
    - led_status wird mit _led_status überschrieben
    - led_status wird auf buttons angezeigt
    - alle Buttons werden wieder freigeschaltet
    - warte Zeit -> Button-OnClick kann led_status ändern
    - wiederhole

    Button-OnClicK:
    - ändert led_status zu gewüschten Zustand 
    - geklickter Button wird gesperrt
 */
