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

        private async void navigate_to(Page page, bool start_scheduler = true)
        {
            // null -> Es besteht keine Netzverbindung
            // Error
            if (App.send_code("XXXX") == null)
            {
                await App.Current.MainPage.DisplayAlert("Bad Connection", "Check your connection settings.", "OK");
                return;
            }

            // Spiegelantwort -> Server arbeitet bereits mit Client
            // Error
            if (start_scheduler && App.send_code("XXXX") == "XXXX")
            {
                await App.Current.MainPage.DisplayAlert("Request denied", "Another client has already connected to the server.\nPlease try again later.", "OK");
                return;
            }

            // Seite öffnen und Scheduler starten
            await Navigation.PushAsync(page);

            if (start_scheduler)
            {
                await Task.Delay(250);
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

