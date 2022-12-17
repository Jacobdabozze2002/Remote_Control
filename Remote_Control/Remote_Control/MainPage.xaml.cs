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

