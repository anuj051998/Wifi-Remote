using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using Wifi_Remote.Helper;

namespace Wifi_Remote
{

    public partial class DeviceListPage : ContentPage
    {
        public ObservableCollection<string> Devices { get; set; } = new ObservableCollection<string>();
        public bool IsScanning { get; set; } = true;

        public DeviceListPage()
        {
            InitializeComponent();
            BindingContext = this;
            LoadDevices();
        }

        private async void LoadDevices()
        {
            var scannedDevices = await NetworkScanner.GetConnectedDevicesAsync();
            foreach (var device in scannedDevices)
            {
                Devices.Add(device);
            }

            IsScanning = false;
            OnPropertyChanged(nameof(IsScanning));

            // Handle item click event
            DeviceListView.ItemSelected += (sender, e) =>
            {
                if (e.SelectedItem != null)
                {
                    // Send the selected device to MainPage
                    WeakReferenceMessenger.Default.Send(new DeviceSelectedMessage(e.SelectedItem.ToString()));

                    // Close the modal
                    Navigation.PopModalAsync();
                }
            };
        }

        private async void OnCloseClicked(object sender, EventArgs e)
        {
            // Close the modal when button is clicked
            await Navigation.PopModalAsync();
        }

    }

}
