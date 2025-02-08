using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Wifi_Remote.Helper
{
    public class DeviceSelectedMessage : ValueChangedMessage<string>
    {
        public DeviceSelectedMessage(string value) : base(value) { }
    }

}
