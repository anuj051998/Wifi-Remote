using System.Net.NetworkInformation;

namespace Wifi_Remote.Helper
{
    public static class NetworkScanner
    {
        public static async Task<List<string>> GetConnectedDevicesAsync()
        {
            List<string> connectedDevices = new List<string>();

            string localIP = NetworkHelper.GetLocalIPAddress();
            if (string.IsNullOrEmpty(localIP)) return connectedDevices;

            string baseIP = localIP.Substring(0, localIP.LastIndexOf('.') + 1); // Example: "192.168.1."

            List<Task> tasks = new List<Task>();
            for (int i = 1; i < 255; i++) // Scan 192.168.1.1 to 192.168.1.254
            {
                string ip = baseIP + i;
                tasks.Add(Task.Run(async () =>
                {
                    if (await PingHost(ip))
                    {
                        lock (connectedDevices)
                        {
                            connectedDevices.Add(ip);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return connectedDevices;
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
