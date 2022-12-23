using System;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Remote_Control
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public void openHint(string title = "Title", string message = "This is a message.")
        {
            hint_title.Text = title;
            hint_message.Text = message;
            popupView.IsVisible = true;
        }

        private void closeHint(object sender, EventArgs e)
        {
            popupView.IsVisible = false;
        }

        private async void navigate_to(Page page, bool start_scheduler = true)
        {
            // Keine Verbindung möglich
            if (start_scheduler)
            {
                string answer = null;
                await Task.Run(() => answer = App.send_code("XXXX"));

                if (answer == null)
                {
                    openHint("Connection Error", "The connection to the server failed.");
                    return;
                };
            }

            // Seite öffnen und Scheduler starten
            await Navigation.PushAsync(page);

            if (start_scheduler)
            {
                App.IS_PAGE_OPEN = true;
                App.scheduler();
            }
        }

        private void open_LED_control_Clicked(object sender, EventArgs e)
        {
            navigate_to(new LED_control());
        }

    }
}

