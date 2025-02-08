namespace Wifi_Remote.Helper
{
    using System.Net;
    using System.Net.Sockets;

    public static class NetworkHelper
    {
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4 Address
                {
                    return ip.ToString();
                }
            }
            return "No network adapters with an IPv4 address in the system!";
        }
    }

}
