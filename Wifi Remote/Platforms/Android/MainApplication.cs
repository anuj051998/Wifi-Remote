﻿using Android.App;
using Android.Content.Res;
using Android.Runtime;

namespace Wifi_Remote
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {

        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
