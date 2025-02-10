using CommunityToolkit.Mvvm.Messaging;
using Wifi_Remote.Helper;
using static Wifi_Remote.Helper.CommandList;

namespace Wifi_Remote
{
    public partial class MainPage : ContentPage
    {
        private string? ipAddress = "192.168.122.212";
        private readonly HttpClient _httpClient;
        private TaskCompletionSource<bool>? _alertTask;
        private readonly IDictionary<string, bool> buttonTracker = new Dictionary<string, bool>()
        {
            { FORWARD, false },
            { BACKWARD, false },
            { LEFT, false },
            { RIGHT, false },
        };

        public MainPage()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<DeviceSelectedMessage>(this, (recipient, message) =>
            {
                DisplayAlert("Device Selected", $"You clicked on: {message.Value}", "OK");
                this.ipAddress = message.Value;
            });
            _httpClient = new HttpClient();
        }

        private static void handleBtnClick(object sender)
        {
            Button? button = sender as Button;
            if (button is not null)
            {
                button.BackgroundColor = Colors.DarkGray;
            }
        }

        private async void forwardButton_Pressed(object sender, EventArgs e)
        {
            handleBtnClick(sender);
            buttonTracker[FORWARD] = true;
            bool res = await DualButtonCheck();
            if (res)
            {
                await SendCommand(FORWARD);
            }
        }

        private async void backwardButton_Pressed(object sender, EventArgs e)
        {
            handleBtnClick(sender);
            buttonTracker[BACKWARD] = true;
            bool res = await DualButtonCheck();
            if (res)
            {
                await SendCommand(BACKWARD);
            }
        }

        private async void rightButton_Pressed(object sender, EventArgs e)
        {
            handleBtnClick(sender);
            buttonTracker[RIGHT] = true;
            bool res = await DualButtonCheck();
            if (res)
            {
                await SendCommand(RIGHT);
            }
        }

        private async void leftButton_Pressed(object sender, EventArgs e)
        {
            handleBtnClick(sender);
            buttonTracker[LEFT] = true;
            bool res = await DualButtonCheck();
            if (res)
            {
                await SendCommand(LEFT);
            }
        }

        private async Task<bool> DualButtonCheck()
        {
            if (buttonTracker[FORWARD] && buttonTracker[LEFT])
            {
                await SendCommand(FORWARD_LEFT);
                return false;
            }
            else if (buttonTracker[FORWARD] && buttonTracker[RIGHT])
            {
                await SendCommand(FORWARD_RIGHT);
                return false;
            }
            else if (buttonTracker[BACKWARD] && buttonTracker[LEFT])
            {
                await SendCommand(BACKWARD_LEFT);
                return false;
            }
            else if (buttonTracker[BACKWARD] && buttonTracker[RIGHT])
            {
                await SendCommand(BACKWARD_RIGHT);
                return false;
            }

            return true;
        }

        private async void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            int speedValue = (int)speed.Value;
            SpeedIndicator.Text = $"{speedValue}";
            speed.Value = speedValue;
            string cmd = speedValue < 10 ? speedValue.ToString() : "q";
            await SendCommand(cmd);
        }

        private async void stopBtn_Clicked(object sender, EventArgs e)
        {
            ((Button)sender).BackgroundColor = Colors.White;
            buttonTracker[FORWARD] = false;
            buttonTracker[RIGHT] = false;
            buttonTracker[LEFT] = false;
            buttonTracker[BACKWARD] = false;
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
            if(string.IsNullOrEmpty(command) || string.IsNullOrEmpty(ipAddress))
            {
                await DisplayAlert("Error", "Please set the IP address and select a device.", "OK");
            }
            try
            {
                await _httpClient.GetAsync($"http://{ipAddress}/?State={command}&speed={SpeedValues.GetValueOrDefault(speed.Value.ToString()), 80}");
            }
            catch (Exception ex)
            {
                if (_alertTask != null && !_alertTask.Task.IsCompleted)
                    return;

                _alertTask = new TaskCompletionSource<bool>();
                //await DisplayAlert("Error", ex.InnerException?.Message ?? ex.Message, "Ok", flowDirection: FlowDirection.LeftToRight);
                _alertTask.SetResult(true);
            }
        }

        private async void setIpBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                string userInput = await DisplayPromptAsync("Ip Address", "Enter ip address of Node MCU.", placeholder: "Enter ip address of Node Mcu ESP8266.", initialValue: this.ipAddress, keyboard: Keyboard.Text);

                if (!string.IsNullOrWhiteSpace(userInput))
                {
                    this.ipAddress = userInput;
                }
                
                await DisplayAlert("Success", $"Ip Address is: {ipAddress}", "OK");
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

        private void speed_Loaded(object sender, EventArgs e)
        {
            speed.Value = 4;
        }

        private async void sosBtn_Clicked(object sender, EventArgs e)
        {
            await SendCommand(SOS);
        }

        private void speed_DragStarted(object sender, EventArgs e)
        {
            Slider slider = (Slider)sender;
            if (slider != null)
            {
                Color newColor = Color.FromRgba(200, 200, 255, 255);

                switch (slider.Value)
                {
                    case 0:
                        newColor = Color.FromRgba(200, 200, 255, 255); // Light Blue (0)
                        break;
                    case > 0 and <= 3:
                        Color.FromRgba(144, 238, 144, 255); // Light Green (3)
                        break;
                    case > 3 and <= 6:
                        newColor = Color.FromRgba(0, 128, 0, 255); // Yellow (6)
                        break;
                    case > 6 and <= 8:
                        newColor = Color.FromRgba(255, 165, 0, 255); // Orange (8)
                        break;
                    default:
                        newColor = Color.FromRgba(255, 69, 69, 255); // Red (10)
                        break;
                }

                // Apply color to slider
                speed.ThumbColor = newColor;
                speed.MinimumTrackColor = newColor;
            }
        }
    }
}
