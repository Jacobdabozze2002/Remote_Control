using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Remote_Control
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void open_LED_control_Clicked(object sender, EventArgs e)
        {
            App.IS_PAGE_OPEN = true;
            await Navigation.PushAsync(new LED_control());
        }


    }
}

