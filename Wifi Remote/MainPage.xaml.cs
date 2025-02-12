using CommunityToolkit.Mvvm.Messaging;
using Wifi_Remote.Helper;
using static Wifi_Remote.Helper.CommandList;

namespace Wifi_Remote
{
    public partial class MainPage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private TaskCompletionSource<bool>? _alertTask;
        private readonly Dictionary<string, bool> buttonTracker = new Dictionary<string, bool>()
        {
            { FORWARD, false },
            { BACKWARD, false },
            { LEFT, false },
            { RIGHT, false },
        };

        private string? IpAddressValue;

        public MainPage()
        {
            InitializeComponent();
            WeakReferenceMessenger.Default.Register<DeviceSelectedMessage>(this, (recipient, message) =>
            {
                DisplayAlert("Device Selected", $"You clicked on: {message.Value}", "OK");
                IpAddressValue = message.Value;
            });
            _httpClient = new HttpClient();

            Task.Run(async () =>
            {
                string? ip = await StorageHelper.GetAsync(Constants.StorageKeys.IpAddress);
                if (!string.IsNullOrEmpty(ip))
                {
                    IpAddressValue = ip;
                    SetIpBtn.Text = $"IP Address: {IpAddressValue}";
                }
            });
        }

        private static void HandleBtnClick(object sender)
        {
            Button? button = sender as Button;
            if (button is not null)
            {
                button.BackgroundColor = Colors.Grey;
            }
        }

        private async void ForwardButton_Pressed(object sender, EventArgs e)
        {
            HandleBtnClick(sender);
            buttonTracker[FORWARD] = true;
            bool res = await DualButtonCheck();
            if (res)
            {
                await SendCommand(FORWARD);
            }
        }

        private async void BackwardButton_Pressed(object sender, EventArgs e)
        {
            HandleBtnClick(sender);
            buttonTracker[BACKWARD] = true;
            bool res = await DualButtonCheck();
            if (res)
            {
                await SendCommand(BACKWARD);
            }
        }

        private async void RightButton_Pressed(object sender, EventArgs e)
        {
            HandleBtnClick(sender);
            buttonTracker[RIGHT] = true;
            bool res = await DualButtonCheck();
            if (res)
            {
                await SendCommand(RIGHT);
            }
        }

        private async void LeftButton_Pressed(object sender, EventArgs e)
        {
            HandleBtnClick(sender);
            buttonTracker[LEFT] = true;
            bool res = await DualButtonCheck();
            if (res)
            {
                await SendCommand(LEFT);
            }
        }

        private async Task<bool> DualButtonCheck()
        {
            bool result = false;
            if (buttonTracker[FORWARD] && buttonTracker[LEFT])
            {
                await SendCommand(FORWARD_LEFT);
            }
            else if (buttonTracker[FORWARD] && buttonTracker[RIGHT])
            {
                await SendCommand(FORWARD_RIGHT);
            }
            else if (buttonTracker[BACKWARD] && buttonTracker[LEFT])
            {
                await SendCommand(BACKWARD_LEFT);
            }
            else if (buttonTracker[BACKWARD] && buttonTracker[RIGHT])
            {
                await SendCommand(BACKWARD_RIGHT);
            }
            else
            {
                result = true;
            }

            return result;
        }

        private async void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            int speedValue = (int)speed.Value;
            SpeedIndicator.Text = $"{speedValue}";
            speed.Value = speedValue;
            string cmd = speedValue < 10 ? speedValue.ToString() : "q";
            await Task.WhenAll(SendCommand(cmd), StorageHelper.SetAsync(Constants.StorageKeys.Speed, speedValue.ToString()));
        }

        private async void StopBtn_Clicked(object sender, EventArgs e)
        {
            ((Button)sender).BackgroundColor = Colors.White;
            buttonTracker[FORWARD] = false;
            buttonTracker[RIGHT] = false;
            buttonTracker[LEFT] = false;
            buttonTracker[BACKWARD] = false;
            await SendCommand(STOP);
        }

        private async void LightBtn_Clicked(object sender, EventArgs e)
        {
            bool isLightOn = !LightBtn.IsVisible;
            if (isLightOn)
            {
                LightBtn.IsVisible = true;
                LightBtnOn.IsVisible = false;
                await SendCommand(LIGHT_OFF);
            }
            else
            {
                LightBtn.IsVisible = false;
                LightBtnOn.IsVisible = true;
                await SendCommand(LIGHT_ON);
            }
        }

        private async Task SendCommand(string command)
        {
            if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(IpAddressValue))
            {
                await DisplayAlert("Error", "Please set the IP address and select a device.", "OK");
            }
            try
            {
                await _httpClient.GetAsync($"http://{IpAddressValue}/?State={command}&speed={SpeedValues.GetValueOrDefault(speed.Value.ToString()),80}");
            }
            catch (Exception ex)
            {
                if (_alertTask != null && !_alertTask.Task.IsCompleted)
                    return;

                _alertTask = new TaskCompletionSource<bool>();
#if DEBUG
                await DisplayAlert("Error", ex.InnerException?.Message ?? ex.Message, "Ok", flowDirection: FlowDirection.LeftToRight);
#endif
                _alertTask.SetResult(true);
            }
        }

        private async void SetIpBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                string? currentIpValue = this.IpAddressValue;
                string userInput = await DisplayPromptAsync("Enter Ip Address", "Enter ip address of Node MCU.", initialValue: IpAddressValue, keyboard: Keyboard.Text);

                if (!string.IsNullOrWhiteSpace(userInput))
                {
                    IpAddressValue = userInput;
                    SetIpBtn.Text = $"IP Address: {IpAddressValue}";
                }

                await StorageHelper.SetAsync(Constants.StorageKeys.IpAddress, userInput);

                if (!string.Equals(currentIpValue, IpAddressValue, StringComparison.OrdinalIgnoreCase))
                {
                    await DisplayAlert("Success", $"Ip Address is: {IpAddressValue}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void ScanIpBtn_Clicked(object sender, EventArgs e)
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

        private async void Speed_Loaded(object sender, EventArgs e)
        {
            string? savedSpeedValue = await StorageHelper.GetAsync(Constants.StorageKeys.Speed);
            if (!string.IsNullOrEmpty(savedSpeedValue) && double.TryParse(savedSpeedValue, out double savedValue))
            {
                speed.Value = savedValue;
            }
            else
            {
                speed.Value = 4;
            }
        }

        private async void SosBtn_Clicked(object sender, EventArgs e)
        {
            await SendCommand(SOS);
        }

        private void Speed_DragStarted(object sender, EventArgs e)
        {
            Slider slider = (Slider)sender;
            if (slider is not null)
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

        private void SetIpBtn_Loaded(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(IpAddressValue))
            {
                SetIpBtn.Text = $"Ip Address: {IpAddressValue}";
            }
        }
    }
}
