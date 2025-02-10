using Android.App;
using Android.Content.Res;
using Android.Runtime;

namespace Wifi_Remote
{
    [Application(UsesCleartextTraffic = true)]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {

        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
