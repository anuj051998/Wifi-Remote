namespace Wifi_Remote.Helper
{
    public class CommandList
    {
        public static readonly IReadOnlyDictionary<string, int> SpeedValues = new Dictionary<string, int>()
        {
            { "0", 60 },
            {"1", 70},
            {"2", 81},
            {"3", 95},
            {"4", 105},
            {"5", 122},
            {"6", 150},
            {"7", 196},
            {"8", 272},
            {"9", 400},
            {"10", 1023}
        };

        public const string FORWARD = "F";
        public const string BACKWARD = "B";
        public const string LEFT = "L";
        public const string RIGHT = "R";
        public const string FORWARD_LEFT = "G";
        public const string FORWARD_RIGHT = "I";
        public const string BACKWARD_LEFT = "H";
        public const string BACKWARD_RIGHT = "J";
        public const string LIGHT_ON = "W";
        public const string LIGHT_OFF = "w";
        public const string STOP = "S";
        public const string SOS = "SOS";
    }
}
