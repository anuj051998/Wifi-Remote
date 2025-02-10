using System.Net.NetworkInformation;

namespace Wifi_Remote.Helper
{
    public static class NetworkScanner
    {
        public static async Task<List<string>> GetConnectedDevicesAsync()
        {
            var devices = new List<string>();
            string subnet = "192.168.1."; // Define the subnet of your local network
            int timeout = 1000; // Timeout for ping requests (1 second)

            // Scan the range of IP addresses from 1 to 254
            for (int i = 1; i <= 254; i++)
            {
                string ipAddress = subnet + i;
                var ping = new Ping();

                try
                {
                    // Send a ping to the current IP address
                    var reply = await ping.SendPingAsync(ipAddress, timeout);

                    // If the reply is successful, add it to the devices list
                    if (reply.Status == IPStatus.Success)
                    {
                        devices.Add(ipAddress);
                    }
                }
                catch (PingException)
                {
                    // Ignore if the ping request fails (e.g., unreachable device)
                }
            }

            return devices;
        }
        public static async Task<List<string>> GetValue()
        {
            var ipList = new List<string>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProperties = ni.GetIPProperties();
                    var ipv4Address = ipProperties.UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                    if (ipv4Address != null)
                    {
                        ipList.Add(ipv4Address.Address.ToString());
                    }
                }
            }

            return ipList;
        }

        private static string GetLocalIpAddress()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProperties = ni.GetIPProperties();
                    var ipv4Address = ipProperties.UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                    if (ipv4Address != null)
                    {
                        return ipv4Address.Address.ToString();
                    }
                }
            }

            return null;
        }

        private static async Task<bool> PingHost(string ipAddress)
        {
            try
            {
                using Ping ping = new Ping();
                PingReply reply = await ping.SendPingAsync(ipAddress, 100);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }

}
