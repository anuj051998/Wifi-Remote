using CommunityToolkit.Mvvm.Messaging;
using Wifi_Remote.Helper;
using static Wifi_Remote.Helper.CommandList;

namespace Wifi_Remote
{
    public partial class MainPage : ContentPage
    {
        private string? ipAddress;

        public MainPage()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<DeviceSelectedMessage>(this, (recipient, message) =>
            {
                DisplayAlert("Device Selected", $"You clicked on: {message.Value}", "OK");
            });
        }

        private async void forwardButton_Pressed(object sender, EventArgs e)
        {
            ((Button)sender).BackgroundColor = Colors.DarkGray;
            await SendCommand(FORWARD);
        }

        private async void backwardButton_Pressed(object sender, EventArgs e)
        {
            await SendCommand(BACKWARD);
        }

        private async void rightButton_Pressed(object sender, EventArgs e)
        {
            await SendCommand(RIGHT);
        }

        private async void leftButton_Pressed(object sender, EventArgs e)
        {
            await SendCommand(LEFT);
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            SpeedIndicator.Text = $"Speed: {speed.Value}";
        }

        private async void stopBtn_Clicked(object sender, EventArgs e)
        {
            await SendCommand(STOP);
        }

        private async void lightBtn_Clicked(object sender, EventArgs e)
        {
            bool isLightOn = !lightBtn.IsVisible;
            if (isLightOn)
            {
                lightBtn.IsVisible = true;
                lightBtnOn.IsVisible = false;
                await SendCommand(LIGHT_OFF);
            }
            else
            {
                lightBtn.IsVisible = false;
                lightBtnOn.IsVisible = true;
                await SendCommand(LIGHT_ON);
            }
        }

        private async Task SendCommand(string command)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    await client.GetAsync($"{ipAddress}?state={command}&speed={SpeedValues.GetValueOrDefault(speed.Value.ToString()),60}");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void setIpBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                this.ipAddress = await DisplayPromptAsync("Ip Address", "Enter ip address of Node MCU.", keyboard: Keyboard.Text);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void scanIpBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushModalAsync(new DeviceListPage());
            }
            catch
            {
                await DisplayAlert("Error", "Unable to scan for devices.", "OK");
            }
        }

        public async Task<IList<string>> GetConnectedDevicesAsync()
        {
            try
            {
                return await NetworkScanner.GetConnectedDevicesAsync();
            }
            catch
            {
                return Enumerable.Empty<string>().ToList();
            }
        }
    }
}
